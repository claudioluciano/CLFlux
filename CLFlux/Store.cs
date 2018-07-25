using System;
using System.Collections.Generic;
using System.Linq;
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

        public virtual object Getters(string Key, string Getter)
        {
            if (!_Getters.ContainsKey(Key))
                return null;

            var getters = _Getters[Key];

            var state = _State[Key];

            var gettersType = getters.GetType();

            var method = gettersType.GetMethod(Getter);

            Action<string> gettersAction = (Getters) => this.Getters(Key, Getters);

            var parameters = GetParameters(method, ("STATE", state), ("GETTERS", gettersAction));

            return method.Invoke(getters, parameters);
        }

        public async virtual Task<object> Dispatch(string Key, string Actions, object Payload = null)
        {
            if (!_Actions.ContainsKey(Key))
                return null;

            var actions = _Actions[Key];

            var state = _State[Key];

            var actionsType = actions.GetType();

            var method = actionsType.GetMethod(Actions);

            Action<string, object> commit = (Mutation, PayloadMutation) => this.Commit(Key, Mutation, PayloadMutation);

            Action<string> getters = (Getters) => this.Getters(Key, Getters);

            Action<string, object> dispatch = async (ActionDispatch, PayloadDispatch) => await this.Dispatch(Key, ActionDispatch, PayloadDispatch);

            var parameters = GetParameters(method, ("STATE", state), ("COMMIT", commit), ("GETTERS", getters), ("DISPATCH", dispatch));
            var task = (Task<object>)method.Invoke(actions, parameters);
            return await task;
        }

        private object[] GetParameters(System.Reflection.MethodInfo methodInfo, params (string Key, object Value)[] paramns)
        {
            var parametersNames = methodInfo.GetParameters().Select(x => x.Name.ToUpper());

            return paramns.Where(p => parametersNames.Contains(p.Key)).Select(v => v.Value).ToArray();
        }
    }

}
