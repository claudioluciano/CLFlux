using System.Threading.Tasks;

namespace CLFlux
{
    public class CLDelegate
    {
        public delegate T CLGetters<T>(string getterName, string key = "");

        public delegate void CLCommit<T>(string mutationName, T payloadMutation, string key = "");

        public delegate Task<T> CLDispatch<T>(string actionName, T payloadAction, string key = "");
    }
}
