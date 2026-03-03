using System.Text;

namespace LanLinker.Console.UI;

internal sealed class InputHandler : IDisposable
{
    private readonly object _lock = new();
    private readonly StringBuilder _buffer = new();
    private bool _disposed;

    public string CurrentBuffer
    {
        get
        {
            lock (_lock)
            {
                return _buffer.ToString();
            }
        }
    }

    public event Action<string>? MessageSubmitted;

    public void Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !_disposed)
        {
            try
            {
                if (!System.Console.KeyAvailable)
                {
                    Thread.Sleep(10);
                    continue;
                }

                ConsoleKeyInfo key = System.Console.ReadKey(intercept: true);

                lock (_lock)
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.Backspace:
                        {
                            if (_buffer.Length > 0)
                            {
                                _buffer.Remove(_buffer.Length - 1, 1);
                            }

                            break;
                        }

                        case ConsoleKey.Enter:
                        {
                            string message = _buffer.ToString().Trim();

                            _buffer.Clear();

                            if (!string.IsNullOrEmpty(message))
                            {
                                MessageSubmitted?.Invoke(message);
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException or InvalidOperationException)
            {
                break;
            }
        }
    }

    public void Dispose() => _disposed = true;
}