using System.Threading.Tasks;
using EventSourcing.Cleanup;
using Microsoft.Azure.Documents.Client;

namespace EventSourcing.DocumentDb
{
    public class DocumentDbTearDown : ITeardown
    {
        private readonly DocumentClient _client;
        private readonly string _databaseId;

        public DocumentDbTearDown(DocumentClient client, string databaseId)
        {
            _client = client;
            _databaseId = databaseId;
        }

        public async Task TearDownAsync()
        {
            await _client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId));
        }
    }
}