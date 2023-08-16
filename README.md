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

## Sample

```csharp
using System;
using System.Threading.Tasks;
using ProcessLib;

var app = ExternalApplication.Create("MyApp.exe", "-c", "OptionValue)
    .In("/var/myapp")
    .OutputTo(Console.Out)
    .ErrorTo(Console.Error)
    .ThrowOnError(n => n < 5);
    
var exitCode = await app.Execute();
```

## License

This library is licensed under the BSD 3-Clause License. See the LICENSE file for details.

