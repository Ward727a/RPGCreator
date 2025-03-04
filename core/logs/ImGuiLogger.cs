using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using RPGCreator.core.debug;
using Serilog.Configuration;

namespace RPGCreator.core.logs
{
    public class ImGuiLogger(IFormatProvider formatProvider) : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider = formatProvider;

        public void Emit(LogEvent logEvent)
        {
            if (!config.Config.debug.b_ImGui) return;
            string message = logEvent.RenderMessage(_formatProvider);
            switch(logEvent.Level)
            {
                case LogEventLevel.Verbose:
                    ImDebug.Logger.AddVerbose(message);
                    break;
                case LogEventLevel.Debug:
                    ImDebug.Logger.AddDebug(message);
                    break;
                case LogEventLevel.Warning:
                    ImDebug.Logger.AddWarn(message);
                    break;
                case LogEventLevel.Information:
                    ImDebug.Logger.AddInfo(message);
                    break;
                case LogEventLevel.Error:
                    ImDebug.Logger.AddError(message);
                    break;
                case LogEventLevel.Fatal:
                    ImDebug.Logger.AddFatal(message);
                    break;
                default:
                    break;
            }
        }
    }

    public static class ImGuiLoggerExtensions
    {
        public static LoggerConfiguration ImGuiLogger(
            this LoggerSinkConfiguration loggerConfiguration,
            IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new ImGuiLogger(formatProvider));
        }
    }
}
