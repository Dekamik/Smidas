using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MethodDecorator.Fody.Interfaces;
using Serilog;
using Serilog.Events;

namespace Smidas.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
    public class StandardLoggingAttribute : Attribute, IMethodDecorator
    {
        private string _method;
        private object[] _args;
        private readonly Stopwatch _stopwatch = new();
        
        public string EntryMessage { get; set; }
        public string ExitMessage { get; set; }
        public string ExceptionMessage { get; set; }
        public LogEventLevel Level { get; set; } = LogEventLevel.Debug;

        public void Init(object instance, MethodBase method, object[] args)
        {
            _method = $"{method.DeclaringType}.{method.Name}";
            _args = args;
        }

        public void OnEntry()
        {
            _stopwatch.Start();
            var message = EntryMessage ?? $"{_method} with args {string.Join(", ", _args.Select(a => a.ToString()))}";
            Log.Write(Level, message);
        }

        public void OnExit()
        {
            _stopwatch.Stop();
            var message = ExitMessage ?? $"{_method} - OK";
            Log.Write(Level, $"{message} ({_stopwatch.ElapsedMilliseconds}ms)");
        }

        public void OnException(Exception exception)
        {
            _stopwatch.Stop();
            var message = ExceptionMessage ?? $"{_method} - ERROR";
            Log.Error(exception, ExceptionMessage ?? $"{message} ({_stopwatch.ElapsedMilliseconds}ms)");
        }
    }
}