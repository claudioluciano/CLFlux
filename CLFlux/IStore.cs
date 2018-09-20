using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CLFlux
{
    public interface IStore
    {
        void Commit(string Key, string Mutation, object Payload = null);

        T Getters<T>(string Key, string Getter);

        Task<T> Dispatch<T>(string Key, string Actions, object Payload = null);

        void WhenAny<T, TProperty>(string Key, Action<object> action, Expression<Func<T, TProperty>> property) where T : INotifyPropertyChanged;

        Store Register(string Key, IState Value);

        Store Register(string Key, IGetters Value);

        Store Register(string Key, IMutations Value);

        Store Register(string Key, IActions Value);
    }
}
