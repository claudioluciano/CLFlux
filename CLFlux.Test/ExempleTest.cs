using CLFlux.Test.Mock;
using System.Threading.Tasks;
using Xunit;

namespace CLFlux.Test
{
    public class ExempleTest
    {
        [Fact]
        public void MutationExemple()
        {
            var store = new Store();

            var state = new MockState();

            var mutation = new MockMutation();

            store.Register(state).
                  Register(mutation);

            store.Commit("Increment", 50);


            Assert.Equal(50, state.Value);
        }

        [Fact]
        public void GettersExemple()
        {
            IStore store = new Store();

            var state = new MockState();

            var getters = new MockGetters();

            var mutation = new MockMutation();

            store.Register(state)
                 .Register(getters)
                 .Register(mutation);


            store.WhenAny<MockState, int>(x => x.Value, HandleValueChanged);

            store.Commit("Increment", 15);

            var ret = store.Getters<int>("GetValue");

            Assert.Equal(15, ret);
        }

        void HandleValueChanged(object propertyName)
        {
            //handle the property changed, nice
        }

        [Fact]
        public async Task ActionExemple()
        {
            IStore store = new Store();

            var state = new MockState();

            var getters = new MockGetters();

            var mutation = new MockMutation();

            var actions = new MockActions();

            store.Register(state)
                 .Register(getters)
                 .Register(mutation)
                 .Register(actions);

            store.Commit("Increment", 50);

            var ret1 = await store.Dispatch<int, int>("Increment", 15);

            //await store.Dispatch("Teste", "IncrementTeste");

            //var ret2 = await store.Dispatch<int>("Teste", "Increment");

            var ret = store.Getters<int>("GetValue");

            Assert.Equal(100, ret);
        }

    }
}
