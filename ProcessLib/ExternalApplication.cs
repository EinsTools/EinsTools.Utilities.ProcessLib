using System.Diagnostics;
using System.Text;

namespace ProcessLib;

public record ExternalApplication(
    string FileName,
    string[] Arguments,
    string? WorkingDirectory = null,
    Action<string>? OutputDataReceived = null,
    Action<string>? ErrorDataReceived = null,
    Func<int, bool>? IsSuccess = null
) {
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
    public ExternalApplication ThrowOnError(Func<int, bool>? isSuccess) =>
        this with { IsSuccess = isSuccess ?? (exitCode => exitCode == 0) };

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
            CreateNoWindow = true
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
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await semaOut.WaitAsync();
        await semaErr.WaitAsync();
        await process.WaitForExitAsync();
        var exitCode = process.ExitCode;
        if (IsSuccess is not null && !IsSuccess(exitCode))
            throw new ExternalApplicationException(exitCode);
        return exitCode;
    }
    
    public static ExternalApplication Create(
        string fileName,
        string[] arguments,
        string? workingDirectory,
        Action<string>? outputDataReceived = null,
        Action<string>? errorDataReceived = null,
        Func<int, bool>? isSuccess = null
    ) {
        return new ExternalApplication(
            fileName,
            arguments,
            workingDirectory,
            outputDataReceived,
            errorDataReceived,
            isSuccess
        );
    }
    
    public static ExternalApplication Create(
        string fileName,
        params string[] arguments) => Create(fileName, arguments, null);
}

public record ApplicationResult(int ExitCode, string StdOut, string StdErr);

public static class ExternalApplicationExtensions {
    public static async Task<ApplicationResult> ExecuteWithResult(this ExternalApplication app,
        Func<int, bool>? isSuccess = null) {
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();
        var exitCode = await app
            .OutputTo(s => {
                stdOut.AppendLine(s);
                app.OutputDataReceived?.Invoke(s);
            })
            .ErrorTo(s => {
                stdErr.AppendLine(s);
                app.ErrorDataReceived?.Invoke(s);
            })
            .Execute();
        return new ApplicationResult(exitCode, stdOut.ToString(), stdErr.ToString());
    }
}