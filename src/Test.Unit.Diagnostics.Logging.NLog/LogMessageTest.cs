//-----------------------------------------------------------------------
// <copyright company="TheNucleus">
// Copyright (c) TheNucleus. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Nuclei.Diagnostics.Logging.NLog
{
    [TestFixture]
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
            Justification = "Unit tests do not need documentation.")]
    public sealed class LogMessageTest
    {
        [Test]
        public void Create()
        {
            var level = LevelToLog.Debug;
            var text = "text";

            var message = new LogMessage(level, text);

            Assert.AreEqual(level, message.Level);
            Assert.AreEqual(text, message.Text());
            Assert.IsFalse(message.HasAdditionalInformation);
        }

        [Test]
        public void CreateWithProperties()
        {
            var level = LevelToLog.Debug;
            var text = "text";
            var properties = new Dictionary<string, object>();

            var message = new LogMessage(level, text, properties);

            Assert.AreEqual(level, message.Level);
            Assert.AreEqual(text, message.Text());
            Assert.IsTrue(message.HasAdditionalInformation);
            Assert.AreSame(properties, message.Properties);
        }
    }
}
