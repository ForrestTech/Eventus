using System;
using System.Configuration;
using EventSourcing.Cleanup;

namespace EventSourcing.Samples.Infrastructure
{
    public class TearDownFactory
    {
        public static ITeardown Create()
        {
            var provider = ConfigurationManager.AppSettings["Provider"].ToLowerInvariant();
            switch (provider)
            {
                case "eventstore":
                    throw new NotImplementedException();
                case "documentdb":
                    return DocumentDbFactory.CreateTeardown();
                default:
                    throw new ConfigurationErrorsException($"Unrecognised provider '{provider}' provide a valid provider");
            }
        }
    }
}