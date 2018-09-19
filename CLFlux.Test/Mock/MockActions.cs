using System;
using System.Threading.Tasks;

namespace CLFlux.Test.Mock
{
    public class MockActions : IActions
    {
        public async Task<int> Increment(CLDelegate.CLCommit<int> commit, MockState state, CLDelegate.CLGetters<int> clGetters)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            var test = clGetters("GetValue");

            commit("Increment", 15);

            return state.Value;
        }
    }
}
