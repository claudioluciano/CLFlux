using System;
using System.Threading.Tasks;

namespace CLFlux.Test.Mock
{
    public class MockActions : IActions
    {
        public async void Increment(CLDelegate.CLDispatch<int> clDispatch, CLDelegate.CLCommit<int> commit, MockState state, CLDelegate.CLGetters clGetters)
        {
            await Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(a =>
            {
                var test = clGetters("Teste", "GetValue");

                commit("Teste", "Increment", 15);
            });
        }
    }
}
