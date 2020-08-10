using System;
using System.Collections.Generic;
using System.Text;
using Conductor.Domain.Interfaces;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;

namespace Conductor.Domain.Services
{
    public class MemoryCache : ICache
    {
        private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _internal;

        public MemoryCache()
        {
            _internal = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());
        }

        public T GetHash<T>([NotNull] string key, [NotNull] string field)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (field == null) throw new ArgumentNullException(nameof(field));
            
            var dicts = _internal.Get<Dictionary<string, object>>(key);
            if (dicts != null && dicts.ContainsKey(field))
            {
                return (T) dicts[field];
            }

            return default(T);
        }

        public string GetString([NotNull] string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            
            return _internal.Get<string>(key);
        }

        public T GetValue<T>([NotNull] string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            
            return _internal.Get<T>(key);
        }

        public bool IsExists([NotNull] string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            
            return _internal.Get(key) != null;
        }

        public void Remove([NotNull] string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            
            _internal.Remove(key);
        }

        public void RemoveField([NotNull] string key, [NotNull] string field)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (field == null) throw new ArgumentNullException(nameof(field));
            
            var dicts = _internal.Get<Dictionary<string, object>>(key);
            if (dicts != null && dicts.ContainsKey(field))
            {
                dicts.Remove(field);
                _internal.Set(key, dicts);
            }
        }

        public void SetHash<T>([NotNull] string key, [NotNull] string field, T value, TimeSpan? timeout = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (field == null) throw new ArgumentNullException(nameof(field));

            var dicts = _internal.Get<Dictionary<string, object>>(key) ?? new Dictionary<string, object>();
            if (dicts.ContainsKey(field))
            {
                throw new ArgumentException("field is exists");
            }

            dicts.Add(field, value);

            MemoryCacheEntryOptions options = null;
            if (timeout.HasValue)
            {
                options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(timeout.Value);
            }

            _internal.Set(key, dicts, options);
        }

        public void SetString([NotNull] string key, string value, TimeSpan? timeout = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            
            MemoryCacheEntryOptions options = null;
            if (timeout.HasValue)
            {
                options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(timeout.Value);
            }

            _internal.Set(key, value, options);
        }

        public void SetValue<T>([NotNull] string key, T value, TimeSpan? timeout = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            
            MemoryCacheEntryOptions options = null;
            if (timeout.HasValue)
            {
                options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(timeout.Value);
            }

            _internal.Set(key, value, options);
        }
    }
}