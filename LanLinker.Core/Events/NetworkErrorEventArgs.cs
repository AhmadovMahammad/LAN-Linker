namespace LanLinker.Core.Events;

public class NetworkErrorEventArgs(Exception ex, string context) : EventArgs
{
    public Exception Exception { get; } = ex;
    public string Context { get; } = context;
}