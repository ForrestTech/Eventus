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
        private static readonly Object LockObject = new();

        private readonly EventusSqlServerOptions _sqlOptions;

        public SqlProviderInitialiser(EventusSqlServerOptions sqlOptions)
        {
            _sqlOptions = sqlOptions;
        }

        public void Init()
        {
            var aggregateTypes = DetectAggregates();

            lock (LockObject)
            {
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
        }

        protected virtual IEnumerable<Type> DetectAggregates()
        {
            var aggregateTypes = AggregateCache.GetAggregateTypes();

            return aggregateTypes;
        }

        protected virtual void CreateTableForAggregate(IDbConnection connection, Type aggregate)
        {
            var aggregateTable = string.Format(
                @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND  TABLE_NAME = '{1}')
                                                BEGIN
                                                CREATE TABLE [{0}].[{1}](
	                                            [Id]               UNIQUEIDENTIFIER NOT NULL,
                                                [AggregateId]      UNIQUEIDENTIFIER NOT NULL,
                                                [TargetVersion]    INT              NOT NULL,
                                                [ClrType]          NVARCHAR (500)   NOT NULL,
                                                [AggregateVersion] INT              NOT NULL,
                                                [TimeStamp]        DATETIME2 (7)    NOT NULL,
                                                [Data]             NVARCHAR (MAX)   NOT NULL
	                                            PRIMARY KEY (AggregateId,[Id]))
                                                END",
                _sqlOptions.Schema,
                SqlSchemaHelper.TableName(aggregate));

            connection.Execute(aggregateTable);
        }

        protected virtual void CreateSnapshotTableForAggregate(IDbConnection connection, Type aggregate)
        {
            var aggregateTable = string.Format(
                @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND  TABLE_NAME = '{1}')
                                                BEGIN
                                                CREATE TABLE [{0}].[{1}](
	                                            [Id]               UNIQUEIDENTIFIER NOT NULL,
                                                [AggregateId]      UNIQUEIDENTIFIER NOT NULL,                                                
                                                [ClrType]          NVARCHAR (500)   NOT NULL,
                                                [AggregateVersion] INT              NOT NULL,
                                                [TimeStamp]        DATETIME2 (7)    NOT NULL,
                                                [Data]             NVARCHAR (MAX)   NOT NULL
	                                            PRIMARY KEY (AggregateId,[Id]))
                                                END",
                _sqlOptions.Schema,
                SqlSchemaHelper.SnapshotTableName(aggregate));

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