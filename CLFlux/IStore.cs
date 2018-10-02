using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CLFlux
{
    public interface IStore
    {
        void Commit<TPayload>(string MutationName, TPayload Payload, string Key = Constraints.DefaultKey);

        void Commit(string MutationName, string Key = Constraints.DefaultKey);

        TReturn Getters<TReturn>(string GetterName, string Key = Constraints.DefaultKey);

        Task<TReturn> Dispatch<TReturn>(string ActionName, string Key = Constraints.DefaultKey);

        Task<TReturn> Dispatch<TReturn, TPayload>(string ActionName, TPayload Payload, string Key = Constraints.DefaultKey);

        void WhenAny<T, TProperty>(Expression<Func<T, TProperty>> property, Action<object> action, string Key = Constraints.DefaultKey) where T : INotifyPropertyChanged;

        /// <summary>
        /// Register the state to the store
        /// </summary>
        /// <param name="Value">State</param>
        /// <param name="Key">Key</param>
        /// <returns></returns>
        Store Register(IState Value, string Key = Constraints.DefaultKey);

        /// <summary>
        /// Register the getter to the store
        /// </summary>
        /// <param name="Value">State</param>
        /// <param name="Key">Key</param>
        /// <returns></returns>
        Store Register(IGetters Value, string Key = Constraints.DefaultKey);

        /// <summary>
        /// Register the mutations to the store
        /// </summary>
        /// <param name="Value">State</param>
        /// <param name="Key">Key</param>
        /// <returns></returns>
        Store Register(IMutations Value, string Key = Constraints.DefaultKey);

        /// <summary>
        /// Register the actions to the store
        /// </summary>
        /// <param name="Value">State</param>
        /// <param name="Key">Key</param>
        /// <returns></returns>
        Store Register(IActions Value, string Key = Constraints.DefaultKey);
    }
}
