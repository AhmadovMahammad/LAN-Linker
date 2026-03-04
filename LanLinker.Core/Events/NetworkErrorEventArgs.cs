namespace LanLinker.Core.Events;

public class NetworkErrorEventArgs(Exception exception, string context) : EventArgs
{
    public Exception Exception { get; } = exception;
    public string Context { get; } = context;
}