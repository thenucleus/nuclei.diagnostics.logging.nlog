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
    /// Defines a logging object that translates <see cref="LogMessage"/> objects and
    /// writes them to a log.
    /// </summary>
    public sealed class NLogLogger : ILogger
    {
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
        /// The logger that performs the actual logging of the log messages.
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="applicationName">The name of the application that has instantiated the logger.</param>
        /// <param name="applicationVersion">The version of the application that has instantiated the logger.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="logger"/> is <see langword="null"/>.
        /// </exception>
        internal NLogLogger(Logger logger, string applicationName = null, Version applicationVersion = null)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _logger = logger;

            // log this at the highest level possible so that it always goes in
            _logger.Fatal("============================================================================");
            _logger.Fatal(CultureInfo.InvariantCulture, "Starting {0} logger.", _logger.Name);

            var assembly = Assembly.GetEntryAssembly();
            if (((applicationName != null) && (applicationVersion != null))
                || (((applicationName == null) && (applicationVersion == null)) && (assembly != null)))
            {
                _logger.Info(
                    "{0} - {1}",
                    applicationName ?? assembly.GetName().Name,
                    applicationVersion ?? assembly.GetName().Version);
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="LevelToLog"/>.
        /// </summary>
        public LevelToLog Level
        {
            get;
            set;
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
        public bool ShouldLog(LogMessage message)
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
        public void Log(LogMessage message)
        {
            if (!ShouldLog(message))
            {
                return;
            }

            var level = TranslateToNlogLevel(message.Level);
            var info = new LogEventInfo(level, _logger.Name, message.FormatProvider, message.Text, message.FormatParameters);
            foreach (var pair in message.Properties)
            {
                info.Properties[pair.Key] = pair.Value;
            }

            _logger.Log(info);
        }

        /// <summary>
        /// Stops the logger and ensures that all log messages have been
        /// saved to the log.
        /// </summary>
        public void Close()
        {
            _logger.Info("Stopping logger.");
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
