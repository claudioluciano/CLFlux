using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        public Store Register(params (string Key, IState Value)[] collection)
        {
            foreach (var (Key, Value) in collection)
                if (!_State.ContainsKey(Key))
                    _State.Add(Key, Value);

            return this;
        }

        public Store Register(params (string Key, IGetters Value)[] collection)
        {
            foreach (var (Key, Value) in collection)
                if (!_Getters.ContainsKey(Key))
                    _Getters.Add(Key, Value);

            return this;
        }

        public Store Register(params (string Key, IMutations Value)[] collection)
        {
            foreach (var (Key, Value) in collection)
                if (!_Mutations.ContainsKey(Key))
                    _Mutations.Add(Key, Value);

            return this;

        }

        public Store Register(params (string Key, IActions Value)[] collection)
        {
            foreach (var (Key, Value) in collection)
                if (!_Actions.ContainsKey(Key))
                    _Actions.Add(Key, Value);

            return this;
        }

        public virtual void Commit(string Key, string MutationName, object Payload = null)
        {
            if (!_Mutations.ContainsKey(Key))
                return;

            var mutation = _Mutations[Key];

            var state = _State[Key];

            var mutationType = mutation.GetType();

            var method = mutationType.GetMethod(MutationName);

            if (method == null)
                throw new NotImplementedException($"The method { MutationName } as not implemented");

            method.Invoke(mutation, Payload == null ? new object[] { state } : new[] { state, Payload });
        }

        public virtual T Getters<T>(string Key, string GetterName)
        {
            if (!_Getters.ContainsKey(Key))
                return default(T);

            var getter = _Getters[Key];

            var state = _State[Key];

            var gettersType = getter.GetType();

            var method = gettersType.GetMethod(GetterName);

            CLDelegate.CLGetters<T> clGetters = (getterName, key) => this.Getters<T>(getterName, key == "" ? Key : key);

            var parameters = GetParameters(method, ("STATE", state), ("CLGETTERS", clGetters));

            if (method == null)
                throw new NotImplementedException($"The method { GetterName } as not implemented");

            return (T)method.Invoke(getter, parameters);
        }

        public virtual async Task<T> Dispatch<T>(string Key, string ActionName, object Payload = null)
        {
            if (!_Actions.ContainsKey(Key))
                return default(T);

            var actions = _Actions[Key];

            var state = _State[Key];

            var actionsType = actions.GetType();

            var method = actionsType.GetMethod(ActionName);

            CLDelegate.CLCommit<T> commit = (mutationName, payloadMutation, key) => this.Commit(key == "" ? Key : key, mutationName, payloadMutation);

            CLDelegate.CLGetters<T> clGetters = (getterName, key) => this.Getters<T>(key == "" ? Key : key, getterName);

            CLDelegate.CLDispatch<T> dispatch = async (actionName, payloadDispatch, key) => await this.Dispatch<T>(key == "" ? Key : key, actionName, payloadDispatch);

            var parameters = GetParameters(method, ("STATE", state), ("COMMIT", commit), ("CLGETTERS", clGetters), ("DISPATCH", dispatch));

            if (method == null)
                throw new NotImplementedException($"The method { ActionName } as not implemented");

            var task = (Task<T>)method.Invoke(actions, parameters);
            return await task;
        }

        private object[] GetParameters(MethodInfo methodInfo, params (string Key, object Value)[] paramns)
        {
            var parametersNames = methodInfo.GetParameters().Select(x => x.Name.ToUpper()).ToArray();

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
        }

        /// <summary>
        /// Gets property information for the specified <paramref name="property"/> expression.
        /// </summary>
        /// <typeparam name="TSource">Type of the parameter in the <paramref name="property"/> expression.</typeparam>
        /// <typeparam name="TValue">Type of the property's value.</typeparam>
        /// <param name="property">The expression from which to retrieve the property information.</param>
        /// <returns>Property information for the specified expression.</returns>
        /// <exception cref="ArgumentException">The expression is not understood.</exception>
        private PropertyInfo GetPropertyInfo<TSource, TValue>(Expression<Func<TSource, TValue>> property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            if (!(property.Body is MemberExpression body))
                throw new ArgumentException("Expression is not a property", "property");

            var propertyInfo = body.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Expression is not a property", "property");

            return propertyInfo;
        }
    }

}
