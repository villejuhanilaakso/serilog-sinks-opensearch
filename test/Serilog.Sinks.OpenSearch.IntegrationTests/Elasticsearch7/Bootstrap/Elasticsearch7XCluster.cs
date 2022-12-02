using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;

namespace Serilog.Sinks.OpenSearch.IntegrationTests.Elasticsearch7.Bootstrap
{
	public class Elasticsearch7XCluster : ClientTestClusterBase
	{
		public Elasticsearch7XCluster() : base(CreateConfiguration()) { }

		private static ClientTestClusterConfiguration CreateConfiguration()
		{
			return new ClientTestClusterConfiguration("7.8.0")
			{
				MaxConcurrency = 1
			};
		}

		protected override void SeedCluster() { }
	}
}
