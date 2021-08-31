using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GolemUI.Utils
{
    public abstract class LookupCache<TKey, TValue>
    {
        private int _maxCap;
        private SemaphoreSlim _semaphore;
        private Dictionary<TKey, TValue> _data;

        protected LookupCache(int maxCap = 10)
        {
            _maxCap = maxCap;
            _semaphore = new SemaphoreSlim(1);
            _data = new Dictionary<TKey, TValue>();
        }

        public abstract Task<TValue> Fetch(TKey key);


        public async Task<TValue> Get(TKey key)
        {
            if (_data.TryGetValue(key, out TValue value))
            {
                return value;
            }
            await _semaphore.WaitAsync();
            try
            {
                if (_data.TryGetValue(key, out TValue value2))
                {
                    return value2;
                }
                TValue resolvedValue = await Fetch(key);
                if (_data.Count >= _maxCap)
                {
                    _data.Clear();
                }
                _data.Add(key, resolvedValue);
                return resolvedValue;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private class FuncLookup : LookupCache<TKey, TValue>
        {
            private readonly Func<TKey, Task<TValue>> _func;

            public FuncLookup(Func<TKey, Task<TValue>> func)
            {
                _func = func;
            }

            public override Task<TValue> Fetch(TKey key)
            {
                return _func(key);
            }
        }

        public static LookupCache<TKey, TValue> FromFunc(Func<TKey, Task<TValue>> func)
        {
            return new FuncLookup(func);
        }
    }
}
