namespace Eventus.SqlServer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using Dapper;
    using Storage;

    public class SqlProviderInitialiser : SqlServerProviderBase, IStorageProviderInitialiser
    {
        public SqlProviderInitialiser(EventusSqlServerOptions options) : base(options)
        {
        }

        public async Task Init()
        {
            var aggregateTypes = DetectAggregates();

            var aggregates = BuildAggregateConfigs(aggregateTypes);

            var connection = await GetOpenConnectionAsync();

            await using (connection)
            {
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var aggregateConfig in aggregates)
                    {
                        await CreateTableForAggregateAsync(connection, aggregateConfig);

                        await CreateSnapshotTableForAggregateAsync(connection, aggregateConfig);
                    }

                    transactionScope.Complete();
                }
            }
        }

        public void InitSync()
        {
            var aggregateTypes = DetectAggregates();

            var aggregates = BuildAggregateConfigs(aggregateTypes);

            var connection = GetOpenConnection();

            using (connection)
            {
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var aggregateConfig in aggregates)
                    {
                        CreateTableForAggregate(connection, aggregateConfig);

                        CreateSnapshotTableForAggregate(connection, aggregateConfig);
                    }

                    transactionScope.Complete();
                }
            }
        }

        protected virtual IEnumerable<Type> DetectAggregates()
        {
            var aggregateTypes = AggregateHelper.GetAggregateTypes();

            return aggregateTypes;
        }

        protected virtual IEnumerable<AggregateConfig> BuildAggregateConfigs(IEnumerable<Type> aggregateTypes)
        {
            var aggregateConfigs = aggregateTypes.Select(t => new AggregateConfig(t));

            return aggregateConfigs;
        }
        
        protected virtual void CreateTableForAggregate(IDbConnection connection, AggregateConfig aggregateConfig)
        {
            var aggregateTable = string.Format(@"IF OBJECT_ID (N'{0}', N'U') IS NOT NULL

                                             DROP TABLE [{1}].[{0}]

                                             CREATE TABLE [{1}].[{0}](
	                                            [Id]               UNIQUEIDENTIFIER NOT NULL,
                                                [AggregateId]      UNIQUEIDENTIFIER NOT NULL,
                                                [TargetVersion]    INT              NOT NULL,
                                                [ClrType]          NVARCHAR (500)   NOT NULL,
                                                [AggregateVersion] INT              NOT NULL,
                                                [TimeStamp]        DATETIME2 (7)    NOT NULL,
                                                [Data]             NVARCHAR (MAX)   NOT NULL
	                                            PRIMARY KEY (AggregateId,[Id]))", TableName(aggregateConfig.AggregateType), Options.Schema);

            connection.Execute(aggregateTable);
        }

        protected virtual Task CreateTableForAggregateAsync(IDbConnection connection, AggregateConfig aggregateConfig)
        {
            var aggregateTable = string.Format(@"IF OBJECT_ID (N'{0}', N'U') IS NOT NULL

                                             DROP TABLE [{1}].[{0}]

                                             CREATE TABLE [{1}].[{0}](
	                                            [Id]               UNIQUEIDENTIFIER NOT NULL,
                                                [AggregateId]      UNIQUEIDENTIFIER NOT NULL,
                                                [TargetVersion]    INT              NOT NULL,
                                                [ClrType]          NVARCHAR (500)   NOT NULL,
                                                [AggregateVersion] INT              NOT NULL,
                                                [TimeStamp]        DATETIME2 (7)    NOT NULL,
                                                [Data]             NVARCHAR (MAX)   NOT NULL
	                                            PRIMARY KEY (AggregateId,[Id]))", TableName(aggregateConfig.AggregateType), Options.Schema);

            return connection.ExecuteAsync(aggregateTable);
        }
        
        protected virtual void CreateSnapshotTableForAggregate(IDbConnection connection, AggregateConfig aggregateConfig)
        {
            var aggregateTable = string.Format(@"IF OBJECT_ID (N'{0}', N'U') IS NOT NULL

                                             DROP TABLE [{1}].[{0}]

                                             CREATE TABLE [{1}].[{0}](
	                                            [Id]               UNIQUEIDENTIFIER NOT NULL,
                                                [AggregateId]      UNIQUEIDENTIFIER NOT NULL,                                                
                                                [ClrType]          NVARCHAR (500)   NOT NULL,
                                                [AggregateVersion] INT              NOT NULL,
                                                [TimeStamp]        DATETIME2 (7)    NOT NULL,
                                                [Data]             NVARCHAR (MAX)   NOT NULL
	                                            PRIMARY KEY (AggregateId,[Id]))", SnapshotTableName(aggregateConfig.AggregateType), Options.Schema);

            connection.Execute(aggregateTable);
        }

        protected virtual Task CreateSnapshotTableForAggregateAsync(IDbConnection connection, AggregateConfig aggregateConfig)
        {
            var aggregateTable = string.Format(@"IF OBJECT_ID (N'{0}', N'U') IS NOT NULL

                                             DROP TABLE [{1}].[{0}]

                                             CREATE TABLE [{1}].[{0}](
	                                            [Id]               UNIQUEIDENTIFIER NOT NULL,
                                                [AggregateId]      UNIQUEIDENTIFIER NOT NULL,                                                
                                                [ClrType]          NVARCHAR (500)   NOT NULL,
                                                [AggregateVersion] INT              NOT NULL,
                                                [TimeStamp]        DATETIME2 (7)    NOT NULL,
                                                [Data]             NVARCHAR (MAX)   NOT NULL
	                                            PRIMARY KEY (AggregateId,[Id]))", SnapshotTableName(aggregateConfig.AggregateType), Options.Schema);

            return connection.ExecuteAsync(aggregateTable);
        }
    }
}