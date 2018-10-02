using System.Threading.Tasks;

namespace CLFlux
{
    public class CLDelegate
    {
        public delegate TReturn CLGetters<TReturn>(string GetterName, string Key = Constraints.DefaultKey);

        public delegate void CLCommit<TPayload>(string MutationName, TPayload PayloadMutation, string Key = Constraints.DefaultKey);

        public delegate void CLCommit(string MutationName, string Key = Constraints.DefaultKey);

        public delegate Task<TReturn> CLDispatch<TReturn, TPayload>(string ActionName, TPayload PayloadAction, string Key = Constraints.DefaultKey);

        public delegate Task<TReturn> CLDispatch<TReturn>(string ActionName, string Key = Constraints.DefaultKey);
    }
}
