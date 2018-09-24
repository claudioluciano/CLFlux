# CLFlux

CLFlux is a state management pattern based for .NET on [Vuex](https://github.com/vuejs/vuex).
You can use on Xamarin, WPF, WinForms e etc...

[![nuget](https://img.shields.io/badge/nuget-download-blue.svg)](https://www.nuget.org/packages/CLFlux/)

------------

#### Actions
```csharp
    public class MyActions : IActions
    {
        public async Task<object> Increment(MyState state, CLDelegate.CLCommit<int> commit)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            commit("Increment", 15);

            return state.Value;
        }
    }
```
~~The parameters for the methods need to be in order : **state, commit, getters, dispatch**.~~
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
You can have any type on state, (like List or Array of two dimensions)


#### Store
```csharp
IStore store = new Store();

IState state = new MyState();

IGetters getters = new MyGetters();

IMutations mutation = new MyMutation();

IActions actions = new MyActions();

store.Register(("App", state))
	 .Register(("App", getters))
	 .Register(("App", mutation))
	 .Register(("App", actions));

```


    store.Register(("App", state))
The "App" in *store.Register(("App", state))* is the key for the module, you can set multiples modules, just change the Key.

##### The Delegate actions for Actions and Getters
```csharp
   public delegate object CLGetters(string Key, string GetterName);

   public delegate void CLCommit<TPayload>(string key, string mutationName, TPayload payloadMutation);

   public delegate Task<object> CLDispatch<TPayload>(string key, string actionName, TPayload payloadAction);
```
##### The key is optional if you leave it blank the method will use the same key as the current module


Getters can have State and Another Getters like this

```csharp
public int GetValue(MockState state, CLDelegate.CLGetters clGetters)
{
	var val = clGetters("MyAwesomeKey", "AnotherGetValue");
}
```

Actions can have State, Mutations,  Getters and Another Actions like this

```csharp
public async Task<int> Increment(CLDelegate.CLCommit<int> commit, MockState state, CLDelegate.CLGetters clGetters)
{
	await Task.Delay(TimeSpan.FromSeconds(5));
 
	var test = clGetters("MyAwesomeKey", "GetValue");
 
	commit("MyAwesomeKey", "Increment", 15);
 
	return state.Value;
}
```

#### WhenAny
You can track a property with WhenAny like this.
```csharp
store.WhenAny<MockState, int>("Teste", HandleValueChanged, x => x.Value);

void HandleValueChanged(object value)
{
    //handle the property changed, nice
}
```

#### Usage
```csharp
IStore store = new Store();

IState state = new MyState();

IGetters getters = new MyGetters();

IMutations mutation = new MyMutation();

IActions actions = new MyActions();
 
store.Register("App", state)
     .Register("App", getters)
     .Register("App", mutation)
     .Register("App", actions);   

 //Increment mutation
store.Commit("App", "Increment", 50);
 
 //Increment async Action with fixed value
var ret1 = await store.Dispatch<int>("App", "Increment");

 //GetValue Getters
var ret = store.Getters<int>("App", "GetValue");
```


**For more usage please see the unit test on the project**
