using Microsoft.Azure.Cosmos.Table;
using SWMSB.COMMON;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace SWMSB.DATA
{
    public static class StorageTableExtension
    {
        public static IList<dynamic> ToDynamicList(IEnumerable<DynamicTableEntity> dynamicTableEntities)
        {
            var list = new List<dynamic>();

            var entityProperties = new HashSet<string>();
            //iterate over all rows to collect all properties
            foreach (var key in dynamicTableEntities.SelectMany(dynamicTableEntity => dynamicTableEntity.Properties.Keys))
            {
                entityProperties.Add(key);
            }

            foreach (var dynamicTableEntity in dynamicTableEntities)
            {
                var dynamicObject = new ExpandoObject() as IDictionary<string, Object>;
                dynamicObject.Add("PartitionKey", (dynamic)dynamicTableEntity.PartitionKey);
                dynamicObject.Add("RowKey", (dynamic)dynamicTableEntity.RowKey);
                dynamicObject.Add("ETag", (dynamic)dynamicTableEntity.ETag);
                dynamicObject.Add("Timestamp", (dynamic)dynamicTableEntity.Timestamp);

                foreach (var entityProperty in entityProperties)
                {
                    if (!dynamicTableEntity.Properties.ContainsKey(entityProperty))
                    {
                        dynamicObject.Add(entityProperty, (dynamic)string.Empty);
                        continue;
                    }

                    var item = dynamicTableEntity.Properties[entityProperty];
                    switch (item.PropertyType)
                    {
                        case EdmType.Binary:
                            dynamicObject.Add(entityProperty, (dynamic)item.BinaryValue);
                            break;
                        case EdmType.Boolean:
                            dynamicObject.Add(entityProperty, (dynamic)item.BooleanValue);
                            break;
                        case EdmType.DateTime:
                            dynamicObject.Add(entityProperty, (dynamic)item.DateTimeOffsetValue);
                            break;
                        case EdmType.Double:
                            dynamicObject.Add(entityProperty, (dynamic)item.DoubleValue);
                            break;
                        case EdmType.Guid:
                            dynamicObject.Add(entityProperty, (dynamic)item.GuidValue);
                            break;
                        case EdmType.Int32:
                            dynamicObject.Add(entityProperty, (dynamic)item.Int32Value);
                            break;
                        case EdmType.Int64:
                            dynamicObject.Add(entityProperty, (dynamic)item.Int64Value);
                            break;
                        case EdmType.String:
                            dynamicObject.Add(entityProperty, (dynamic)item.StringValue);
                            break;
                    }
                }
                list.Add(dynamicObject);
            }

            return list;
        }
    }

    public class StorageTableProvider<T> : ITable<T> where T : TableEntity, ITableEntity, new()
    {
        private readonly int maxPageSize;
        private readonly CloudTable cloudTable;
        private readonly CloudTableClient cloudTableClient;

        public StorageTableProvider(
            string connectionString, string tableName)
        {

            this.maxPageSize = 10;

            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            var requestOptions = new TableRequestOptions
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 3)
            };
            cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            cloudTableClient.DefaultRequestOptions = requestOptions;

            cloudTable = cloudTableClient.GetTableReference(tableName);
        }
        public void CreateEntity(T entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            cloudTable.ExecuteAsync(insertOperation);
        }
        public IEnumerable<T> GetEntitiesByPartitionKey(string partitionKey)
        {
            var query =
                new TableQuery<T>()
                    .Where(TableQuery.GenerateFilterCondition(
                        "PartitionKey",
                        QueryComparisons.Equal,
                        partitionKey));

            var result = cloudTable.ExecuteQuerySegmentedAsync(query, null);
            return result.Result;
        }
        public IEnumerable<T> GetEntitiesByRowKey(string rowKey)
        {
            var query =
                new TableQuery<T>()
                    .Where(TableQuery.GenerateFilterCondition(
                        "RowKey",
                        QueryComparisons.Equal,
                        rowKey));

            var result = cloudTable.ExecuteQuerySegmentedAsync(query, null);
            return result.Result;
        }
        public async Task<TableResult<T>> GetEntitiesByPartitionKeyWithContinuationTokenAsync(string partitionKey, TableContinuationToken token, int pageSize)
        {
            var query =
                new TableQuery<T>()
                    .Where(TableQuery.GenerateFilterCondition(
                        "PartitionKey",
                        QueryComparisons.Equal,
                        partitionKey)).
                        Take(Math.Min(pageSize, maxPageSize));

            var result = await cloudTable.ExecuteQuerySegmentedAsync(query, token);
            return new TableResult<T>()
            {
                NextToken = result.ContinuationToken,
                PreviousToken = token,
                Data = result.OrderByDescending(d => d.Timestamp).ToList()
            };
        }
        public async Task<TableResult<T>> GetEntitiesByRowKeyWithContinuationTokenAsync(string rowKey, TableContinuationToken token)
        {
            var query =
                new TableQuery<T>()
                    .Where(TableQuery.GenerateFilterCondition(
                        "RowKey",
                        QueryComparisons.Equal,
                        rowKey)).
                        Take(maxPageSize);
            var result = await cloudTable.ExecuteQuerySegmentedAsync(query, null);

            return new TableResult<T>()
            {
                NextToken = result.ContinuationToken,
                PreviousToken = token,
                Data = result.ToList()
            };
        }
        public T GetEntityByPartitionKeyAndRowKey(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var retrievedResult = cloudTable.ExecuteAsync(retrieveOperation);
            return retrievedResult.Result as T;
        }
        public async Task InsertOrUpdateAsync(T entity)
        {
            try
            {

                var insertOrUpdateOperation = TableOperation.InsertOrMerge(entity);
                await cloudTable.ExecuteAsync(insertOrUpdateOperation);
            }
            catch (Exception)
            {

            }
        }
        public async Task<IList<TableResult>> Insert(T[] entities)
        {
            try
            {
                var insertOperation = new TableBatchOperation();
                entities.ToList().ForEach(insertOperation.Insert);
                return await cloudTable.ExecuteBatchAsync(insertOperation);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<IEnumerable<DynamicTableEntity>> Query(string tableName, TableQuery query)
        {
            try
            {
                var table = cloudTableClient.GetTableReference(tableName);
                TableContinuationToken token = null;
                var entities = new List<DynamicTableEntity>();
                do
                {
                    var queryResult = await table.ExecuteQuerySegmentedAsync(query, token);
                    entities.AddRange(queryResult.Results);
                    token = queryResult.ContinuationToken;
                } while (token != null);

                return entities;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    public class TableResult<T> : TableEntity
    {
        public List<T> Data { get; set; }
        public TableContinuationToken NextToken { get; set; }
        public TableContinuationToken PreviousToken { get; set; }
    }
    public interface ITable<T> where T : TableEntity, new()
    {
        void CreateEntity(T entity);
        IEnumerable<T> GetEntitiesByPartitionKey(string partitionKey);
        IEnumerable<T> GetEntitiesByRowKey(string rowKey);
        Task<TableResult<T>> GetEntitiesByPartitionKeyWithContinuationTokenAsync(string partitionKey, TableContinuationToken token, int pageSize);
        Task<TableResult<T>> GetEntitiesByRowKeyWithContinuationTokenAsync(string rowKey, TableContinuationToken token);
        T GetEntityByPartitionKeyAndRowKey(string partitionKey, string rowKey);
        Task InsertOrUpdateAsync(T entity);
        Task<IList<TableResult>> Insert(T[] entities);
        Task<IEnumerable<DynamicTableEntity>> Query(string tableName, TableQuery query);
    }

    public class BaseTableEntity : TableEntity
    {
        public BaseTableEntity()
        {
            RowKey = DateTime.UtcNow.EpochTime().ToString();
        }
    }

    public class TelemetryStorage : BaseTableEntity
    {
        public TelemetryStorage()
        {

        }
        public string DeviceId { get; set; }
        public string Type { get; set; }
        public string WaterUsage { get; set; }

        public long Epoch { get; set; }


        public static string GeneratePartitionKey(long messageGenerated)
        {
#if DEBUG
            var dateNow = messageGenerated.EpochTimeToLocalDateTime();
#else
             var dateNow = messageGenerated.EpochTimeToUtcDateTime();

#endif
            var _partition = dateNow.Minute > 30 ?
                new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, 30, 0, DateTimeKind.Utc) :
                new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, 0, 0, DateTimeKind.Utc);

            return string.Format("{0:s}", _partition);
        }
        public static string GeneratePartitionKey(DateTime dateNow)
        {
            var _partition = dateNow.Minute > 30 ?
                new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, 30, 0, DateTimeKind.Utc) :
                new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, 0, 0, DateTimeKind.Utc);

            return string.Format("{0:s}", _partition);
        }
        public static string GenerateRowKey(string deviceId, long epoch)
        {
            return $"{deviceId}_{epoch}";
        }
    }
}