//-----------------------------------------------------------------------
// <copyright company="TheNucleus">
// Copyright (c) TheNucleus. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

namespace Nuclei.Diagnostics.Logging.NLog
{
    /// <summary>
    /// Defines constants for possible additional log message properties.
    /// </summary>
    public static class AdditionalLogMessageProperties
    {
        /// <summary>
        /// Defines the property key for information about an EventLog event ID.
        /// </summary>
        public const string EventId = "EventId";

        /// <summary>
        /// Defines the property key for information about an EventLog event category.
        /// </summary>
        public const string EventCategory = "EventCategory";
    }
}
