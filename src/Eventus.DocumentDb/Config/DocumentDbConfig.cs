using System;
using System.Collections.Generic;

namespace Eventus.DocumentDb.Config
{
    public class DocumentDbConfig
    {
        public DocumentDbConfig(string databaseId, int defaultThroughput, int defaultSnapshotThroughput)
        {
            DatabaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
            DefaultThroughput = defaultThroughput;
            DefaultSnapshotThroughput = defaultSnapshotThroughput;

            PartitionKey = "/aggregateId";
            ExcludePaths = new List<string>
            {
                "/data/*",
                "/clrType/*",
                "/targetVersion/*",
                "/timestamp/*"
            };
        }

        public string DatabaseId { get; }

        public int DefaultThroughput { get; }

        public int DefaultSnapshotThroughput { get; }

        public string PartitionKey { get; set; }

        public List<string> ExcludePaths { get; set; }
    }
}