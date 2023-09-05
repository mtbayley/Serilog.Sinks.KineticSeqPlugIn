using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.KineticSeqPlugIn;

public class KineticSeqSink : ILogEventSink, IDisposable
{
    private const string cExceptionStart = "<Exception><![CDATA[";
    private const string cExceptionEnd = "]]></Exception>";

    private readonly ILogEventSink mTargetSink;

    public KineticSeqSink(ILogEventSink targetSink)
    {
        mTargetSink = targetSink ?? throw new ArgumentNullException(nameof(targetSink));
    }

    public void Emit(LogEvent logEvent)
    {
        if (LogLevelMustChange(logEvent, out var newLogLevel, out var exception))
        {
            // Clone the original log event, but with the new log level
            var newLogEvent = new LogEvent(
                logEvent.Timestamp,
                newLogLevel,
                exception ?? logEvent.Exception,
                logEvent.MessageTemplate,
                logEvent.Properties
                    .Select(kv => new LogEventProperty(kv.Key, kv.Value)));

            mTargetSink.Emit(newLogEvent);
        }
        else
        {
            // Pass-through the original event
            mTargetSink.Emit(logEvent);
        }
    }

    private bool LogLevelMustChange(LogEvent logEvent, out LogEventLevel newLogLevel, out Exception? exception)
    {
        if (logEvent.Level == LogEventLevel.Information && logEvent.Properties.TryGetValue("Message", out var messageValue))
        {
            var messageValueString = messageValue.ToString();
            if (messageValueString.Contains(cExceptionStart))
            {
                // Extract exception
                var exceptionStartIndex = messageValueString.IndexOf(cExceptionStart) + cExceptionStart.Length;
                var exceptionEndIndex = messageValueString.LastIndexOf(cExceptionEnd);
                exception = new Exception(messageValueString.Substring(exceptionStartIndex, exceptionEndIndex - exceptionStartIndex));

                // Set log level
                newLogLevel = LogEventLevel.Error;

                return true;
            }
        }

        newLogLevel = default;
        exception = null;
        return false;
    }

    public void Dispose()
    {
        (mTargetSink as IDisposable)?.Dispose();
    }
}

// Extension method to hook the wrapper into the configuration syntax
public static class LoggerSinkConfigurationLogLevelModifierExtensions
{
    public static LoggerConfiguration KineticSeq(this LoggerSinkConfiguration loggerSinkConfiguration, string serverUrl, string apiKey)
    {
        return LoggerSinkConfiguration.Wrap(loggerSinkConfiguration, sink => new KineticSeqSink(sink), c =>
        {
            c.Seq(serverUrl, apiKey: apiKey);
        }, LevelAlias.Minimum, null);
    }
}