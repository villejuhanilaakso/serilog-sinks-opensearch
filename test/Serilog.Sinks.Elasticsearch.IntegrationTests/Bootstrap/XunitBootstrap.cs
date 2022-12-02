using Serilog.Sinks.Elasticsearch.IntegrationTests.Bootstrap;
using Xunit;

[assembly: TestFramework("OpenSearch.OpenSearch.Xunit.Sdk.OpenSearchTestFramework", "OpenSearch.OpenSearch.Xunit")]
[assembly: ElasticXunitConfiguration(typeof(SerilogSinkElasticsearchXunitRunOptions))]
