using System;
using System.Configuration;
using EventSourcing.Cleanup;
using EventSourcing.Samples.Infrastructure.DocumentDb;

namespace EventSourcing.Samples.Infrastructure.Factories
{
    public class TearDownFactory
    {
        public static ITeardown Create()
        {
            var provider = ConfigurationManager.AppSettings["Provider"].ToLowerInvariant();
            switch (provider)
            {
                case Constants.Eventstore:
                    throw new NotImplementedException();
                case Constants.DocumentDb:
                    return DocumentDbFactory.CreateTeardown();
                default:
                    throw new ConfigurationErrorsException($"Unrecognised provider '{provider}' provide a valid provider");
            }
        }
    }
}