using System;
using System.Threading.Tasks;

namespace CLFlux.Test.Mock
{
    public class MockActions : IActions
    {
        public async Task Increment(CLDelegate.CLDispatch<int, int> clDispatch,
                                    CLDelegate.CLCommit<int> commit,
                                    MockState state,
                                    CLDelegate.CLGetters<int> clGetters,
                                    int payload)
        {
            await Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(async a =>
            {
                var test = clGetters("GetValue");

                state.Value += test;

                clDispatch("Decrement", 15);

                commit("Increment", 15);
            });
        }

        public void Decrement(MockState state, int payload)
        {
            state.Value -= payload;
        }
    }
}
