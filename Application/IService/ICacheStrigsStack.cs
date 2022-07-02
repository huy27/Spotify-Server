using System;
using System.Collections.Generic;
using System.Text;

namespace Application.IService
{
    public interface ICacheStrigsStack
    {
        bool IsKeyExists(string key);
        void SetString(string key, string value, TimeSpan? timeout = null);
        string GetStrings(string key);
        bool StoreList<T>(string key, T value, TimeSpan? timeout = null);
        T GetList<T>(string key);
    }
}
