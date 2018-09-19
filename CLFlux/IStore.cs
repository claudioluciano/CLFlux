using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CLFlux
{
    public interface IStore
    {
        void Commit(string Key, string Mutation, object Payload);

        T Getters<T>(string Key, string Getter);

        Task<T> Dispatch<T>(string Key, string Actions, object Payload = null);

        void WhenAny<T, TProperty>(string Key, Action<object> action, Expression<Func<T, TProperty>> property) where T : INotifyPropertyChanged;

        Store Register(params (string Key, IState Value)[] collection);

        Store Register(params (string Key, IGetters Value)[] collection);

        Store Register(params (string Key, IMutations Value)[] collection);

        Store Register(params (string Key, IActions Value)[] collection);
    }
}
