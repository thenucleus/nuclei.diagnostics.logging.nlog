//-----------------------------------------------------------------------
// <copyright company="TheNucleus">
// Copyright (c) TheNucleus. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENCE.md file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Nuclei.Configuration;
using NUnit.Framework;

namespace Nuclei.Diagnostics.Logging.NLog.Samples
{
    [TestFixture]
    public sealed class LoggerBuilderSample
    {
        [Test]
        [Ignore("Implemented but not runable. Just here so that we can get a documentation sample.")]
        public void ForEventLog()
        {
            var template = new DebugLogTemplate(
                new NullConfiguration(),
                () => DateTimeOffset.Now);
            var logger = LoggerBuilder.ForEventLog(
                "MyEventLog",
                template,
                "MyApplication",
                new Version(1, 0, 0, 0));

            Assert.IsNotNull(logger);
        }

        [Test]
        [Ignore("Implemented but not runable. Just here so that we can get a documentation sample.")]
        public void ForFile()
        {
            var template = new DebugLogTemplate(
                new NullConfiguration(),
                () => DateTimeOffset.Now);
            var logger = LoggerBuilder.ForFile(
                @"c:\temp\log.txt",
                template,
                "MyApplication",
                new Version(1, 0, 0, 0));

            Assert.IsNotNull(logger);
        }
    }
}
