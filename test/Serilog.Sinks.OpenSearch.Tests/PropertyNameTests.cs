﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class PropertyNameTests : OpenSearchSinkTestsBase
    {

        [Fact]
        public async Task UsesCustomPropertyNames()
        {
            try
            {
                await this.ThrowAsync();
            }
            catch (Exception e)
            {
                var timestamp = new DateTimeOffset(2013, 05, 28, 22, 10, 20, 666, TimeSpan.FromHours(10));
                var messageTemplate = "{Song}++";
                var template = new MessageTemplateParser().Parse(messageTemplate);
                using (var sink = new OpenSearchSink(_options))
                {
                    var properties = new List<LogEventProperty>
                    {
                        new LogEventProperty("Song", new ScalarValue("New Macabre")), 
                        new LogEventProperty("Complex", new ScalarValue(new { A = 1, B = 2 }))
                    };
                    var logEvent = new LogEvent(timestamp, LogEventLevel.Information, e, template, properties);
                    //one off
                    sink.Emit(logEvent);

                    sink.Emit(logEvent);
                    logEvent = new LogEvent(timestamp.AddDays(2), LogEventLevel.Information, e, template, properties);
                    sink.Emit(logEvent);
                }
                var bulkJsonPieces = this.AssertSeenHttpPosts(_seenHttpPosts, 4);
                bulkJsonPieces[0].Should().Contain(@"""_index"":""logstash-2013.05.28");
                bulkJsonPieces[1].Should().Contain("New Macabre");
                bulkJsonPieces[1].Should().NotContain("Properties\"");
                bulkJsonPieces[1].Should().Contain("fields\":{");
                bulkJsonPieces[1].Should().Contain("@timestamp");
                bulkJsonPieces[2].Should().Contain(@"""_index"":""logstash-2013.05.30");
            }
        }
    }

}
