//-----------------------------------------------------------------------
// <copyright company="TheNucleus">
// Copyright (c) TheNucleus. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using NLog;

namespace Nuclei.Diagnostics.Logging.NLog
{
    /// <summary>
    /// Defines a logging object that translates <see cref="ILogMessage"/> objects and
    /// writes them to a log.
    /// </summary>
    public sealed class NLogLogger : ILogger
    {
        /// <summary>
        /// Translates from NLog level to the apollo log level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns>
        /// The <see cref="LevelToLog"/>.
        /// </returns>
        private static LevelToLog TranslateFromNlogLevel(Logger logger)
        {
            if (logger.IsTraceEnabled)
            {
                return LevelToLog.Trace;
            }

            if (logger.IsDebugEnabled)
            {
                return LevelToLog.Debug;
            }

            if (logger.IsInfoEnabled)
            {
                return LevelToLog.Info;
            }

            if (logger.IsWarnEnabled)
            {
                return LevelToLog.Warn;
            }

            if (logger.IsErrorEnabled)
            {
                return LevelToLog.Error;
            }

            return logger.IsFatalEnabled ? LevelToLog.Fatal : LevelToLog.None;
        }

        /// <summary>
        /// Translates from apollo log level to NLog level.
        /// </summary>
        /// <param name="levelToLog">The log level.</param>
        /// <returns>
        /// The LogLevel.
        /// </returns>
        private static LogLevel TranslateToNlogLevel(LevelToLog levelToLog)
        {
            switch (levelToLog)
            {
                case LevelToLog.Trace:
                    return LogLevel.Trace;
                case LevelToLog.Debug:
                    return LogLevel.Debug;
                case LevelToLog.Info:
                    return LogLevel.Info;
                case LevelToLog.Warn:
                    return LogLevel.Warn;
                case LevelToLog.Error:
                    return LogLevel.Error;
                case LevelToLog.Fatal:
                    return LogLevel.Fatal;
                case LevelToLog.None:
                    return LogLevel.Off;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The log factory that is used to create the logger.
        /// </summary>
        private readonly LogFactory _factory;

        /// <summary>
        /// The logger that performs the actual logging of the log messages.
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// The log template that is used to translate log messages.
        /// </summary>
        private readonly ILogTemplate _template;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLogger"/> class.
        /// </summary>
        /// <param name="factory">The log factory.</param>
        /// <param name="template">The template.</param>
        /// <param name="applicationName">The name of the application that has instantiated the logger.</param>
        /// <param name="applicationVersion">The version of the application that has instantiated the logger.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="factory"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="template"/> is <see langword="null"/>.
        /// </exception>
        internal NLogLogger(LogFactory factory, ILogTemplate template, string applicationName = null, Version applicationVersion = null)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            if (template == null)
            {
                throw new ArgumentNullException("template");
            }

            _template = template;
            _factory = factory;
            {
                _factory.GlobalThreshold = TranslateToNlogLevel(template.DefaultLogLevel());
            }

            _logger = _factory.GetLogger(template.Name);

            // log this at the highest level possible so that it always goes in
            var separationLine = "============================================================================";
            _logger.Fatal(separationLine);

            var openingText = string.Format(CultureInfo.InvariantCulture, "Starting {0} logger.", template.Name);
            _logger.Fatal(template.Translate(new LogMessage(LevelToLog.Info, openingText)));

            var assembly = Assembly.GetEntryAssembly();
            if (((applicationName != null) && (applicationVersion != null))
                || (((applicationName == null) && (applicationVersion == null)) && (assembly != null)))
            {
                var versionInfoText =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} - {1}",
                        applicationName ?? assembly.GetName().Name,
                        applicationVersion ?? assembly.GetName().Version);
                _logger.Info(template.Translate(new LogMessage(LevelToLog.Info, versionInfoText)));
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="LevelToLog"/>.
        /// </summary>
        public LevelToLog Level
        {
            [DebuggerStepThrough]
            get
            {
                return TranslateFromNlogLevel(_logger);
            }

            [DebuggerStepThrough]
            set
            {
                var nlogLevel = TranslateToNlogLevel(value);
                _factory.GlobalThreshold = nlogLevel;
            }
        }

        /// <summary>
        /// Indicates if a message will be written to the log file based on the
        /// current log level and the level of the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        /// <see langword="true" /> if the message will be logged; otherwise, <see langword="false" />.
        /// </returns>
        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.DocumentationRules",
            "SA1628:DocumentationTextMustBeginWithACapitalLetter",
            Justification = "Documentation can start with a language keyword")]
        public bool ShouldLog(ILogMessage message)
        {
            if (Level == LevelToLog.None)
            {
                return false;
            }

            if (message == null)
            {
                return false;
            }

            if (message.Level == LevelToLog.None)
            {
                return false;
            }

            return message.Level >= Level;
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "0",
            Justification = "The 'ShouldLog' method validates the message.")]
        public void Log(ILogMessage message)
        {
            if (!ShouldLog(message))
            {
                return;
            }

            var level = TranslateToNlogLevel(message.Level);
            var info = new LogEventInfo(level, _template.Name, _template.Translate(message));
            if (message.HasAdditionalInformation)
            {
                foreach (var pair in message.Properties)
                {
                    info.Properties[pair.Key] = pair.Value;
                }
            }

            _logger.Log(info);
        }

        /// <summary>
        /// Stops the logger and ensures that all log messages have been
        /// saved to the log.
        /// </summary>
        public void Close()
        {
            _logger.Fatal(_template.Translate(new LogMessage(LevelToLog.Info, "Stopping logger.")));
            _logger.Factory.Flush();
        }

        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or
        ///  resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}
