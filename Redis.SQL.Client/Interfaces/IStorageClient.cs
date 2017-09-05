﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.SQL.Client.Interfaces
{
    internal interface IStorageClient
    {
        Task<string> GetValue(string key);
        Task<bool> StoreValue<T>(string key, T value);
        Task<long> IncrementValue(string key);
        Task<string> GetHashField(string hashSet, string key);
        Task<bool> StoreHashField<T>(string hashSet, string key, T value);
        Task<string> GetListElementByIndex(string key, int index);
        Task<long> AddToListTail<T>(string key, T value);
        Task<long> AddToListHead<T>(string key, T value);
        Task<bool> SetContains<T>(string key, T value);
        Task<bool> AddToSet<T>(string key, T value);
        Task<IEnumerable<string>> GetSortedSetElementsByScore(string key, double minScore, double maxScore);
        Task<IEnumerable<string>> GetSortedSetElementsByIndex(string key, long startIndex, long endIndex);
        Task<IEnumerable<string>> GetSortedSetElementsByValue(string key, string minValue, string maxValue);
        Task<bool> AddToSortedSet<T>(string key, T value, double score = 0D);
    }
}
