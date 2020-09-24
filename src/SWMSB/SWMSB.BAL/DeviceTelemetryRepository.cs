using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.COMMON;
using SWMSB.DATA;
using SWMSB.DEVICE;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWMSB.BAL
{
    public interface IDeviceTelemetryRepository : IGenericRepository<DeviceTelemetryDocument>
    {
        void TelemetryMsgReceived(DeviceTelemetryMsg msg);
        Task<bool> SampleTelemetryData();

        void StoreTTNTelemetries();
    }

    public class DeviceTelemetryRepository : GenericRepository<DeviceTelemetryDocument>, IDeviceTelemetryRepository
    {
        ILogger logger;

        public MapperConfiguration TelemetryMsgToDocument { get; }
        public MapperConfiguration TelemetryMsgToTelemetryStorage { get; }
        public StorageTableProvider<TelemetryStorage> StorageTableProvider { get; }
        public Config Config { get; set; }

        public DeviceTelemetryRepository(Config _config, ILogger _logger) : base(_config)
        {
            Config = _config;
            logger = _logger;
            StorageTableProvider = new StorageTableProvider<TelemetryStorage>(
                _config.BlobStorageConnectionString,
                _config.StorageTelemetryTableName);
        }

        public async void TelemetryMsgReceived(DeviceTelemetryMsg msg)
        {
            logger.LogInformation("device msg received");

           
            TelemetryStorage _telemetryStorageData = new TelemetryStorage
            {
                DeviceId = msg.Deviceid,
                Type = "tlm",
                WaterUsage = msg.Waterusage,
                Epoch = msg.MessageGenerated,
                PartitionKey = TelemetryStorage.GeneratePartitionKey(msg.MessageGenerated),
                RowKey = TelemetryStorage.GenerateRowKey(msg.Deviceid, msg.MessageGenerated)
            };
            await StorageTableProvider.InsertOrUpdateAsync(_telemetryStorageData);


            await SampleTelemetryData();
            logger.LogInformation("device msg saved in db");

        }

        public async Task<bool> SampleTelemetryData()
        {
            logger.LogInformation("sampling started ");



            var query =
                new TableQuery()
                    .Where(

                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, TelemetryStorage.GeneratePartitionKey(dateTimeUtcNow))

                    );
            var data = await StorageTableProvider.Query(Config.StorageTelemetryTableName, query);
            var xxx = StorageTableExtension.ToDynamicList(data);
            var dddd = JsonConvert.DeserializeObject<TelemetryStorage>(xxx[0]); 
            logger.LogInformation("sampling started ");

            return true;
        }

        public void StoreTTNTelemetries()
        {
           var data =  TTNRestProvider.
           QueryTTNServer<List<TTNDeviceTelemetry>>(Config.TTN_DATA_STORAGE_ENDPOINT, Config.TTN_API_KEY);
            logger.LogInformation($"Count {data.Count}");

        }
    }
}
