using System.Threading.Tasks;

namespace CLFlux
{
    public class CLDelegate
    {
        public delegate object CLGetters(string Key, string GetterName);

        public delegate void CLCommit<TPayload>(string key, string mutationName, TPayload payloadMutation);

        public delegate Task<object> CLDispatch<TPayload>(string key, string actionName, TPayload payloadAction);
    }
}
