namespace Eventus.SqlServer
{
    using System;

    public static class SqlSchemaHelper
    {
        public static string TableName(Type aggregateType)
        {
            return aggregateType.Name;
        }

        public  static string SnapshotTableName(Type aggregateType)
        {
            return $"{aggregateType.Name}_Snapshot";
        }
    }
}