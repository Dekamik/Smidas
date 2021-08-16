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
    public class ControllerLoggingAttribute : Attribute, IMethodDecorator
    {
        private readonly Stopwatch _stopwatch = new();
        private string _baseMessage;
        
        public string EndpointName { get; set; }
        public LogEventLevel Level { get; set; } = LogEventLevel.Information;

        public void Init(object instance, MethodBase method, object[] args)
        {
            _baseMessage = $"ENDPOINT /{EndpointName}/{args.First()}";
        }

        public void OnEntry()
        {
            _stopwatch.Start();
            Log.Write(Level, _baseMessage);
        }

        public void OnExit()
        {
            _stopwatch.Stop();
            Log.Write(Level, $"{_baseMessage} - 200 OK ({_stopwatch.ElapsedMilliseconds}ms)");
        }

        public void OnException(Exception exception)
        {
            _stopwatch.Stop();
            Log.Error(exception, $"{_baseMessage} - 500 ERROR ({_stopwatch.ElapsedMilliseconds}ms)");
        }
    }
}