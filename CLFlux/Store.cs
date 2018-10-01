using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CLFlux
{
    public class Store : IStore
    {
        protected Dictionary<string, IState> _State { get; }
        protected Dictionary<string, IGetters> _Getters { get; }
        protected Dictionary<string, IMutations> _Mutations { get; }
        protected Dictionary<string, IActions> _Actions { get; }

        public Store()
        {
            _State = new Dictionary<string, IState>();
            _Getters = new Dictionary<string, IGetters>();
            _Mutations = new Dictionary<string, IMutations>();
            _Actions = new Dictionary<string, IActions>();
        }

        public Store Register(string Key, IState Value)
        {
            if (!_State.ContainsKey(Key))
                _State.Add(Key, Value);

            return this;
        }

        public Store Register(string Key, IGetters Value)
        {
            if (!_Getters.ContainsKey(Key))
                _Getters.Add(Key, Value);

            return this;
        }

        public Store Register(string Key, IMutations Value)
        {
            if (!_Mutations.ContainsKey(Key))
                _Mutations.Add(Key, Value);

            return this;

        }

        public Store Register(string Key, IActions Value)
        {
            if (!_Actions.ContainsKey(Key))
                _Actions.Add(Key, Value);

            return this;
        }



        public virtual void Commit<T>(string Key, string MutationName, T Payload = default(T))
        {
            if (!_Mutations.ContainsKey(Key))
                return;

            var (method, mutation) = GetCommit(Key, MutationName);

            if (method == null)
                return;

            var state = _State[Key];

            method.Invoke(mutation, new object[] { state, Payload });
        }

        public virtual void Commit(string Key, string MutationName)
        {
            if (!_Mutations.ContainsKey(Key))
                return;

            var (method, mutation) = GetCommit(Key, MutationName);

            if (method == null)
                return;

            var state = _State[Key];

            method.Invoke(mutation, new object[] { state });
        }

        private (MethodInfo, IMutations) GetCommit(string Key, string MutationName)
        {
            var mutation = _Mutations[Key];

            var mutationType = mutation.GetType();

            var method = mutationType.GetMethod(MutationName);

            if (method == null)
                throw new NotImplementedException($"The method { MutationName } as not implemented");

            return (method, mutation);
        }



        public virtual TReturn Getters<TReturn>(string Key, string GetterName)
        {
            if (!_Getters.ContainsKey(Key))
                return default(TReturn);

            var getter = _Getters[Key];

            var state = _State[Key];

            var gettersType = getter.GetType();

            var method = gettersType.GetMethod(GetterName);

            var parameters = this.Parameters(method, state);

            if (method == null)
                throw new NotImplementedException($"The method { GetterName } as not implemented");

            return (TReturn)method.Invoke(getter, parameters);
        }


        public virtual async Task<object> Dispatch<T>(string Key, string ActionName, T Payload = default(T))
        {
            if (!_Actions.ContainsKey(Key))
                return default(object);

            var actions = _Actions[Key];

            var state = _State[Key];

            var actionsType = actions.GetType();

            var method = actionsType.GetMethod(ActionName);

            var parameters = this.Parameters(method, state);

            if (method == null)
                throw new NotImplementedException($"The method { ActionName } as not implemented");

            var task = method.Invoke(actions, parameters);

            if (task == null)
                return default(object);

            if (task is Task<object>)
                return await (task as Task<object>);
            else
            {
                await (task as Task);
                return default(object);
            }
        }

        private object[] Parameters(MethodInfo method, IState state)
        {
            var parametersFromMethod = GetParametersFromMethod(method);

            var parametersNames = parametersFromMethod.Select(x => x.name).ToArray();

            var listParamns = new List<(string, object)>();

            if (parametersFromMethod.Any(x => typeof(IState).IsAssignableFrom(x.type)))
            {
                var name = parametersFromMethod.First(x => typeof(IState).IsAssignableFrom(x.type));

                listParamns.Add((name.name, state));
            }

            if (parametersNames.Contains("CLGETTERS"))
            {
                var (name, type) = parametersFromMethod.First(x => x.name == "CLGETTERS");

                listParamns.Add((name, GenerateGettersDelegate(type)));
            }

            if (parametersNames.Contains("CLCOMMIT"))
            {
                var (name, type) = parametersFromMethod.First(x => x.name == "CLCOMMIT");

                listParamns.Add((name, GenerateCommitDelegate(type)));
            }

            if (parametersNames.Contains("CLDISPATCH"))
            {
                var (name, type) = parametersFromMethod.First(x => x.name == "CLDISPATCH");

                listParamns.Add((name, GenerateDispatchDelegate(type)));
            }

            return OrderParamns(method, listParamns.ToArray());
        }

        private Delegate GenerateGettersDelegate(Type gettersType)
        {

            var delegateType = typeof(CLDelegate.CLGetters<>).MakeGenericType(gettersType.GenericTypeArguments);

            // work out concrete type for calling the generic MyMethod
            var dispatchMethodInfo = this.GetType().GetMethod("Getters").MakeGenericMethod(delegateType.GenericTypeArguments);

            // create an instance of the delegate type wrapping MyMethod so we can pass it to the constructor
            var delegateInstance = Delegate.CreateDelegate(delegateType, this, dispatchMethodInfo);

            return delegateInstance;
        }

        private Delegate GenerateCommitDelegate(Type commitType)
        {
            // create the delegate type so we can find the appropriate constructor
            var delegateType = typeof(CLDelegate.CLCommit<>).MakeGenericType(commitType.GenericTypeArguments);

            // work out concrete type for calling the generic MyMethod
            var commitMethodInfo =
                this.GetType().GetMethod("Commit").MakeGenericMethod(commitType.GenericTypeArguments);

            // create an instance of the delegate type wrapping MyMethod so we can pass it to the constructor
            var delegateInstance = Delegate.CreateDelegate(delegateType, this, commitMethodInfo);

            return delegateInstance;
        }

        private Delegate GenerateDispatchDelegate(Type dispatchType)
        {
            var delegateType = typeof(CLDelegate.CLDispatch<>).MakeGenericType(dispatchType.GenericTypeArguments);

            // work out concrete type for calling the generic MyMethod
            var dispatchMethodInfo = this.GetType().GetMethod("Dispatch").MakeGenericMethod(dispatchType.GenericTypeArguments);

            // create an instance of the delegate type wrapping MyMethod so we can pass it to the constructor
            var delegateInstance = Delegate.CreateDelegate(delegateType, this, dispatchMethodInfo);

            return delegateInstance;
        }

        private Delegate GenerateDelegate(DelegateType @delegate)
        {
            Type delegateType = null;
            string method = "";

            switch (@delegate)
            {
                case DelegateType.Getters:
                    delegateType = typeof(CLDelegate.CLGetters<>);
                    method = "Getters";
                    break;
                case DelegateType.Commit:
                    delegateType = typeof(CLDelegate.CLCommit<>);
                    method = "Commit";
                    break;
                case DelegateType.Dispatch:
                    delegateType = typeof(CLDelegate.CLDispatch<>);
                    method = "Dispatch";
                    break;
            }

            // work out concrete type for calling the generic method
            var dispatchMethodInfo = this.GetType().GetMethod(method).MakeGenericMethod(delegateType.GenericTypeArguments);

            // create an instance of the delegate type wrapping MyMethod so we can pass it to the constructor
            var delegateInstance = Delegate.CreateDelegate(delegateType, this, dispatchMethodInfo);

            return delegateInstance;
        }

        private (string name, Type type)[] GetParametersFromMethod(MethodInfo methodInfo)
        {

            var parameters = methodInfo.GetParameters()
                .Select(x => (name: GetName(x.ParameterType.Name.ToUpper()), type: x.ParameterType)).ToArray();

            return parameters;
        }

        private object[] OrderParamns(MethodInfo methodInfo, params (string Key, object Value)[] paramns)
        {
            var parametersNames = methodInfo.GetParameters()
                .Select(x => GetName(x.ParameterType.Name.ToUpper())).ToArray();

            var actionHaveParamns = paramns.Where(p => parametersNames.Contains(p.Key)).ToArray();

            var list = new (string Key, object Value)[actionHaveParamns.Count()];

            for (var i = 0; i < actionHaveParamns.Count(); i++)
            {
                for (var j = 0; j < parametersNames.Count(); j++)
                {
                    if (actionHaveParamns[i].Key != parametersNames[j]) continue;

                    list[j] = actionHaveParamns[i];
                    break;
                }
            }

            return list.Select(x => x.Value).ToArray();
        }

        private string GetName(string name)
        {
            return Regex.Replace(name, "[^a-zA-Z]", "");
        }

        public void WhenAny<T, TProperty>(string Key, Action<object> action, Expression<Func<T, TProperty>> property) where T : INotifyPropertyChanged
        {
            if (!_State.ContainsKey(Key))
                return;

            var state = _State[Key];

            ((T)state).PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (GetPropertyInfo(property).Name == e.PropertyName)
                    action(sender.GetType().GetProperty(e.PropertyName).GetValue(sender));
            };

            /// <summary>
            /// Gets property information for the specified <paramref name="property"/> expression.
            /// </summary>
            /// <typeparam name="TSource">Type of the parameter in the <paramref name="property"/> expression.</typeparam>
            /// <typeparam name="TValue">Type of the property's value.</typeparam>
            /// <param name="property">The expression from which to retrieve the property information.</param>
            /// <returns>Property information for the specified expression.</returns>
            /// <exception cref="ArgumentException">The expression is not understood.</exception>
            PropertyInfo GetPropertyInfo<TSource, TValue>(Expression<Func<TSource, TValue>> propertyToGetInfo)
            {
                if (propertyToGetInfo == null)
                    throw new ArgumentNullException("property");

                if (!(propertyToGetInfo.Body is MemberExpression body))
                    throw new ArgumentException("Expression is not a property", "property");

                var propertyInfo = body.Member as PropertyInfo;
                if (propertyInfo == null)
                    throw new ArgumentException("Expression is not a property", "property");

                return propertyInfo;
            }
        }

        public enum DelegateType
        {
            Getters,
            Commit,
            Dispatch
        }
    }
}
