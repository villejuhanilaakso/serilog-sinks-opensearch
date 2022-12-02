using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;
using Xunit;

[assembly: TestFramework("OpenSearch.OpenSearch.Xunit.Sdk.OpenSearchTestFramework", "OpenSearch.OpenSearch.Xunit")]
[assembly: OpenSearchXunitConfiguration(typeof(SerilogSinkOpenSearchXunitRunOptions))]
