using System.Diagnostics;

namespace EinsTools.Utilities.ProcessLib;

public record ExternalApplication(
    string FileName,
    string[] Arguments,
    string? WorkingDirectory = null,
    Action<string>? OutputDataReceived = null,
    Action<string>? ErrorDataReceived = null,
    Func<int, bool>? IsSuccess = null,
    ProcessWindowStyle WindowStyle = ProcessWindowStyle.Hidden
) {
    /// <summary>
    /// Sets (and overrides!) the arguments for the external application.
    /// </summary>
    /// <param name="arguments">The arguments to use</param>
    public ExternalApplication ReplaceArguments(params string[] arguments) =>
        this with { Arguments = arguments };
    
    /// <summary>
    /// Adds arguments to the external application.
    /// </summary>
    /// <param name="arguments">The arguments to add</param>
    public ExternalApplication AddArguments(params string[] arguments) =>
        this with { Arguments = Arguments.Concat(arguments).ToArray() };
    
    /// <summary>
    /// Sets the working directory for the external application.
    /// </summary>
    /// <param name="workingDirectory">The working directory to use</param>
    /// <returns>A new object with the WorkingDirectory set</returns>
    public ExternalApplication In(string workingDirectory) =>
        this with { WorkingDirectory = workingDirectory };

    /// <summary>
    /// Defines a callback for the output of the external application.
    /// </summary>
    /// <param name="outputDataReceived">Callback</param>
    /// <returns>A new object with the output callback set</returns>
    public ExternalApplication OutputTo(Action<string> outputDataReceived) =>
        this with { OutputDataReceived = outputDataReceived };

    /// <summary>
    /// Defines a callback for the error output of the external application.
    /// </summary>
    /// <param name="errorDataReceived">Callback</param>
    /// <returns>The object with the error callback set</returns>
    public ExternalApplication ErrorTo(Action<string> errorDataReceived) =>
        this with { ErrorDataReceived = errorDataReceived };

    /// <summary>
    /// Sets if the application should throw an exception if the exit code is a success exit code. If the
    /// function returns true, Execute will return the exit code otherwise it will throw an exception.
    /// </summary>
    /// <param name="isSuccess">Function to determine if the exit code is a success. This will
    /// usually be n => n == 0.</param>
    /// <returns></returns>
    public ExternalApplication ThrowOnError(Func<int, bool>? isSuccess = null) =>
        this with { IsSuccess = isSuccess ?? (exitCode => exitCode == 0) };
    
    public ExternalApplication ThrowOnError(Range range) =>
        this with { IsSuccess = exitCode =>
            {
                var (offset, length) = range.GetOffsetAndLength(int.MaxValue);
                return exitCode >= offset && exitCode <= offset + length;
            }
        };

    public ExternalApplication WithWindowStyle(ProcessWindowStyle windowStyle) =>
        this with { WindowStyle = windowStyle };
    
    public ExternalApplication ShowWindow() =>
        this with { WindowStyle = ProcessWindowStyle.Normal };
    
    public ExternalApplication HideWindow() =>
        this with { WindowStyle = ProcessWindowStyle.Hidden };
    
    public ExternalApplication MinimizeWindow() =>
        this with { WindowStyle = ProcessWindowStyle.Minimized };
    
    public ExternalApplication MaximizeWindow() =>
        this with { WindowStyle = ProcessWindowStyle.Maximized };
    
    /// <summary>
    /// Executes the external application.
    /// </summary>
    /// <returns>The exit code of the application</returns>
    /// <exception cref="ExternalApplicationException">Thrown if IsSuccess is set and returns false</exception>
    public async Task<int> Execute() {
        var psi = new ProcessStartInfo {
            FileName = FileName,
            WorkingDirectory = WorkingDirectory,
            RedirectStandardOutput = OutputDataReceived is not null,
            RedirectStandardError = ErrorDataReceived is not null,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = WindowStyle
        };

        foreach (var arg in Arguments)
            psi.ArgumentList.Add(arg);
        var process = new Process {
            StartInfo = psi,
            EnableRaisingEvents = true
        };
        var semaOut = new SemaphoreSlim(0);
        var semaErr = new SemaphoreSlim(0);
        process.OutputDataReceived += (_, e) => {
            if (e.Data is null)
                semaOut.Release();
            else
                OutputDataReceived?.Invoke(e.Data);
        };
        process.ErrorDataReceived += (_, e) => {
            if (e.Data is null)
                semaErr.Release();
            else
                ErrorDataReceived?.Invoke(e.Data);
        };
        process.Start();
        if (OutputDataReceived is not null) process.BeginOutputReadLine();
        if (ErrorDataReceived is not null) process.BeginErrorReadLine();
        if (OutputDataReceived is not null) await semaOut.WaitAsync();
        if (ErrorDataReceived is not null) await semaErr.WaitAsync();
        await process.WaitForExitAsync();
        var exitCode = process.ExitCode;
        if (IsSuccess is not null && !IsSuccess(exitCode))
            throw new ExternalApplicationException(exitCode);
        return exitCode;
    }
    
    public static ExternalApplication Create(
        string fileName,
        string[] arguments,
        Action<string>? outputDataReceived,
        Action<string>? errorDataReceived,
        Func<int, bool>? isSuccess = null
    ) =>
        new(
            fileName,
            arguments,
            null,
            outputDataReceived,
            errorDataReceived,
            isSuccess
        );

    public static ExternalApplication Create(
        string fileName,
        Action<string>? outputDataReceived,
        Action<string>? errorDataReceived,
        Func<int, bool>? isSuccess = null
    ) =>
        new(
            fileName,
            Array.Empty<string>(),
            null,
            outputDataReceived,
            errorDataReceived,
            isSuccess
        );
    
    public static ExternalApplication Create(
        string fileName,
        params string[] arguments) => Create(fileName, arguments, null, null);
}