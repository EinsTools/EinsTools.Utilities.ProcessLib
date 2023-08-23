using System.Text;

namespace EinsTools.Utilities.ProcessLib;

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