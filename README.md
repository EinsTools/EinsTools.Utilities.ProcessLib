# ProcessLib

## Description

This library contains functions that ease the process of running external programs from within
a .Net application.

## Usage

You create a new instance of the ExternalApplication class, using the Create method.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue);
```

In this case the application is created with the executable and two command line arguments. You can then use the 
members of the ExternalApplication class to set more parameters, such as the working directory.

```csharp
app = app.In("C:\\MyApp");
```

You can then run the application using the Execute method.

```csharp
var exitCode = await app.Execute();
```

The Execute method returns a Task<int> that represents the exit code of the application.

## ExternalApplication Members

### Create

There are two overloads of the Create method. The first takes the executable name and 
a params array of command line arguments. The second takes an array of strings of the command line arguments.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue);
```

```csharp
var app = ExternalApplication.Create("MyApp.exe", new string[] { "-c", "OptionValue" }, workingDirectory: "C:\\MyApp");
```

### In
In sets the working directory of the application.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .In("C:\\MyApp");
```

### OutputTo
OutputTo sets the action that the application's standard output will be written to.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .OutputTo(Console.Out);
```

### ErrorTo
ErrorTo sets the action that the application's standard error will be written to.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .ErrorTo(Console.Error);
```

### ThrowOnError
ThrowOnError sets a function that will be called with the exit code of the application. If the function returns true,
the execution of the application will succeed. If the function returns false, an exception will be thrown.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .ThrowOnError(n => n < 5);
```

There is an overload of ThrowOnError that takes a range value. If the exit code of the application is within the range,
the execution of the application will succeed. If the exit code is outside the range, an exception will be thrown.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .ThrowOnError(0..5);
```

Please note that the range is inclusive. In the example the exit code must be >= 0 and <= 5.

### AddArguments
AddArguments adds command line arguments to the application.

```csharp
var app = ExternalApplication.Create("MyApp.exe")
    .AddArguments("-d", "OptionValue");
```

### ReplaceArguments
ReplaceArguments replaces the command line arguments of the application.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .AddArguments("-c", "OptionValue1")
    .ReplaceArguments("-d", "OptionValue");
```

In this example, the command line arguments will be "-d", "OptionValue". All arguments added before are replaced.

### Execute
Execute runs the application and returns a Task<int> that represents the exit code of the application.

```csharp
var exitCode = await app.Execute();
```

### ExecuteWithResult

ExecuteWithResult runs the application and returns a Task<ExternalApplicationResult> that represents the result of 
the application, including the exit code and both stdout and stderr output.

```csharp
var result = await app.ExecuteWithResult();
Console.WriteLine($"Exit code: {result.ExitCode}");
Console.WriteLine($"Stdout: {result.Stdout}");
Console.WriteLine($"Stderr: {result.Stderr}");
```

### WithWindowStyle

WithWindowStyle sets the window style of the application. Possible values for the ProcessWindowStyle enum are
ProcessWindowStyle.Hidden, ProcessWindowStyle.Maximized, ProcessWindowStyle.Minimized and ProcessWindowStyle.Normal.

The default value is ProcessWindowStyle.Hidden.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .WithWindowStyle(ProcessWindowStyle.Hidden);
```

### ShowWindow

ShowWindow sets the window style of the application to ProcessWindowStyle.Normal.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .ShowWindow();
```

### HideWindow

HideWindow sets the window style of the application to ProcessWindowStyle.Hidden.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .HideWindow();
```

### MaximizeWindow

MaximizeWindow sets the window style of the application to ProcessWindowStyle.Maximized.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .MaximizeWindow();
```

### MinimizeWindow

MinimizeWindow sets the window style of the application to ProcessWindowStyle.Minimized.

```csharp
var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .MinimizeWindow();
```

## Sample

```csharp
using System;
using System.Threading.Tasks;
using EinsTools.Utilities.ProcessLib;

var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .In("/var/myapp")
    .OutputTo(Console.Out)
    .ErrorTo(Console.Error)
    .ThrowOnError(n => n < 5);
    
var exitCode = await app.Execute();
```

## License

This library is licensed under the BSD 3-Clause License. See the LICENSE file for details.

