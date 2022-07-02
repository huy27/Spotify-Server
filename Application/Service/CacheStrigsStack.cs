using Application.IService;
using Microsoft.Extensions.Configuration;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service
{
    public class CacheStrigsStack : ICacheStrigsStack
    {
        private readonly RedisEndpoint _redisEndpoint;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _defaultTimeout;

        public CacheStrigsStack(IConfiguration configuration)
        {
            _configuration = configuration;

            _defaultTimeout = TimeSpan.FromMinutes(5);
            var host = _configuration["Redis:Host"];
            var port = int.Parse(_configuration["Redis:Port"]);
            var password = _configuration["Redis:Password"];

            _redisEndpoint = new RedisEndpoint(host, port, password);
        }

        public bool IsKeyExists(string key)
        {
            using (var redisClient = new RedisClient(_redisEndpoint))
            {
                if (redisClient.ContainsKey(key))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void SetString(string key, string value, TimeSpan? timeout = null)
        {
            using (var redisClient = new RedisClient(_redisEndpoint))
            {
                redisClient.SetValue(key, value, timeout != null ? (TimeSpan)timeout : _defaultTimeout);
            }
        }

        public string GetStrings(string key)
        {
            using (var redisClient = new RedisClient(_redisEndpoint))
            {
                return redisClient.GetValue(key);
            }
        }

        public bool StoreList<T>(string key, T value, TimeSpan? timeout = null)
        {
            try
            {
                using (var redisClient = new RedisClient(_redisEndpoint))
                {
                    redisClient.As<T>().SetValue(key, value, timeout != null ? (TimeSpan)timeout : _defaultTimeout);
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T GetList<T>(string key)
        {
            T result;

            using (var client = new RedisClient(_redisEndpoint))
            {
                var wrapper = client.As<T>();

                result = wrapper.GetValue(key);
            }

            return result;
        }

    }
}
