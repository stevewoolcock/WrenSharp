<!-- omit in toc -->
# WrenSharp
A neat .NET wrapper for the Wren programming language written in C#.

- A clean, safe C# API for interfacing with Wren
- All Wren API features are exposed, with many quality-of-life additions
- Efficient, non-allocating methods for sending source code to Wren
- Intuitive binding API for Wren foreign classes and methods
- Direct access to the full Wren native API via p/invoke
- Fully customisable integration: Supply your own methods for any of the callbacks Wren supports
- Full supports for the Unity game engine (runs in both Mono and IL2CPP)

For more information on the Wren language, visit:  
[wren.io](https://wren.io/)  
[wren on GitHub](https://github.com/wren-lang/wren)




<!-- omit in toc -->
## Table of Contents
- [Hello World](#hello-world)
- [Slots and Handles](#slots-and-handles)
- [Calling Wren from C#](#calling-wren-from-c)
- [Calling C# from Wren](#calling-c-from-wren)
- [Sharing Data](#sharing-data)
- [Dynamic Functions](#dynamic-functions)
- [Modules](#modules)
- [Custom VM](#custom-vm)
- [Unity Support](#unity-support)
  - [IL2CPP & P/Invoke](#il2cpp--pinvoke)
- [License](#license)




## Hello World
Let's begin with the classics. Here's a simple example of starting up a WrenVM, running some code, and then cleaning up.

```cs
using WrenSharp;

// Create a configuration for the VM that will forward Wren output to System.Console
var output = new WrenConsoleOutput();
var config = new WrenVMConfiguration()
{
    LogErrors = true,
    ErrorOutput = output,
    WriteOutput = output,
};

// Fire up a new Wren VM using the configuration from above
var vm = new WrenSharpVM(config);

// Run some Wren source code!
vm.Interpret(
    module: "main",
    source: "System.print(\"Hello WrenSharp!\")", 
    throwOnFailure: true);

// Output: "Hello WrenSharp!"

// WrenVM implements IDisposable, and can  be used within a using() statement
// to automate disposal. However, in most cases the VM lives longer than a using()
// statement would, so a manual call to Dispose() is required to shut it down and
// free the resources it acquired.
vm.Dispose();
```




## Slots and Handles
<!-- omit in toc -->
### Wren Documentation
- [Slots and Handles](https://wren.io/embedding/slots-and-handles.html)


<!-- omit in toc -->
### Slots
WrenSharp provides full access to Wren's API stack, with all the same methods the native exposes, wrapped by a `WrenVM` instance. Here's a small sample of the methods available.

```cs
// Use EnsureSlotCount() to ensure enough slots are available to perform your instruction
vm.EnsureSlotCount(3);

// Use SetSlot() overloads to add values to the Wren API stack
vm.SetSlot(1, "Melbourne");
vm.SetSlot(2, 1234.5678);
vm.SetSlotNull(3);

// Lists
// - Create a new list in slot 0
// - Append value in slot 1 to list in slot 0
vm.SetSlotNewList(0);    
vm.ListAddElement(0, 1);

// Maps
// - Create a new Map object in slot 0
// - Assign the value in slot 2 to the key in slot 1, to the map in slot 0
vm.SetSlotNewMap(0);
vm.MapSetValue(0, 1, 2);
```


<!-- omit in toc -->
### Handles
Of course, handles are also supported. Handles can be created via several methods:

```cs
// Handles to the value in the API stack (via slot index)
WrenHandle slot0Handle = vm.CreateHandle(0); // Handle from value in slot 0
WrenHandle slot2Handle = vm.CreateHandle(2); // Handle from value in slot 2

// Handles to values held by module-level variables
WrenHandle classHandle = vm.CreateHandle("main", "MyClass");
WrenHandle variableHandle = vm.CreateHandle("main", "myModuleLevelVariable");

// Call handle using Wren method signatures
WrenCallHandle callHandle = vm.CreateCallHandle("saySomething(_)");
```

When a WrenHandle is created, Wren allocates memory to "box" it and ensures that the value will not be garbage collected. It is your responsibility to free handles you create. If you don't free handles, they will stay in memory for as long as the VM runs. This could result in memory leaks if you don't intend to use the handles again in the future.

The `WrenHandle` types implement `IDisposable`, and are freed when `Dispose()` is called. They are compatible with the `using` statement.

```cs
WrenHandle classHandle = vm.CreateHandle("main", "MyClass");

// .. do some work with the handle ..

classHandle.Dispose();


// Within a using statement:
using (WrenHandle slotHandle = vm.CreateHandle(0))
{
    // ... do some work ...
}
```

`WrenVM` instances will automatically release all handles allocated through them when disposed, so there's no need to release handles you intend to keep for the lifetime of the VM.




## Calling Wren from C#
<!-- omit in toc -->
### Wren Documentation
- [Calling Wren from C](https://wren.io/embedding/calling-wren-from-c.html)

Calling a Wren method from your C# code is simple. In this example, a static method is declared on a Wren class, which we will call from C#.

```cs
// Define a class in Wren that we can call into
vm.Interpret(
    module: "main",
    source:
    @"
    class Greeter {
        static greet(message) { System.print(message) }
    }
    ");

// - Create a handle to the "Greeter" class
// - Create a call handle for the "greet(_)" method
//   - greet(message) has one parameter, the underscore denotes this
WrenHandle greeterClass = vm.CreateHandle("main", "Greeter");
WrenCallHandle greetCall = vm.CreateCallHandle("greet(_)");

// Make the call, passing an argument
WrenCall greet = vm.CreateCall(greeterClass, greetCall);
greet.SetArg(0, "Hello Wren!"); // arg 0 (message)
greet.Call();

// Output: "Hello Wren!"

// Release the handles to free the memory Wren allocated for them
// They are invalid once released and cannot be used again
greeterClass.Dispose();
greetCall.Dispose();
```

The `WrenCall` value returned by `WrenVM.CreateCall()` is designed to make working with Wren method calls more intuitive by providing an API for settings the arguments to supply, invoking the call and reading return values. `WrenCall` is a struct, so creating one doesn't allocate any memory on the heap.

OK, so what about calling methods on an _instance_? They're almost exactly the same, except this time the receiver is an _instance_ of a class, so we need to get a handle on one first. Let's make some modifications to the above example that calls `greet()` on an instance instead.

```cs
// - Define a constructor so Greeter can be instantiated
// - Declare greet() as an instance method instead of a static method
// - Create an instance of Greeter and store it in a module level variable
vm.Interpret(
    module: "main",
    source:
    @"
    class Greeter {
        construct new() {}
        greet(msg, name) { System.print(""%(msg), %(name)!"") }
    }

    var theGreeter = Greeter.new()
    ");

// - Create a handle to the "Greeter" instance stored in "theGreeter"
// - Create a call handle for the "greet(_,_)" method
//   - greet() has two parameters this time, so two underscores are required
// - Handles implement IDisposable, so can be used within using() statements
using (WrenHandle theGreeter = vm.CreateHandle("main", "theGreeter"))
using (WrenCallHandle greetCall = vm.CreateCallHandle("greet(_,_)"))
{
    // Make the call, passing arguments
    WrenCall greet = vm.CreateCall(theGreeter, greetCall);
    greet.SetArg(0, "Hello");     // arg 0 (msg)
    greet.SetArg(1, "Alejandro"); // arg 1 (name)
    greet.Call();

    // Output: "Hello, Alejandro!"
}
```




## Calling C# from Wren
<!-- omit in toc -->
### Wren Documentation
- [Calling C from Wren](https://wren.io/embedding/calling-c-from-wren.html)
- [Storing C Data](https://wren.io/embedding/storing-c-data.html)

Here's where things get interesting. You're most likely going to want to implement some functionality in C# that Wren scripts should be able to call into. Wren's "foreign method" concept is fully supported by WrenSharp and is backed by an intuitive builder API that makes creating foreign bindings a breeze.

```cs
// Define a foreign class that we will provide bindings for
var wrenSource =
@"
foreign class MyForeignClass {
    construct new() { }

    foreign myForeignMethod()
}

// Call a function to instantiate a short-lived instance
Fn.new {
    var instance = MyForeignClass.new()
    instance.myForeignMethod()
}.call()

// Run the GC and clean up the instance
System.gc()
";

// Host provides bindings for the foreign class and methods
// This must be done BEFORE interpreting the source defining the class in Wren,
// as the function pointers it creates must be avaialble during interpretation
vm

  // Get a "WrenForeign" for the class we defined above, which provides
  // a builder pattern API for creating foreign bindings
  .Foreign(moduleName: "main", className: "MyForeignClass")
  
  // Allocator
  // Called whenever a new instance of the class is created by Wren
  // Any blittable value type can be supplied as data to a foriegn class
  .Allocate<int>((WrenCallContext ctx, ref int data) =>
  {
    // "ctx" provides an API for easily accessing contextual information
    // about the call being made, including the arguments that were passed

    // Set the foreign method data field to the argument supplied to
    // the constructor in the Wren script
    data = (int)ctx.GetArgDouble(0);
    vm.Print($"MyForeignClass.allocate called, data: {data}");
  })
  
  // Finalizer
  // Called when an instance of the class is collected by the Wren GC
  // Note that finalizers have no access to the VM for safety reasons
  // Only the foreign class data is available here
  .Finalize<int>((ref int data) =>
  {
    vm.Print($"MyForeignClass.finalize called, data: {data}");
  })
  
  // Methods
  // Assign a delegate for each foreign method in the class. These are
  // invoked whenever the methods are called within a Wren script
  .Instance(signature: "myForeignMethod()", ctx =>
  {
    // ctx can retrieve information about the receiver of the call,
    // including the instance's data that was allocated in Allocate() above
    int data = ctx.GetReceiverForeign<int>();
    vm.Print($"MyForeignClass.myForeignMethod called, data: {data}");
  });

// Run the source
vm.Interpret("main", wrenSource);

// Output:
// MyForeignClass.allocate called, data: 1234
// MyForeignClass.myForeignMethod called, data: 1234
// MyForeignClass.finalize called, data: 1234
```




## Sharing Data
Being able to share data between Wren and the host application is important. It is possible to pass data between Wren and WrenSharp using blittable structs and primitive value types, however this can often be tedious and error prone.

WrenSharp provides a simpler way to pass managed data to and from Wren. A `WrenVM` instance comes with a built in `SharedData` table, which is a store for managed objects. Add a managed object to the table and it will be assigned a handle that can be passed around via the WrenSharp API. Internally, the handle is just a 32 bit integer representing the index of the object in the table.

This can be used as the backing type of a foreign class instance. Here's an example that shows how to assign a shared data handle to foreign class instances, and how to return data from the managed object back to Wren.

```cs
// Define the foreign class bindings
vm
.Foreign("main", "IntList")
.Allocate((WrenVM vm, ref WrenSharedDataHandle data) =>
{
    // Create a list and add it to the VM's SharedData table
    // The returned handle is attached to the foreign instance
    var list = new List<int>() { 1, 2, 3, 4, 5 };
    data = vm.SharedData.Add(list);
})
.Instance("count", ctx => 
{
    // The C# object can be retrieved from the receiver
    var list = ctx.GetReceiverSharedData<List<int>>();
    ctx.Return(list.Count);
})
.Instance("[_]", ctx =>
{
    var index = (int)ctx.GetArgDouble(0);
    var list = ctx.GetReceiverSharedData<List<int>>();
    ctx.Return(list[index]);
});

// - Define the IntList class in Wren
// - Create an instance and call its methods
vm.Interpret(
    module: "main",
    source:
    @"
    foreign class IntList {
        construct new() { }
        
        foreign count
        foreign [index]
    }

    var list = IntList.new()
    System.print(list.count)
    System.print(list[1])
    ");

// Output: 5
// Output: 2
```

This example could easily be extended with methods to manipulate the list. Try adding foreign method bindings for `add(item)` and `remove(item)`.

When you no longer need to share a managed object with Wren, it needs to be removed from the VM's shared data table to ensure it becomes eligable for garbage collection.

```cs
WrenSharedDataHandle handle;

// .. Add and object, acquire handle and do some work ..

// Remove objects from the table via handles
vm.SharedData.Remove(handle);
```



## Dynamic Functions
WrenSharp provides an API for generating Wren functions on the fly that can be cached and called at any time. You supply an argument signature and function body, and in turn receive a `WrenHandle` wrapping the compiled function. The function can then be called from anywhere, at anytime.

Here's an example:
```cs
// Define a function, which returns a WrenHandle value.
// The WrenHandle value can be stored and called again at any point in the future.
// The function lives as long as the handle remains allocated.
WrenHandle dynamicFn = vm.CreateFunction(
    module: "main",                 // Compile in "main" module
    paramsSignature: "active, num", // Function has 2 parameters
    functionBody: @"
        System.print(""args: active=%(active), num=%(num)"")
        return num * 2
    ");

// If the function did not compile, the WrenHandle will be not be valid.
// CreateFunction() can also be instructed to throw a WrenInterpretException.
if (dynamicFn.IsValid)
{
    WrenCall call = dynamicFn.CreateCall();
    call.SetArg(0, true); // arg 0 (active)
    call.SetArg(1, 1234); // arg 1 (num)
    call.Call(out double returnValue);
    
    Console.WriteLine(returnValue); // Output: 2468
}
    
// WrenHandle implements IDisposable, which will release the underlying WrenHandle
// to the function that was created when Dispose() is called.
dynamicFn.Dispose();

```




## Modules
<!-- omit in toc -->
### Wren Documentation
- [Modularity](https://wren.io/modularity.html)

Wren supports importing modules into scripts via the `import` statement. _How_ modules are loaded and supplied to Wren is up to the host application to decide.


<!-- omit in toc -->
### Loading Modules
Modules can be loaded from anywhere: the file system, embedded assets, memory - it's up to the host. WrenSharp provides two interfaces for providing modules to Wren: `IWrenModuleProvider` and `IWrenSource`.

Here's a very simple example that uses the built in `WrenStringSource` to load a module from a managed string.
```cs
public class ModuleLoader : IWrenModuleProvider
{
    public IWrenSource GetModuleSource(WrenVM vm, string moduleName)
    {
        vm.Print($"Loading module: {moduleName}");

        if (moduleName == "beverages")
        {
            return new WrenStringSource(
            @"
            class Coffee {
                construct new() {}
            }
            ");
        }

        return null;
    }

    public void OnModuleLoadComplete(WrenVM vm, string moduleName, IWrenSource source)
    {
        vm.Print($"Module loaded: {moduleName}");

        // This callback gives the host a change to clean up any resources that may
        // have been acquired to load the module.
        source.Dispose();
    }
}

var moduleProvider = new ModuleLoader();
var output = new WrenConsoleOutput();
var config = new WrenVMConfiguration()
{
    LogErrors = true,
    ErrorOutput = output,
    WriteOutput = output,

    // Specify the provider as part of the VM configuration
    ModuleProvider = moduleProvider,
};

var vm = new WrenSharpVM(vm);
vm.Interpret(
    module: "main",
    source:
    @"
    import ""beverages"" for Coffee
    
    var coffee = Coffee.new()
    System.print(coffee)
    ");

// Output: "instance of Coffee"
```


<!-- omit in toc -->
### Resolving Modules
Another useful Wren feature supported by WrenSharp is module name resolution. The `IWrenModuleResolver` interface provides a method for resolving a module name provided by an `import` statement. The resolver is passed the name of the module requesting the import (`importer`) and the module it wants to import (`name`). Together, these can be used to return a new string that uniquely identifies the module.

The resolved module name becomes its identifier going forward. It is passed to the `IWrenModuleProvider`, is reported in stack traces, etc. This is a powerful feature that allows the host to supply, for example, relative imports: different implementations of a module depending on the importing module.

```cs
public class ModuleResolver : IWrenModuleResolver
{
    public string ResolveModule(WrenVM vm, string importer, string name)
    {
        // When the "main" module requests an import for "drinks", point it to
        // the "beverages" module from the previous example
        if (importer == "main" && name == "drinks")
            return "beverages";

        // If null is returned, this indicates the name is unresolvable and
        // Wren will treat it is a runtime error. This can be useful for
        // sandboxing, to allow strict control over the modules that can be
        // imported into user scripts.
        return null;
    }
}

var moduleResolver = new ModuleResolver();
var moduleProvider = new ModuleLoader(); // See previous example
var output = new WrenConsoleOutput();
var config = new WrenVMConfiguration()
{
    LogErrors = true,
    ErrorOutput = output,
    WriteOutput = output,
    ModuleProvider = moduleProvider,

    // Specify the resolver as part of the VM configuration
    ModuleResolver =  moduleResolver,
};

var vm = new WrenSharpVM(vm);
vm.Interpret(
    module: "main",
    source:
    @"
    import ""drinks"" for Coffee
    
    var coffee = Coffee.new()
    System.print(coffee)
    ");

```



## Custom VM
The `WrenVM` base class is extendable. You can derive from it to encapsulate your own set of features and configurations, or you can simply pass a `WrenConfiguration` with delegates matching the native Wren functions. This gives you (most) of the power of WrenSharp, while having the ability to supply your own Wren callbacks to do exactly what you need.

Here's a basic Hello World example that defines a simple output forward to the Console:

```cs
// The "Native" namespace contains the types that map directly
// to their Wren C counterparts, including WrenConfiguration
using WrenSharp.Native;

WrenConfiguration config = WrenConfiguration.InitializeNew();
config.Write = (vmPtr, text) => Console.Write(text);

var vm = new WrenVM(ref config);
vm.Interpret(
    module: "main",
    source: "System.print(\"Hello WrenSharp!\")"
);

// Output: Hello WrenSharp!

vm.Dispose();
```

Or, if you wanted to hide the implementation details from the rest of the application, you can derive a class from `WrenVM`.

```cs
using System;
using WrenSharp.Native;

public class CustomWrenVM : WrenVM
{
    public CustomWrenVM()
    {
        var config = WrenConfiguration.InitializeNew();
        config.Write = OutputWrite;
        config.LoadModule = LoadModule;

        Initialize(ref config);
    }

    private void OutputWrite(IntPtr vmPtr, string text)
    {
        // Forward to console
        Console.Write(text);
    }

    private WrenLoadModuleResult LoadModule(IntPtr vmPtr, string module)
    {
        // ... handle module loading ...
    }
}
```



## Unity Support
WrenSharp enables Wren scripts to be run from C# in the Unity game engine. If you're only interested in Mono builds, `WrenSharpVM` will work out of the box (you would need to supply Unity specific functions for hanlding output, though).

However, as always, IL2CPP throws a spanner in the works. `WrenSharpVM` doesn't work with IL2CPP, as it uses features that IL2CPP does not support: marshalling C function pointers through instance delegates. While they are very convenient in regular C# land where a JIT compiler is available, they are unsupported for AOT compilation via IL2CPP.

A specialized Unity-targeted project is included that provides an IL2CPP compatible VM type: `UnityWrenVM`. It has feature parity with `WrenSharpVM` (well, _almost_), but with some modifications to support IL2CPP.

Here's an example of creating a `WrenVM` within a Unity project.
```cs
var vm = new UnityWrenVM();
vm.Interpret(
    module: "main",
    source: "System.print(\"Hello WrenSharp.Unity!\")"
);

// Output: "Hello WrenSharp.Unity!"
```
Simple. This creates a Wren VM with all the default settings for working within Unity, including supplying a default output handler that will forward Wren output to the Unity console and player log via `Debug.Log()`.


### IL2CPP & P/Invoke

<!-- omit in toc -->
#### **The Problem**
A limitation of IL2CPP is that it can only marshal native function pointers through static methods. In practice, this means that every host function Wren calls is required to be a static C# method, known at compile time. This can be worked around for the more general  function bindings Wren requires, but foreign methods are a bit more tricky.

Wren's foreign methods are nice and simple (just like Wren itself): a C function pointer. The only argument provided to a foreign method call is a pointer to the native WrenVM instance, which gives no contextual information about the call itself.

Using static methods for bindings in this way doesn't _feel_ like C#. The `WrenForeign` API makes working with Wren foreign classes and methods easy but requires some special handling to support IL2CPP.

<!-- omit in toc -->
#### **The Solution**
To support the `WrenForeign` API with IL2CPP, a small modification has been made to the Wren library included with WrenSharp.Unity. This modification wraps the `WrenForeignFn` function pointer assigned to foreign methods in a new struct: `WrenForeignMethodData`, in which an additional `uint16_t` field lives alongside the function pointer. These 16 bits are known as a "symbol".

The symbol is generated by the WrenSharp.Unity `WrenForeign` implementation and is passed to Wren when it binds foreign methods to classes, along with a function pointer to a single, pre-allocated static C# delegate. Whenever Wren calls that foreign method, it invokes the static C# method and passes along an extra argument: the symbol it was supplied with when binding.

The symbol is then used to lookup the appropriate delegate to invoke on the C# side, allowing for a nice alternative to juggling static P/Invoke methods. This introduces a _slightly_ higher memory cost for Wren (an extra 2 bytes for every defined method) and an additional level of indirection on the managed side, but makes for cleaner code that can make full use of the WrenSharp feature set.

<!-- omit in toc -->
#### **An Alternative Solution**
The above solution makes working with foreign methods identical between WrenSharp and WrenSharp.Unity, at the cost of requiring a modification to the Wren native library and _slightly_ worse performance (although the difference is arguably negligable in this context).

If this is undesirable, it is of course entirely possible to create a bare metal `WrenVM` and provide your own foreign binding methods that will be compatible with IL2CPP, at the cost of losing the `WrenForeign` API.




## License
This library is released under the MIT License.
