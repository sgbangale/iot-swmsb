using Microsoft.Extensions.Logging;
using SWMSB.COMMON;
using SWMSB.DATA;
using SWMSB.DEVICE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SWMSB.BAL
{
    public interface IDeviceTwinRepository : IGenericRepository<DeviceTwinDocument>
    {
        void TwinMsgReceived(DeviceTwinMsg msg);
    }

    public class DeviceTwinRepository : GenericRepository<DeviceTwinDocument>, IDeviceTwinRepository
    {
        ILogger logger;
        public DeviceTwinRepository(Config config, ILogger _logger) : base(config)
        {
            logger = _logger;
        }

        public void TwinMsgReceived(DeviceTwinMsg msg)
        {

        }
    }
}
