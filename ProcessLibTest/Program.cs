// See https://aka.ms/new-console-template for more information

using System.Text;
using ProcessLib;

var sbOut = new StringBuilder();
var sbErr = new StringBuilder();
var app = ExternalApplication.Create("pwsh", "-c", "echo 'Hello, World!'")
    .OutputTo(s => sbOut.AppendLine(s))
    .ErrorTo(s => sbErr.AppendLine(s));
    
var exitCode = await app.Execute();
Console.WriteLine($"Exit code: {exitCode}");
Console.WriteLine($"StdOut: {sbOut}");
Console.WriteLine($"StdErr: {sbErr}");

app = ExternalApplication.Create("git", "branch")
    .OutputTo(s => sbOut.AppendLine(s))
    .ErrorTo(s => sbErr.AppendLine(s));
    
exitCode = await app.Execute();

Console.WriteLine($"Exit code: {exitCode}");
Console.WriteLine($"StdOut: {sbOut}");
Console.WriteLine($"StdErr: {sbErr}");

app = ExternalApplication.Create("pwsh", "-c", "ls")
    .OutputTo(s => sbOut.AppendLine(s))
    .ErrorTo(s => sbErr.AppendLine(s))
    .ThrowOnError(code => code == 0);
    
exitCode = await app.Execute();
Console.WriteLine($"Exit code: {exitCode}");
Console.WriteLine($"StdOut: {sbOut}");
Console.WriteLine($"StdErr: {sbErr}");

app = ExternalApplication.Create("pwsh", "-c", "ls")
    .In(Path.GetTempPath())
    .OutputTo(s => sbOut.AppendLine(s))
    .ErrorTo(s => sbErr.AppendLine(s))
    .ThrowOnError(code => code == 0);
    
exitCode = await app.Execute();
Console.WriteLine($"Exit code: {exitCode}");
Console.WriteLine($"StdOut: {sbOut}");
Console.WriteLine($"StdErr: {sbErr}");