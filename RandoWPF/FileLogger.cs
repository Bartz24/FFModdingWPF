using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Bartz24.RandoWPF;
public class FileLogger : ILogger, IDisposable
{
    private StreamWriter _streamWriter;

    public string FileName { get; set; }
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    public FileLogger(string fileName, LogLevel logLevel)
    {
        FileName = fileName;
        LogLevel = logLevel;

        if (File.Exists(FileName))
        {
            File.Delete(FileName);
        }

        _streamWriter = new StreamWriter(FileName);
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return default;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string logLevelString = $"[{logLevel}]";
        logLevelString = logLevelString.PadRight(15);

        string message = $"{logLevelString} " + formatter(state, exception);

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (exception != null)
        {
            message += Environment.NewLine + Environment.NewLine + exception.ToString();
        }

        _streamWriter.WriteLine(message);
        _streamWriter.Flush();
    }

    public void Dispose()
    {
        _streamWriter?.Dispose();
    }
}
