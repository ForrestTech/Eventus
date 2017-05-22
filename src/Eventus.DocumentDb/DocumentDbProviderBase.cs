using System;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Eventus.DocumentDb
{
    public abstract class DocumentDbProviderBase
    {
        private static JsonSerializerSettings _serializerSetting;

        protected DocumentClient Client;
        protected string DatabaseId;

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

        protected DocumentDbProviderBase(DocumentClient client, string databaseId)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            DatabaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
        }
        
        protected static string GetClrTypeName(object item)
        {
            return item.GetType() + "," + item.GetType().Assembly.GetName().Name;
        }

        protected static string SnapshotCollectionName(Type aggregateType)
        {
            return $"{aggregateType.Name}-snapshot";
        }
    }
}