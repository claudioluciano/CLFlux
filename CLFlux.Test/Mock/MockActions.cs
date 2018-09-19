using System;
using System.Threading.Tasks;

namespace CLFlux.Test.Mock
{
    public class MockActions : IActions
    {
        public async Task<int> Increment(Action<string, int> commit, MockState state, Action<string> getters)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            commit("Increment", 15);

            return state.Value;
        }
    }
}
