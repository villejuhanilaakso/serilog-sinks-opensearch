﻿using OpenSearch.Net;
using Xunit;

namespace Serilog.Sinks.Elasticsearch.Tests.Discrepancies
{
    public class ElasticsearchDefaultSerializerTests : ElasticsearchSinkUniformityTestsBase
    {
        public ElasticsearchDefaultSerializerTests() : base(new LowLevelRequestResponseSerializer()) { }

        [Fact]
        public void Should_SerializeToExpandedExceptionObjectWhenExceptionIsSet()
        {
            this.ThrowAndLogAndCatchBulkOutput("test_with_default_serializer");
        }
    }

}
