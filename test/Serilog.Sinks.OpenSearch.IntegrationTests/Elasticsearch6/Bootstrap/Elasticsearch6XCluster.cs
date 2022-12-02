using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;

namespace Serilog.Sinks.OpenSearch.IntegrationTests.Elasticsearch6.Bootstrap
{
	public class Elasticsearch6XCluster : ClientTestClusterBase
	{
		public Elasticsearch6XCluster() : base(CreateConfiguration()) { }

		private static ClientTestClusterConfiguration CreateConfiguration()
		{
			return new ClientTestClusterConfiguration("6.6.0")
			{
				MaxConcurrency = 1
			};
		}

		protected override void SeedCluster() { }
	}
}
