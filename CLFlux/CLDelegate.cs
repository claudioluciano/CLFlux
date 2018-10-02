using System.Threading.Tasks;

namespace CLFlux
{
    public class CLDelegate
    {
        public delegate TReturn CLGetters<TReturn>(string Key, string GetterName);

        public delegate void CLCommit<TPayload>(string Key, string MutationName, TPayload PayloadMutation);

        public delegate void CLCommit(string Key, string MutationName);

        public delegate Task<TReturn> CLDispatch<TReturn, TPayload>(string Key, string ActionName, TPayload PayloadAction);

        public delegate Task<TReturn> CLDispatch<TReturn>(string Key, string ActionName);
    }
}
