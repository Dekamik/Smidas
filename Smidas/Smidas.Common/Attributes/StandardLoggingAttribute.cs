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
        
        public LogEventLevel Level { get; set; } = LogEventLevel.Information;

        public void Init(object instance, MethodBase method, object[] args)
        {
            _method = $"{method.DeclaringType}.{method.Name}";
            _args = args;
        }

        public void OnEntry()
        {
            _stopwatch.Start();
            Log.Write(Level, $"{_method} with args {string.Join(", ", _args.Select(a => a.ToString()))}");
        }

        public void OnExit()
        {
            _stopwatch.Stop();
            Log.Write(Level, $"{_method} - OK ({_stopwatch.ElapsedMilliseconds}ms)");
        }

        public void OnException(Exception exception)
        {
            _stopwatch.Stop();
            Log.Error(exception, $"{_method} - ERROR ({_stopwatch.ElapsedMilliseconds}ms)");
        }
    }
}