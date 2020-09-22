using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using SWMSB.COMMON;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace SWMSB.PROVIDERS
{
    public static class StorageTableExtension
    {
        public static IList<TelemetryStorage> ToTelemetryList(IEnumerable<DynamicTableEntity> dynamicTableEntities)
        {
            var list = new List<TelemetryStorage>();
            foreach (var dynamicTableEntity in dynamicTableEntities)
            {
                var item = new TelemetryStorage {
                    PartitionKey = dynamicTableEntity.PartitionKey.ToString(),
                    RowKey = dynamicTableEntity.RowKey.ToString(),
                    DayWaterUsage =double.Parse(dynamicTableEntity.Properties["daywaterusage"].ToString()),
                    AvgWaterUsage = double.Parse(dynamicTableEntity.Properties["avg"].ToString()),
                };
                list.Add(item);
            }

            return list;
        }
    }

    public class StorageTableProvider<T> : ITable<T> where T : TableEntity, ITableEntity, new()
    {
        public Config Config { get; }

        private readonly int maxPageSize;
        private readonly CloudTable cloudTable;
        private readonly CloudTableClient cloudTableClient;

        public StorageTableProvider(
            Config config)
        {
            Config = config;
            this.maxPageSize = 10;

            var cloudStorageAccount = CloudStorageAccount.Parse(config.BlobStorageConnectionString);
            var requestOptions = new TableRequestOptions
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 3)
            };
            cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            cloudTableClient.DefaultRequestOptions = requestOptions;

            cloudTable = cloudTableClient.GetTableReference(config.StorageTelemetryTableName);
        }
        public void CreateEntity(T entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            cloudTable.ExecuteAsync(insertOperation);
        }
        public async Task<IEnumerable<T>> GetEntitiesByPartitionKey(string partitionKey)
        {
            var query =
                new TableQuery<T>()
                    .Where(TableQuery.GenerateFilterCondition(
                        "PartitionKey",
                        QueryComparisons.Equal,
                        partitionKey));

            var result = await cloudTable.ExecuteQuerySegmentedAsync(query, null);
            return result;
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
        Task<IEnumerable<T>> GetEntitiesByPartitionKey(string partitionKey);
        IEnumerable<T> GetEntitiesByRowKey(string rowKey);
        Task<TableResult<T>> GetEntitiesByPartitionKeyWithContinuationTokenAsync(string partitionKey, TableContinuationToken token, int pageSize);
        Task<TableResult<T>> GetEntitiesByRowKeyWithContinuationTokenAsync(string rowKey, TableContinuationToken token);
        T GetEntityByPartitionKeyAndRowKey(string partitionKey, string rowKey);
        Task InsertOrUpdateAsync(T entity);
        Task<IList<TableResult>> Insert(T[] entities);
        Task<IEnumerable<DynamicTableEntity>> Query(string tableName, TableQuery query);
    }
    public class TelemetryStorage : TableEntity
    {
        public TelemetryStorage()
        {

        }

        [JsonProperty("Daywaterusage")]
        public double DayWaterUsage { get; set; }

        [JsonProperty("AvgWaterUsage")]
        public double AvgWaterUsage { get; set; }

    }
}