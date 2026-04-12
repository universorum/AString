using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System;

internal static class KeyValuePairExtensions
{
    extension<TKey, TValue>(KeyValuePair<TKey, TValue> self)
    {
#if !NETSTANDARD2_1_OR_GREATER
        public void Deconstruct(out TKey key, out TValue value)
        {
            key   = self.Key;
            value = self.Value;
        }
#endif
    }
}