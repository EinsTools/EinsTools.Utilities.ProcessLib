namespace EinsTools.Utilities.ProcessLib;

public class ExternalApplicationException : Exception {
    public int ExitCode { get; }

    public ExternalApplicationException(int exitCode) {
        ExitCode = exitCode;
    }
}