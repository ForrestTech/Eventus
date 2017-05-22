using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Eventus.SqlServer
{
    public abstract class SqlServerProviderBase
    {
        private static JsonSerializerSettings _serializerSetting;
        protected static JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (_serializerSetting != null)
                    return _serializerSetting;

                _serializerSetting = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                _serializerSetting.Converters.Add(new StringEnumConverter());

                return _serializerSetting;
            }
        }

        protected string ConnectionString;

        protected async Task<SqlConnection> GetOpenConnectionAsync()
        {
            var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        protected SqlServerProviderBase(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        protected static string GetClrTypeName(object item)
        {
            return item.GetType() + "," + item.GetType().Assembly.GetName().Name;
        }
    }
}