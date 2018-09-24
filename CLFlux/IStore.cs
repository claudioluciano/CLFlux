using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CLFlux
{
    public interface IStore
    {
        void Commit<T>(string Key, string Mutation, T Payload = default(T));

        object Getters(string Key, string Getter);

        Task<object> Dispatch<T>(string Key, string Actions, T Payload = default(T));

        void WhenAny<T, TProperty>(string Key, Action<object> action, Expression<Func<T, TProperty>> property) where T : INotifyPropertyChanged;

        Store Register(string Key, IState Value);

        Store Register(string Key, IGetters Value);

        Store Register(string Key, IMutations Value);

        Store Register(string Key, IActions Value);
    }
}
