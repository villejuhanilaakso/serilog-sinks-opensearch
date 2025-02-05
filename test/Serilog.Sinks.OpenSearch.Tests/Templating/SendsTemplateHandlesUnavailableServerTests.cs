﻿using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using Serilog.Debugging;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    [Collection("isolation")]
    public class SendsTemplateHandlesUnavailableServerTests : OpenSearchSinkTestsBase
    {
        [Fact]
        public void Should_not_crash_when_server_is_unavailable()
        {
            // If this crashes, the test will fail
            CreateLoggerThatCrashes();
        }

        [Fact]
        public void Should_write_error_to_self_log()
        {
            var selfLogMessages = new StringBuilder();
            SelfLog.Enable(new StringWriter(selfLogMessages));

            // Exception occurs on creation - should be logged
            CreateLoggerThatCrashes();

            var selfLogContents = selfLogMessages.ToString();
            selfLogContents.Should().Contain("Failed to create the template");

        }

        private static ILogger CreateLoggerThatCrashes()
        {
            var loggerConfig = new LoggerConfiguration()
                .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9199"))
                {
                    AutoRegisterTemplate = true,
                    TemplateName = "crash"
                });

            return loggerConfig.CreateLogger();
        }
    }
}