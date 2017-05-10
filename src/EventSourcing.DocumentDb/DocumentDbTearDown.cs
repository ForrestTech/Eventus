using System.Net;
using System.Threading.Tasks;
using EventSourcing.Cleanup;
using Microsoft.Azure.Documents;
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
            try
            {
                await _client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId));
            }
            catch (DocumentClientException e)
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