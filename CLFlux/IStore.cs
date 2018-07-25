using System.Threading.Tasks;

namespace CLFlux
{
    public interface IStore
    {
        void Commit(string Key, string Mutation, object Payload);

        object Getters(string Key, string Getter);

        Task<object> Dispatch(string Key, string Actions, object Payload);

        Store Register(params (string Key, IState Value)[] collection);

        Store Register(params (string Key, IGetters Value)[] collection);

        Store Register(params (string Key, IMutations Value)[] collection);

        Store Register(params (string Key, IActions Value)[] collection);
    }
}
