using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Eventus.SqlServer.Config;

namespace Eventus.SqlServer
{
    public class SqlProviderInitialiser : SqlServerProviderBase
    {
        public SqlProviderInitialiser(string connectionString) : base(connectionString)
        {
        }

        public async Task InitAsync(SqlServerConfig config)
        {
            //todo manage migrations of initial schema, 
            //todo pass in schema prefix

            var connection = await GetOpenConnectionAsync()
                .ConfigureAwait(false);

            //todo move to bulk action if it gets slow
            using (connection)
            {
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var aggregateConfig in config.Aggregates)
                    {
                        await CreateTableForAggregateAsync(connection, aggregateConfig).ConfigureAwait(false);

                        await CreateSnapshotTableForAggregateAsync(connection, aggregateConfig).ConfigureAwait(false);
                    }

                    transactionScope.Complete();
                }
            }
        }

        private static Task CreateTableForAggregateAsync(IDbConnection connection, AggregateConfig aggregateConfig)
        {
            var aggregateTable = string.Format(@"IF OBJECT_ID (N'{0}', N'U') IS NOT NULL

                                             DROP TABLE [dbo].[{0}]

                                             CREATE TABLE [dbo].[{0}](
	                                            [Id]               UNIQUEIDENTIFIER NOT NULL,
                                                [AggregateId]      UNIQUEIDENTIFIER NOT NULL,
                                                [TargetVersion]    INT              NOT NULL,
                                                [ClrType]          NVARCHAR (500)   NOT NULL,
                                                [AggregateVersion] INT              NOT NULL,
                                                [TimeStamp]        DATETIME2 (7)    NOT NULL,
                                                [Data]             NVARCHAR (MAX)   NOT NULL
	                                            PRIMARY KEY (AggregateId,[Id]))", TableName(aggregateConfig.AggregateType));

            return connection.ExecuteAsync(aggregateTable);
        }

        private static Task CreateSnapshotTableForAggregateAsync(IDbConnection connection, AggregateConfig aggregateConfig)
        {
            var aggregateTable = string.Format(@"IF OBJECT_ID (N'{0}', N'U') IS NOT NULL

                                             DROP TABLE [dbo].[{0}]

                                             CREATE TABLE [dbo].[{0}](
	                                            [Id]               UNIQUEIDENTIFIER NOT NULL,
                                                [AggregateId]      UNIQUEIDENTIFIER NOT NULL,                                                
                                                [ClrType]          NVARCHAR (500)   NOT NULL,
                                                [AggregateVersion] INT              NOT NULL,
                                                [TimeStamp]        DATETIME2 (7)    NOT NULL,
                                                [Data]             NVARCHAR (MAX)   NOT NULL
	                                            PRIMARY KEY (AggregateId,[Id]))", SnapshotTableName(aggregateConfig.AggregateType));

            return connection.ExecuteAsync(aggregateTable);
        }
    }
}