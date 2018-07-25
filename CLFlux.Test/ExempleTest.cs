using CLFlux.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLFlux.Test
{
    [TestClass]

    public class ExempleTest
    {
        [TestMethod]
        public void MutationExemple()
        {
            var store = new Store();

            var state = new MockState();

            var mutation = new MockMutation();

            store.Register(("Teste", state)).
                  Register(("Teste", mutation));

            store.Commit("Teste", "Increment", 50);


            Assert.AreEqual(50, state.Value);
        }

        [TestMethod]
        public void GettersExemple()
        {
            var store = new Store();

            var state = new MockState();

            var getters = new MockGetters();

            var mutation = new MockMutation();

            store.Register(("Teste", state))
                 .Register(("Teste", getters))
                 .Register(("Teste", mutation));

            store.Commit("Teste", "Increment", 50);

            var ret = store.Getters("Teste", "GetValue");

            Assert.AreEqual(50, ret);
        }

        [TestMethod]
        public async Task ActionExemple()
        {
            var store = new Store();

            var state = new MockState();

            var getters = new MockGetters();

            var mutation = new MockMutation();

            var actions = new MockActions();

            store.Register(("Teste", state))
                 .Register(("Teste", getters))
                 .Register(("Teste", mutation))
                 .Register(("Teste", actions));

            store.Commit("Teste", "Increment", 50);

            var ret1 = await store.Dispatch("Teste", "Increment");

            var ret2 = await store.Dispatch("Teste", "Increment");

            var ret = store.Getters("Teste", "GetValue");

            Assert.AreEqual(80, ret);
        }

    }
}
