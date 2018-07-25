# CLFlux

CLFlux is a state management pattern based on [Vuex](https://vuex.vuejs.org).

------------

#### Actions
```csharp
    public class MyActions : IActions
    {
        public async Task<object> Increment(MyState state, Action<string, object> commit)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            commit("Increment", 15);

            return state.Value;
        }
    }
```
The parameters for the methods need to be in order : **state, commit, getters, dispatch**.
Actions can be asynchronous or synchronous.

#### Getters
```csharp
    public class MyGetters : IGetters
    {
        public int GetValue(MyState state)
        {
            return state.Value;
        }
    }

```
#### Mutations
```csharp
    public class MyMutation : IMutations
    {
        public void Increment(MyState state, int payload)
        {
            state.Value += payload;
        }
    }
```
#### State
```csharp
    public class MyState : IState
    {
        public int Value { get; set; }
    }
```
You can set any property on state.


#### Store
```csharp
var store = new Store();

var state = new MyState();

var getters = new MyGetters();

var mutation = new MyMutation();

var actions = new MyActions();

store.Register(("App", state))
	.Register(("App", getters))
	.Register(("App", mutation))
	.Register(("App", actions));

```


    store.Register(("App", state))
The "App" is the key for the module, you can set multiple modules, just change the Key.


```csharp
//Increment mutation
store.Commit("App", "Increment", 50);

//Increment Action
var incrementedValue = await store.Dispatch("App", "Increment");

//GetValue Getters
var value = store.Getters("App", "GetValue");
```
