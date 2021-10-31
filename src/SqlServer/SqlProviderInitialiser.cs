namespace Eventus.SqlServer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Transactions;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public class SqlProviderInitialiser
    {
        private readonly EventusSqlServerOptions _sqlOptions;

        public SqlProviderInitialiser(EventusSqlServerOptions sqlOptions)
        {
            _sqlOptions = sqlOptions;
        }

        public void Init()
        {
            var aggregateTypes = DetectAggregates();

            var connection = GetOpenConnection();

            using (connection)
            {
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var aggregate in aggregateTypes)
                    {
                        CreateTableForAggregate(connection, aggregate);

                        CreateSnapshotTableForAggregate(connection, aggregate);
                    }

                    transactionScope.Complete();
                }
            }
        }

        protected virtual IEnumerable<Type> DetectAggregates()
        {
            var aggregateTypes = AggregateCache.GetAggregateTypes();

            return aggregateTypes;
        }

        protected virtual void CreateTableForAggregate(IDbConnection connection, Type aggregate)
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
	                                            PRIMARY KEY (AggregateId,[Id]))", SqlSchemaHelper.TableName(aggregate),
                _sqlOptions.Schema);

            connection.Execute(aggregateTable);
        }

        protected virtual void CreateSnapshotTableForAggregate(IDbConnection connection, Type aggregate)
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
	                                            PRIMARY KEY (AggregateId,[Id]))",
                SqlSchemaHelper.SnapshotTableName(aggregate),
                _sqlOptions.Schema);

            connection.Execute(aggregateTable);
        }

        private SqlConnection GetOpenConnection()
        {
            var connection = new SqlConnection(_sqlOptions.ConnectionString);
            connection.Open();
            return connection;
        }
    }
}