using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System;

internal static class DictionaryExtensions
{
    extension<TKey, TValue>(Dictionary<TKey, TValue> self)
    {
#if !NETSTANDARD2_1_OR_GREATER
        public bool TryAdd(TKey key, TValue value)
        {
            if (self.ContainsKey(key)) { return false; }

            self.Add(key, value);
            return true;
        }
#endif
    }
}