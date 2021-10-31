namespace Console.CosmosDB
{
    using Eventus.Samples.Core.Cleanup;
    using Eventus.SqlServer.Configuration;
    using Microsoft.Azure.Cosmos;
    using System.Net;
    using System.Threading.Tasks;

    public class CosmosDBTeardown : ITeardown
    {
        private readonly CosmosClient _client;
        private readonly string _databaseId;

        public CosmosDBTeardown(CosmosClient client, string databaseId)
        {
            _client = client;
            _databaseId = databaseId;
        }

        public async Task TearDownAsync()
        {
            try
            {
                var dataBase = _client.GetDatabase(_databaseId);
                await dataBase.DeleteAsync();

            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return;
                }
                throw;
            }
        }
    }
}