using System;
using System.Threading.Tasks;

namespace CLFlux.Test.Mock
{
    public class MockActions : IActions
    {
        public async Task<object> Increment(MockState state, Action<string, object> commit)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            commit("Increment", 15);

            return state.Value;
        }
    }
}
