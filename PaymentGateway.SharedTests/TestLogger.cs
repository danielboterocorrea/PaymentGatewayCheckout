﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.SharedTests
{
    public static class TestLogger
    {
        public static ILogger<T> Create<T>()
        {
            var logger = new NUnitLogger<T>();
            return logger;
        }

        class NUnitLogger<T> : ILogger<T>, IDisposable
        {
            private readonly Action<string> output = Console.WriteLine;

            public void Dispose()
            {
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter) => output(formatter(state, exception));

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => this;
        }
    }
}
