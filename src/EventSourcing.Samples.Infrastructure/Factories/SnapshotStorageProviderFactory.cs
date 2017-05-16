﻿using System;
using System.Configuration;
using System.Threading.Tasks;
using EventSourcing.Samples.Infrastructure.DocumentDb;
using EventSourcing.Storage;

namespace EventSourcing.Samples.Infrastructure.Factories
{
    public class SnapshotStorageProviderFactory
    {
        public static Task<ISnapshotStorageProvider> CreateAsync(bool addLogging = false)
        {
            var provider = ConfigurationManager.AppSettings["Provider"].ToLowerInvariant();
            switch (provider)
            {
                case Constants.Eventstore:
                    return EventStoreFactory.CreateSnapshotStorageProviderAsync(addLogging);
                case Constants.DocumentDb:
                    return DocumentDbFactory.CreateDocumentDbSnapshotProviderAsync(addLogging);
                default:
                    throw new ConfigurationErrorsException($"Unrecognized provider '{provider}' provide a valid provider");
            }
        }
    }
}