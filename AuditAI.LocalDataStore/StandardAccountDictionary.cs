using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Auditai.LocalDataStore
{
    /// <summary>
    /// 标准科目条目
    /// </summary>
    public class StandardAccount
    {
        /// <summary>科目编码（保留前导零，唯一）</summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>标准科目名称</summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>借贷方向：1=借方 -1=贷方 null=未指定</summary>
        [JsonProperty("dc", NullValueHandling = NullValueHandling.Ignore)]
        public int? Dc { get; set; }

        /// <summary>父级科目编码，null 表示一级科目</summary>
        [JsonProperty("parentCode", NullValueHandling = NullValueHandling.Ignore)]
        public string ParentCode { get; set; }
    }

    /// <summary>
    /// 标准科目树节点
    /// </summary>
    public class StandardAccountNode
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? Dc { get; set; }
        public StandardAccountNode Parent { get; set; }
        public List<StandardAccountNode> Children { get; } = new List<StandardAccountNode>();
    }

    /// <summary>
    /// 标准科目字典（第 4 种数据字典，格式同 TableCollectDic/CellCollectDic/LedgerValidateDic）
    /// JSON 结构：{ "Version": n, "Accounts": [ { code, name, dc, parentCode } ] }
    /// </summary>
    public class StandardAccountDictionary
    {
        [JsonProperty("Version")]
        public int Version { get; set; }

        [JsonProperty("Accounts")]
        public List<StandardAccount> Accounts { get; set; } = new List<StandardAccount>();

        private Dictionary<string, StandardAccount> _byCode;

        /// <summary>按科目编码精确索引（编码区分大小写，重复编码后者覆盖前者）</summary>
        public Dictionary<string, StandardAccount> ByCode
        {
            get
            {
                if (_byCode == null)
                {
                    _byCode = new Dictionary<string, StandardAccount>(StringComparer.Ordinal);
                    if (Accounts != null)
                    {
                        foreach (var acc in Accounts)
                        {
                            if (acc == null || string.IsNullOrEmpty(acc.Code)) continue;
                            _byCode[acc.Code] = acc;
                        }
                    }
                }
                return _byCode;
            }
        }

        /// <summary>
        /// 从 JSON 字符串解析字典；空内容返回空实例，非法 JSON 抛异常由调用方处理
        /// </summary>
        public static StandardAccountDictionary Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new StandardAccountDictionary();
            return JsonConvert.DeserializeObject<StandardAccountDictionary>(json) ?? new StandardAccountDictionary();
        }

        /// <summary>
        /// 构建树形结构。parentCode 为空、指向自身或在字典中找不到对应父级的科目视为根节点
        /// </summary>
        public List<StandardAccountNode> GetTree()
        {
            var roots = new List<StandardAccountNode>();
            if (Accounts == null) return roots;

            var nodes = new Dictionary<string, StandardAccountNode>(StringComparer.Ordinal);
            foreach (var acc in Accounts)
            {
                if (acc == null || string.IsNullOrEmpty(acc.Code) || nodes.ContainsKey(acc.Code)) continue;
                nodes[acc.Code] = new StandardAccountNode { Code = acc.Code, Name = acc.Name, Dc = acc.Dc };
            }

            foreach (var acc in Accounts)
            {
                if (acc == null || string.IsNullOrEmpty(acc.Code)) continue;
                var node = nodes[acc.Code];
                if (string.IsNullOrEmpty(acc.ParentCode) || acc.ParentCode == acc.Code || !nodes.ContainsKey(acc.ParentCode))
                {
                    roots.Add(node);
                }
                else
                {
                    node.Parent = nodes[acc.ParentCode];
                    nodes[acc.ParentCode].Children.Add(node);
                }
            }
            return roots;
        }

        /// <summary>
        /// 校验字典数据：code 非空且唯一、name 非空、parentCode 为空或存在于字典且不指向自身（防环）
        /// </summary>
        public bool Validate(out string error)
        {
            error = null;
            if (Accounts == null) return true;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var acc in Accounts)
            {
                if (acc == null)
                {
                    error = "存在空的科目条目";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(acc.Code))
                {
                    error = "存在科目编码为空的条目";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(acc.Name))
                {
                    error = $"科目 {acc.Code} 的名称为空";
                    return false;
                }
                if (!seen.Add(acc.Code))
                {
                    error = $"科目编码重复: {acc.Code}";
                    return false;
                }
            }

            var byCode = ByCode;
            foreach (var acc in Accounts)
            {
                if (acc == null || string.IsNullOrEmpty(acc.ParentCode)) continue;
                if (acc.ParentCode == acc.Code)
                {
                    error = $"科目 {acc.Code} 的父级指向自身";
                    return false;
                }
                if (!byCode.ContainsKey(acc.ParentCode))
                {
                    error = $"科目 {acc.Code} 的父级 {acc.ParentCode} 不存在";
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 从本地文件加载字典；文件不存在返回 null（由调用方处理）
        /// </summary>
        public static StandardAccountDictionary LoadFromFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;
            return Parse(File.ReadAllText(path));
        }
    }
}
