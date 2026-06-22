﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

/// <summary>
/// 跨项目数据引用的两级缓存系统（内存 + 磁盘）
/// </summary>
public class CrossProjectRefCache
{
    private class CacheEntry
    {
        public List<List<object>> Data { get; set; }
        public DateTime CachedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime SourceFileLastWriteTime { get; set; }
        public long SourceFileSize { get; set; }
        public int HitCount { get; set; }
    }

    private class DiskCacheEntry
    {
        public List<List<object>> Data { get; set; }
        public DateTime CachedAt { get; set; }
        public string SourceProjectPath { get; set; }
        public long SourceFileLastWriteTimeTicks { get; set; }
    }

    private static readonly ConcurrentDictionary<Id64, CacheEntry> _memoryCache = new ConcurrentDictionary<Id64, CacheEntry>();
    private static int _totalRequests = 0;
    private static int _cacheHits = 0;

    private readonly string _cacheDir;

    public CrossProjectRefCache(long userId)
    {
        _cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", userId.ToString(), "cache");
        if (!Directory.Exists(_cacheDir))
        {
            Directory.CreateDirectory(_cacheDir);
        }
    }

    /// <summary>获取缓存的数据</summary>
    public List<List<object>> GetCachedData(Id64 refId, string sourceProjectPath, int cacheDurationSeconds = 60)
    {
        _totalRequests++;

        // 1. 检查内存缓存
        if (_memoryCache.TryGetValue(refId, out var memEntry))
        {
            if (DateTime.Now < memEntry.ExpiresAt)
            {
                // 检查源文件时间戳是否变化
                var fileInfo = new FileInfo(sourceProjectPath);
                if (fileInfo.Exists && fileInfo.LastWriteTimeUtc == memEntry.SourceFileLastWriteTime)
                {
                    memEntry.HitCount++;
                    _cacheHits++;
                    return memEntry.Data;
                }
            }
            // 过期或文件已变更，移除内存缓存
            _memoryCache.TryRemove(refId, out _);
        }

        // 2. 检查磁盘缓存
        var diskPath = GetDiskCachePath(refId);
        if (File.Exists(diskPath))
        {
            try
            {
                var diskEntry = JsonConvert.DeserializeObject<DiskCacheEntry>(File.ReadAllText(diskPath));
                if (diskEntry != null && diskEntry.SourceProjectPath == sourceProjectPath)
                {
                    var fileInfo = new FileInfo(sourceProjectPath);
                    if (fileInfo.Exists && fileInfo.LastWriteTimeUtc.Ticks == diskEntry.SourceFileLastWriteTimeTicks)
                    {
                        // 磁盘缓存有效，恢复到内存缓存
                        var expiresAt = diskEntry.CachedAt.AddSeconds(cacheDurationSeconds);
                        if (DateTime.Now < expiresAt)
                        {
                            _memoryCache[refId] = new CacheEntry
                            {
                                Data = diskEntry.Data,
                                CachedAt = diskEntry.CachedAt,
                                ExpiresAt = expiresAt,
                                SourceFileLastWriteTime = fileInfo.LastWriteTimeUtc,
                                SourceFileSize = fileInfo.Length,
                                HitCount = 1
                            };
                            _cacheHits++;
                        }
                        return diskEntry.Data;
                    }
                }
            }
            catch { }
        }

        return null; // 缓存未命中
    }

    /// <summary>设置缓存</summary>
    public void SetCache(Id64 refId, List<List<object>> data, string sourceProjectPath, int cacheDurationSeconds = 60)
    {
        var fileInfo = new FileInfo(sourceProjectPath);
        var expiresAt = DateTime.Now.AddSeconds(cacheDurationSeconds);

        // 内存缓存
        _memoryCache[refId] = new CacheEntry
        {
            Data = data,
            CachedAt = DateTime.Now,
            ExpiresAt = expiresAt,
            SourceFileLastWriteTime = fileInfo.Exists ? fileInfo.LastWriteTimeUtc : DateTime.MinValue,
            SourceFileSize = fileInfo.Exists ? fileInfo.Length : 0,
            HitCount = 0
        };

        // 数据量超过 1000 行时写入磁盘缓存
        if (data != null && data.Count > 1000)
        {
            var diskPath = GetDiskCachePath(refId);
            var diskEntry = new DiskCacheEntry
            {
                Data = data,
                CachedAt = DateTime.Now,
                SourceProjectPath = sourceProjectPath,
                SourceFileLastWriteTimeTicks = fileInfo.Exists ? fileInfo.LastWriteTimeUtc.Ticks : 0
            };
            try
            {
                File.WriteAllText(diskPath, JsonConvert.SerializeObject(diskEntry));
            }
            catch { }
        }
    }

    /// <summary>使指定引用的缓存失效</summary>
    public void Invalidate(Id64 refId)
    {
        _memoryCache.TryRemove(refId, out _);
        var diskPath = GetDiskCachePath(refId);
        if (File.Exists(diskPath))
        {
            try { File.Delete(diskPath); } catch { }
        }
    }

    /// <summary>使所有缓存失效</summary>
    public void InvalidateAll()
    {
        _memoryCache.Clear();
        if (Directory.Exists(_cacheDir))
        {
            try
            {
                foreach (var file in Directory.GetFiles(_cacheDir, "cross_ref_*.json"))
                {
                    try { File.Delete(file); } catch { }
                }
            }
            catch { }
        }
    }

    /// <summary>缓存统计信息</summary>
    public class CacheStats
    {
        public int TotalRequests { get; set; }
        public int CacheHits { get; set; }
        public double HitRate { get; set; }
        public int MemCacheCount { get; set; }
    }

    /// <summary>获取缓存统计信息</summary>
    public CacheStats GetCacheStats()
    {
        var hitRate = _totalRequests > 0 ? (double)_cacheHits / _totalRequests * 100 : 0;
        return new CacheStats
        {
            TotalRequests = _totalRequests,
            CacheHits = _cacheHits,
            HitRate = Math.Round(hitRate, 1),
            MemCacheCount = _memoryCache.Count
        };
    }

    private string GetDiskCachePath(Id64 refId)
    {
        return Path.Combine(_cacheDir, $"cross_ref_{refId.Value}.json");
    }
}