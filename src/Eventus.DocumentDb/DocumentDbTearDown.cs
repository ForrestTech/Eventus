using System.Net;
using System.Threading.Tasks;
using Eventus.Cleanup;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Eventus.DocumentDb
{
    public class DocumentDbTeardown : ITeardown
    {
        private readonly DocumentClient _client;
        private readonly string _databaseId;

        public DocumentDbTeardown(DocumentClient client, string databaseId)
        {
            _client = client;
            _databaseId = databaseId;
        }

        public async Task TearDownAsync()
        {
            try
            {
                await _client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId))
                    .ConfigureAwait(false);
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