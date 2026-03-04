using System.Text;

namespace LanLinker.Console.UI;

internal sealed class InputHandler : IDisposable
{
    private readonly StringBuilder _buffer = new();
    
    private readonly object _lock = new();
    
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

    public void Dispose() => _disposed = true;

    public event Action? BufferChanged;

    public event Action<string>? Submitted;

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

                string? submitted = null;

                bool changed = false;

                lock (_lock)
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.Backspace:
                        {
                            if (_buffer.Length > 0)
                            {
                                _buffer.Remove(_buffer.Length - 1, 1);
                                changed = true;
                            }

                            break;
                        }

                        case ConsoleKey.Enter:
                        {
                            string input = _buffer.ToString().Trim();

                            _buffer.Clear();

                            changed = true;

                            if (!string.IsNullOrEmpty(input))
                            {
                                submitted = input;
                            }

                            break;
                        }

                        default:
                        {
                            if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
                            {
                                _buffer.Append(key.KeyChar);
                                changed = true;
                            }

                            break;
                        }
                    }
                }

                if (changed)
                {
                    BufferChanged?.Invoke();
                }

                if (submitted is not null)
                {
                    Submitted?.Invoke(submitted);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException or InvalidOperationException)
            {
                break;
            }
        }
    }
}