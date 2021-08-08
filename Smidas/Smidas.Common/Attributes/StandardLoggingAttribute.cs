using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using MethodDecorator.Fody.Interfaces;
using Serilog;
using Serilog.Events;

namespace Smidas.Common.Attributes
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
    public class StandardLoggingAttribute : Attribute, IMethodDecorator
    {
        private MethodBase _method;
        private object[] _args;
        private readonly Stopwatch _stopwatch = new();
        private string MethodName => $"{_method.DeclaringType?.Name ?? "UNKNOWN"}.{_method.Name}";
        
        public string EntryMessage { get; set; }
        public string ExitMessage { get; set; }
        public string ExceptionMessage { get; set; }
        public LogEventLevel Level { get; set; } = LogEventLevel.Debug;
        public bool ForceTimePrintout { get; set; } = false;

        public void Init(object instance, MethodBase method, object[] args)
        {
            _method = method;
            _args = args;
        }

        public void OnEntry()
        {
            _stopwatch.Start();
            var message = EntryMessage ?? $"{MethodName} with args {string.Join(", ", _args.Select(a => a.ToString()))}";
            Log.Write(Level, message);
        }

        public void OnExit()
        {
            _stopwatch.Stop();
            var message = (ExitMessage ?? $"{MethodName} - OK") + (IsMethodAbstract() && !ForceTimePrintout ? "" : $" ({_stopwatch.ElapsedMilliseconds}ms)");
            Log.Write(Level, message);
        }

        public void OnException(Exception exception)
        {
            _stopwatch.Stop();
            var message = ExceptionMessage ?? $"{MethodName} - ERROR";
            Log.Error(exception, ExceptionMessage ?? $"{message} ({_stopwatch.ElapsedMilliseconds}ms)");
        }

        private bool IsMethodAbstract()
        {
            var info = _method as MethodInfo;
            return info != null && info.IsAbstract;
        }
    }
}