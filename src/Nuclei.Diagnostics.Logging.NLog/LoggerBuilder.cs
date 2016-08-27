//-----------------------------------------------------------------------
// <copyright company="TheNucleus">
// Copyright (c) TheNucleus. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Nuclei.Diagnostics.Logging.NLog
{
    /// <summary>
    /// Defines a factory that builds <see cref="ILogger"/> objects.
    /// </summary>
    public static class LoggerBuilder
    {
        private static LogFactory BuildLogFactory(string name, LogLevel minimumLevel, Target target)
        {
            var config = new LoggingConfiguration();
            {
                config.AddTarget(name, target);

                // define only one logging rule. We log everything (*) to the this rule starting
                // at log level TRACE and everything goes to the only target
                config.LoggingRules.Add(new LoggingRule("*", minimumLevel, target));
            }

            var result = new LogFactory(config);
            {
                result.GlobalThreshold = LogLevel.Trace;
                result.ThrowExceptions = true;
            }

            result.ResumeLogging();
            return result;
        }

        /// <summary>
        /// Builds an <see cref="ILogger"/> object that logs information to the event log.
        /// </summary>
        /// <param name="loggerName">The name of the logger.</param>
        /// <param name="eventLogSource">The event log source name under which the current application is registered.</param>
        /// <param name="applicationName">The name of the application that has instantiated the logger.</param>
        /// <param name="applicationVersion">The version of the application that has instantiated the logger.</param>
        /// <returns>
        /// The newly created logger that logs information to the event log.
        /// </returns>
        public static ILogger ForEventLog(
            string loggerName,
            string eventLogSource,
            string applicationName = null,
            Version applicationVersion = null)
        {
            var eventLogTarget = new EventLogTarget
            {
                // Only write the message. The message should contain all the important
                // information anyway.
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}",

                // The source which has been registered to write to the event log.
                Source = eventLogSource,

                // Always write to the application log for now.
                Log = "Application",

                // Define how we move the event id to the logger.
                EventId = string.Format(CultureInfo.InvariantCulture, "${{event-context:item={0}}}", AdditionalLogMessageProperties.EventId),

                // Define how we move the event category to the logger.
                Category = string.Format(CultureInfo.InvariantCulture, "${{event-context:item={0}}}", AdditionalLogMessageProperties.EventCategory),
            };

            var factory = BuildLogFactory(loggerName, LogLevel.Warn, eventLogTarget);
            var logger = factory.GetLogger(loggerName);
            return new NLogLogger(logger, applicationName, applicationVersion);
        }

        /// <summary>
        /// Builds an <see cref="ILogger"/> object that logs information to a given file.
        /// </summary>
        /// <param name="loggerName">The name of the logger.</param>
        /// <param name="filePath">The file path to which the information gets logged.</param>
        /// <param name="applicationName">The name of the application that has instantiated the logger.</param>
        /// <param name="applicationVersion">The version of the application that has instantiated the logger.</param>
        /// <returns>
        /// The newly created logger that logs information to a given file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="loggerName"/> is <see langword="null" />.
        /// </exception>
        public static ILogger ForFile(string loggerName, string filePath, string applicationName = null, Version applicationVersion = null)
        {
            if (loggerName == null)
            {
                throw new ArgumentNullException("loggerName");
            }

            if (string.IsNullOrWhiteSpace(loggerName))
            {
                throw new ArgumentException();
            }

            var fileTarget = new FileTarget
            {
                // Only write the message. The message should contain all the important
                // information anyway.
                Layout = "${message}",

                // Get the file path for the log file.
                FileName = filePath,

                // Create the directories if needed
                CreateDirs = true,

                // Automatically flush each message to the file
                AutoFlush = true,

                // Always close the file so that we don't lose messages
                // this does make logging slower though.
                KeepFileOpen = false,

                // Always append to the file
                ReplaceFileContentsOnEachWrite = false,

                // Do not concurrently write to the logger (at least for now)
                ConcurrentWrites = false,
            };

            var factory = BuildLogFactory(loggerName, LogLevel.Trace, fileTarget);
            var logger = factory.GetLogger(loggerName);
            return new NLogLogger(logger, applicationName, applicationVersion);
        }

        /// <summary>
        /// Builds an <see cref="ILogger"/> object that logs information to the log target as specified by the NLog configuration
        /// of the same name.
        /// </summary>
        /// <param name="loggerName">The name of the logger.</param>
        /// <param name="applicationName">The name of the application that has instantiated the logger.</param>
        /// <param name="applicationVersion">The version of the application that has instantiated the logger.</param>
        /// <returns>
        /// The newly created logger that logs information to the log target as specified by the NLog configuration
        /// of the same name.
        /// </returns>
        public static ILogger FromConfiguration(
            string loggerName,
            string applicationName = null,
            Version applicationVersion = null)
        {
            var logger = LogManager.GetLogger(loggerName);
            return new NLogLogger(logger, applicationName, applicationVersion);
        }
    }
}
