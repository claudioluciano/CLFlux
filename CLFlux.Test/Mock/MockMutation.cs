namespace CLFlux.Test.Mock
{
    public class MockMutation : IMutations
    {
        public void Increment(ref MockState state, int payload)
        {
            state.Value += payload;
        }
    }
}
