namespace CLFlux.Test.Mock
{
    public class MockGetters : IGetters
    {
        public int GetValue(MockState state)
        {
            return state.Value;
        }
    }
}
