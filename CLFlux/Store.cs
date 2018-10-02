using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Store Register(IState Value, string Key = Constraints.DefaultKey)
        {
            if (!_State.ContainsKey(Key))
                _State.Add(Key, Value);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Store Register(IGetters Value, string Key = Constraints.DefaultKey)
        {
            if (!_Getters.ContainsKey(Key))
                _Getters.Add(Key, Value);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Store Register(IMutations Value, string Key = Constraints.DefaultKey)
        {
            if (!_Mutations.ContainsKey(Key))
                _Mutations.Add(Key, Value);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Store Register(IActions Value, string Key = Constraints.DefaultKey)
        {
            if (!_Actions.ContainsKey(Key))
                _Actions.Add(Key, Value);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Commit<TPayload>(string MutationName, TPayload Payload, string Key = Constraints.DefaultKey)
        {
            if (!_Mutations.ContainsKey(Key))
                return;

            var (method, mutation, parameters) = GetCommit(Key, MutationName, Payload);

            if (method == null)
                return;

            method.Invoke(mutation, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Commit(string MutationName, string Key = Constraints.DefaultKey)
        {
            if (!_Mutations.ContainsKey(Key))
                return;

            var (method, mutation, parameters) = GetCommit(Key, MutationName);

            if (method == null)
                return;

            var state = _State[Key];

            method.Invoke(mutation, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (MethodInfo, IMutations, object[]) GetCommit(string Key, string MutationName, object Payload = null)
        {
            var mutation = _Mutations[Key];

            var state = _State[Key];

            var mutationType = mutation.GetType();

            var method = mutationType.GetMethod(MutationName);

            var parameters = this.Parameters(method, state, Payload);

            if (method == null)
                throw new NotImplementedException($"The method { MutationName } as not implemented");

            return (method, mutation, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual TReturn Getters<TReturn>(string GetterName, string Key = Constraints.DefaultKey)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async Task<TReturn> Dispatch<TReturn, TPayload>(string ActionName, TPayload Payload, string Key = Constraints.DefaultKey)
        {
            if (!_Actions.ContainsKey(Key))
                return default(TReturn);

            var (method, actions, parameters) = this.GetDispatch(Key, ActionName, Payload);

            if (method == null)
                throw new NotImplementedException($"The method { ActionName } as not implemented");

            var task = method.Invoke(actions, parameters);

            if (task == null)
                return default(TReturn);

            if (task is Task<object>)
                return await (task as Task<TReturn>);
            else
            {
                await (task as Task);
                return default(TReturn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async Task<TReturn> Dispatch<TReturn>(string ActionName, string Key = Constraints.DefaultKey)
        {
            if (!_Actions.ContainsKey(Key))
                return default(TReturn);

            var (method, actions, parameters) = this.GetDispatch(Key, ActionName);

            if (method == null)
                throw new NotImplementedException($"The method { ActionName } as not implemented");

            var task = method.Invoke(actions, parameters);

            if (task == null)
                return default(TReturn);

            if (task is Task<object>)
                return await (task as Task<TReturn>);
            else
            {
                await (task as Task);
                return default(TReturn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (MethodInfo, IActions, object[]) GetDispatch(string Key, string ActionName, object Payload = null)
        {
            var actions = _Actions[Key];

            var state = _State[Key];

            var actionsType = actions.GetType();

            var method = actionsType.GetMethod(ActionName);

            var parameters = this.Parameters(method, state, Payload);

            return (method, actions, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object[] Parameters(MethodInfo method, IState state, object Payload = null)
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

                listParamns.Add((name, GenerateDelegate(DelegateType.Getters, type)));
            }

            if (parametersNames.Contains("CLCOMMIT"))
            {
                var (name, type) = parametersFromMethod.First(x => x.name == "CLCOMMIT");

                listParamns.Add((name, GenerateDelegate(DelegateType.Commit, type)));
            }

            if (parametersNames.Contains("CLDISPATCH"))
            {
                var (name, type) = parametersFromMethod.First(x => x.name == "CLDISPATCH");

                listParamns.Add((name, GenerateDelegate(DelegateType.Dispatch, type)));
            }

            var delegates = new[] { "CLGETTERS", "CLCOMMIT", "CLDISPATCH" };

            if (Payload != null && parametersNames.Any(x => !delegates.Contains(x))
                                && parametersFromMethod.Any(x => !typeof(IState).IsAssignableFrom(x.type)))
            {
                var (name, type) = parametersFromMethod.FirstOrDefault(x =>
                                    !delegates.Contains(x.name) &&
                                    !typeof(IState).IsAssignableFrom(x.type));
                if (name == null)
                    throw new NotImplementedException($"The method { method.Name } has no payload");

                listParamns.Add((name, Payload));
            }


            (string name, Type type)[] GetParametersFromMethod(MethodInfo methodInfo)
            {
                var parameters = methodInfo.GetParameters()
                    .Select(x => (name: GetName(x.ParameterType.Name.ToUpper()), type: x.ParameterType)).ToArray();

                return parameters;
            }

            object[] SortParamns((string Key, object Value)[] paramns)
            {
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

            return SortParamns(listParamns.ToArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Delegate GenerateDelegate(DelegateType typeDelegate, Type actionType)
        {
            Type delegateType = null;
            MethodInfo method = null;

            switch (typeDelegate)
            {
                case DelegateType.Getters:

                    delegateType = typeof(CLDelegate.CLGetters<>).MakeGenericType(actionType.GenericTypeArguments);

                    method = this.GetType().GetMethod("Getters").MakeGenericMethod(actionType.GenericTypeArguments);

                    break;
                case DelegateType.Commit:
                    if (actionType.GenericTypeArguments.Length == 1)
                    {
                        delegateType = typeof(CLDelegate.CLCommit<>).MakeGenericType(actionType.GenericTypeArguments);

                        method = this.GetType().GetMethods().First(x =>
                         x.Name == "Commit" && x.GetGenericArguments().Count() == 1
                        ).MakeGenericMethod(actionType.GenericTypeArguments);
                    }
                    else
                    {
                        delegateType = typeof(CLDelegate.CLCommit);

                        method = this.GetType().GetMethod("Commit");
                    }

                    break;
                case DelegateType.Dispatch:


                    if (actionType.GenericTypeArguments.Length == 1)
                    {
                        delegateType = typeof(CLDelegate.CLDispatch<>).MakeGenericType(actionType.GenericTypeArguments);

                        method = this.GetType().GetMethods().First(x =>
                            x.Name == "Dispatch" && x.GetGenericArguments().Count() == 1)
                            .MakeGenericMethod(actionType.GenericTypeArguments);
                    }
                    else
                    {
                        delegateType = typeof(CLDelegate.CLDispatch<,>).MakeGenericType(actionType.GenericTypeArguments);

                        method = this.GetType().GetMethods()
                            .First(x => x.Name == "Dispatch" && x.GetGenericArguments().Count() == 2)
                            .MakeGenericMethod(actionType.GenericTypeArguments);
                    }

                    break;
            }

            //// work out concrete type for calling the generic method
            //var dispatchMethodInfo = this.GetType().GetMethod(method).MakeGenericMethod(delegateType.GenericTypeArguments);

            // create an instance of the delegate type wrapping MyMethod so we can pass it to the constructor
            var delegateInstance = Delegate.CreateDelegate(delegateType, this, method);

            return delegateInstance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetName(string name)
        {
            return Regex.Replace(name, "[^a-zA-Z]", "");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WhenAny<T, TProperty>(Expression<Func<T, TProperty>> property, Action<object> action, string Key = Constraints.DefaultKey) where T : INotifyPropertyChanged
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
