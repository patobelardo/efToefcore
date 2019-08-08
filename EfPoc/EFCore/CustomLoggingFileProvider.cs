using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfPoc.EFCore
{
    public class CustomLoggingFileProvider : ILoggerProvider
    {
        private readonly string _logFilePath;
        public CustomLoggingFileProvider(string logFilePath)
        {
            _logFilePath = logFilePath;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger(categoryName, _logFilePath);
        }

        public void Dispose()
        {
        }

        public class CustomLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly string _path;

            public CustomLogger(string categoryName, string logFilePath)
            {
                _path = logFilePath;
                _categoryName = categoryName;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                try
                {
                    RecordMsg(logLevel, eventId, state, exception, formatter);
                }
                catch (Exception ex)
                {
                    //this is being used in case of error 'the process cannot access the file because it is being used by another process', could not find a better way to resolve the issue
                    RecordMsg(logLevel, eventId, state, exception, formatter);
                }
            }

            Task DoAsync()
            {
                var f = File.Open(_path, FileMode.Open);
                f.Close();
                return Task.CompletedTask;
            }

            private void RecordMsg<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                string msg = $"{logLevel} :: {_categoryName} :: {formatter(state, exception)} :: username :: {DateTime.Now}";
                using (var writer = File.AppendText(_path))
                {
                    writer.WriteLine(msg);
                }
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return new NoopDisposable();
            }

            private class NoopDisposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
