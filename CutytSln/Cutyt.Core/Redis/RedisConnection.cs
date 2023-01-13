using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Redis
{
    public static class RedisConnection
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = CreateConnection();

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        private static Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                string cacheConnection = "cutyt.redis.cache.windows.net:6380,password=oabIij7A6ICpEWpLckL8sHS2773uDeBlDAzCaH7VShQ=,ssl=True,abortConnect=False";
                return ConnectionMultiplexer.Connect(cacheConnection);
            });
        }
    }
}
