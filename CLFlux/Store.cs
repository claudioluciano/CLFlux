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
        public Dictionary<string, IState> _State { get; private set; }
        public Dictionary<string, IGetters> _Getters { get; private set; }
        public Dictionary<string, IMutations> _Mutations { get; private set; }
        public Dictionary<string, IActions> _Actions { get; private set; }

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

        public virtual void Commit(string Key, string Mutation, object Payload = null)
        {
            if (!_Mutations.ContainsKey(Key))
                return;

            var mutation = _Mutations[Key];

            var state = _State[Key];

            var mutationType = mutation.GetType();

            var method = mutationType.GetMethod(Mutation);

            method.Invoke(mutation, new object[] { state, Payload });
        }

        public virtual T Getters<T>(string Key, string Getter)
        {
            if (!_Getters.ContainsKey(Key))
                return default(T);

            var getters = _Getters[Key];

            var state = _State[Key];

            var gettersType = getters.GetType();

            var method = gettersType.GetMethod(Getter);

            Action<string> gettersAction = (Getters) => this.Getters<T>(Key, Getters);

            var parameters = GetParameters(method, ("STATE", state), ("GETTERS", gettersAction));

            return (T)method.Invoke(getters, parameters);
        }

        public async virtual Task<T> Dispatch<T>(string Key, string Actions, object Payload = null)
        {
            if (!_Actions.ContainsKey(Key))
                return default(T);

            var actions = _Actions[Key];

            var state = _State[Key];

            var actionsType = actions.GetType();

            var method = actionsType.GetMethod(Actions);

            Action<string, T> commit = (Mutation, PayloadMutation) => this.Commit(Key, Mutation, PayloadMutation);

            Action<string> getters = (Getters) => this.Getters<T>(Key, Getters);

            Action<string, T> dispatch = async (ActionDispatch, PayloadDispatch) => await this.Dispatch<T>(Key, ActionDispatch, PayloadDispatch);

            var parameters = GetParameters(method, ("STATE", state), ("COMMIT", commit), ("GETTERS", getters), ("DISPATCH", dispatch));
            var task = (Task<T>)method.Invoke(actions, parameters);
            return await task;
        }

        private object[] GetParameters(MethodInfo methodInfo, params (string Key, object Value)[] paramns)
        {
            var parametersNames = methodInfo.GetParameters().Select(x => x.Name.ToUpper());

            return paramns.Where(p => parametersNames.Contains(p.Key)).Select(v => v.Value).ToArray();
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
