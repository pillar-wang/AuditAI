﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 响应断言工具集（Task 5）。
    /// 对上一步 cloud_* 调用捕获的 SessionState.LastResponse 做结构化校验。
    /// 所有断言结果会被记录到 SessionState.AssertionResults 并以 JSON 字符串形式返回。
    /// </summary>
    public static class AssertionTools
    {
        /// <summary>
        /// 注册所有断言工具。
        /// </summary>
        public static void Register()
        {
            RegisterAssertStatus();
            RegisterAssertJsonPath();
            RegisterAssertErrorCode();
            RegisterAssertHeader();
            RegisterAssertResponseTime();
            RegisterAssertBodyContains();
            RegisterAssertBodyEmpty();
            RegisterAssertBodyNotEmpty();
        }

        // =====================================================================
        // 辅助方法
        // =====================================================================

        /// <summary>
        /// 根据 sessionName 解析对应的 LastResponse。
        /// sessionName 为空时使用全局 SessionState.LastResponse；否则查指定 session 的 LastResponse。
        /// </summary>
        private static TestResponse ResolveLastResponse(string sessionName)
        {
            if (string.IsNullOrEmpty(sessionName))
            {
                return SessionState.LastResponse;
            }
            TestSession session = SessionState.GetOrCreateSession(sessionName);
            return session.LastResponse ?? SessionState.LastResponse;
        }

        /// <summary>
        /// 记录断言结果到 SessionState.AssertionResults，并返回标准 JSON 字符串。
        /// </summary>
        private static string RecordAssertion(bool passed, string actual, string expected, string message, string assertionType)
        {
            DateTime ts = DateTime.Now;
            var result = new AssertionResult
            {
                AssertionType = assertionType,
                Passed = passed,
                Actual = actual,
                Expected = expected,
                Message = message,
                Timestamp = ts
            };
            try
            {
                if (SessionState.AssertionResults != null)
                {
                    SessionState.AssertionResults.Add(result);
                }
            }
            catch { /* 防御性：AssertionResults 不可用时仅忽略 */ }
            return JsonConvert.SerializeObject(new
            {
                passed = passed,
                actual = actual,
                expected = expected,
                message = message,
                assertionType = assertionType,
                timestamp = ts.ToString("o")
            }, Formatting.Indented);
        }

        /// <summary>
        /// 构造"无响应"失败结果。
        /// </summary>
        private static string NoResponseResult(string assertionType)
        {
            return RecordAssertion(false, null, null, "No response captured yet. Call a cloud_* tool first.", assertionType);
        }

        // =====================================================================
        // SubTask 5.2: assert_status
        // =====================================================================

        private static void RegisterAssertStatus()
        {
            ToolRegistry.Register("assert_status",
                "断言上一步 cloud_* 调用的 HTTP 状态码等于 expected。返回 {passed, actual, expected, message}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["expected"] = new JObject { ["type"] = "integer", ["description"] = "期望的 HTTP 状态码（如 200/401/403/402/429/500）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选，默认断言主会话的 LastResponse）" }
                    },
                    ["required"] = new JArray { "expected" }
                },
                (args) => AssertStatusImpl(args));
        }

        private static string AssertStatusImpl(JObject args)
        {
            int expected = args["expected"] != null ? args["expected"].Value<int>() : 0;
            string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
            TestResponse resp = ResolveLastResponse(sessionName);
            if (resp == null) return NoResponseResult("status");

            int actual = resp.StatusCode;
            bool passed = actual == expected;
            string message = passed ? "Status code matches" : "Status code mismatch";
            return RecordAssertion(passed, actual.ToString(), expected.ToString(), message, "status");
        }

        // =====================================================================
        // SubTask 5.3: assert_json_path
        // =====================================================================

        private static void RegisterAssertJsonPath()
        {
            ToolRegistry.Register("assert_json_path",
                "断言上一步响应体的 JSON 路径值等于 expected。使用 Newtonsoft.Json JPath 语法（如 $.Item1.Token）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["path"] = new JObject { ["type"] = "string", ["description"] = "JSON 路径（如 $.Item1.Token 或 $.data[0].id）" },
                        ["expected"] = new JObject { ["description"] = "期望值（可为 string/number/boolean/null）" },
                        ["operator"] = new JObject { ["type"] = "string", ["description"] = "比较操作符（默认 eq，可选 eq/ne/contains/not_empty/exists）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "path" }
                },
                (args) => AssertJsonPathImpl(args));
        }

        private static string AssertJsonPathImpl(JObject args)
        {
            string path = args["path"] != null ? args["path"].ToString() : null;
            string op = args["operator"] != null ? args["operator"].ToString() : "eq";
            if (string.IsNullOrEmpty(op)) op = "eq";
            JToken expectedToken = args["expected"];
            string expectedStr = FormatToken(expectedToken);
            string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;

            TestResponse resp = ResolveLastResponse(sessionName);
            if (resp == null) return NoResponseResult("json_path");
            if (string.IsNullOrEmpty(path))
            {
                return RecordAssertion(false, null, expectedStr, "Missing required argument: path", "json_path");
            }

            JObject body;
            try
            {
                body = JObject.Parse(resp.Body ?? "{}");
            }
            catch
            {
                return RecordAssertion(false, resp.Body, expectedStr, "Body is not valid JSON", "json_path");
            }

            JToken actualToken;
            try
            {
                actualToken = body.SelectToken(path);
            }
            catch
            {
                return RecordAssertion(false, null, expectedStr, "Invalid JSON path: " + path, "json_path");
            }

            string actualStr = FormatToken(actualToken);
            bool passed;
            string message;
            switch (op)
            {
                case "eq":
                    passed = TokensEqual(actualToken, expectedToken);
                    message = passed ? "Value matches expected" : "Value does not match expected";
                    break;
                case "ne":
                    passed = !TokensEqual(actualToken, expectedToken);
                    message = passed ? "Value differs from expected" : "Value equals expected (should differ)";
                    break;
                case "contains":
                    if (actualToken == null)
                    {
                        passed = false;
                        message = "Actual value is null; cannot check contains";
                    }
                    else
                    {
                        string actualText = actualToken.ToString();
                        string expectedText = expectedToken != null ? expectedToken.ToString() : "";
                        passed = actualText.IndexOf(expectedText, StringComparison.Ordinal) >= 0;
                        message = passed ? "Substring found" : "Substring not found";
                    }
                    break;
                case "not_empty":
                    passed = !IsEmptyToken(actualToken);
                    message = passed ? "Value is non-empty" : "Value is empty or null";
                    break;
                case "exists":
                    passed = actualToken != null;
                    message = passed ? "Path exists" : "Path does not exist";
                    break;
                default:
                    passed = false;
                    message = "Unknown operator: " + op;
                    break;
            }
            return RecordAssertion(passed, actualStr, expectedStr, message, "json_path");
        }

        // =====================================================================
        // SubTask 5.4: assert_error_code
        // =====================================================================

        private static void RegisterAssertErrorCode()
        {
            ToolRegistry.Register("assert_error_code",
                "断言上一步响应体的 error 字段（或 error.code）等于 expected。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["expected"] = new JObject { ["type"] = "string", ["description"] = "期望的 error 值" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "expected" }
                },
                (args) => AssertErrorCodeImpl(args));
        }

        private static string AssertErrorCodeImpl(JObject args)
        {
            string expected = args["expected"] != null ? args["expected"].ToString() : null;
            string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;

            TestResponse resp = ResolveLastResponse(sessionName);
            if (resp == null) return NoResponseResult("error_code");

            string actual = null;
            try
            {
                JObject body = JObject.Parse(resp.Body ?? "{}");
                JToken err = body["error"];
                if (err != null && err.Type != JTokenType.Null)
                {
                    if (err.Type == JTokenType.String)
                    {
                        actual = err.ToString();
                    }
                    else
                    {
                        JToken code = err["code"];
                        if (code != null && code.Type != JTokenType.Null)
                        {
                            actual = code.ToString();
                        }
                    }
                }
            }
            catch
            {
                return RecordAssertion(false, resp.Body, expected, "Body is not valid JSON", "error_code");
            }

            bool passed = string.Equals(actual, expected, StringComparison.Ordinal);
            string message = passed ? "Error code matches" : "Error code mismatch";
            return RecordAssertion(passed, actual, expected, message, "error_code");
        }

        // =====================================================================
        // SubTask 5.5: assert_header
        // =====================================================================

        private static void RegisterAssertHeader()
        {
            ToolRegistry.Register("assert_header",
                "断言上一步响应的指定 Header 等于 expected。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["name"] = new JObject { ["type"] = "string", ["description"] = "Header 名称（如 Content-Type）" },
                        ["expected"] = new JObject { ["type"] = "string", ["description"] = "期望值" },
                        ["operator"] = new JObject { ["type"] = "string", ["description"] = "操作符（默认 eq，可选 eq/contains/exists）" }
                    },
                    ["required"] = new JArray { "name" }
                },
                (args) => AssertHeaderImpl(args));
        }

        private static string AssertHeaderImpl(JObject args)
        {
            string name = args["name"] != null ? args["name"].ToString() : null;
            string expected = args["expected"] != null ? args["expected"].ToString() : null;
            string op = args["operator"] != null ? args["operator"].ToString() : "eq";
            if (string.IsNullOrEmpty(op)) op = "eq";

            TestResponse resp = SessionState.LastResponse;
            if (resp == null) return NoResponseResult("header");
            if (string.IsNullOrEmpty(name))
            {
                return RecordAssertion(false, null, expected, "Missing required argument: name", "header");
            }

            string actual = resp.GetHeader(name);
            bool passed;
            string message;
            switch (op)
            {
                case "eq":
                    passed = string.Equals(actual ?? "", expected ?? "", StringComparison.Ordinal);
                    message = passed ? "Header matches" : "Header mismatch";
                    break;
                case "contains":
                    if (actual == null)
                    {
                        passed = false;
                        message = "Header not present";
                    }
                    else
                    {
                        passed = actual.IndexOf(expected ?? "", StringComparison.Ordinal) >= 0;
                        message = passed ? "Header contains substring" : "Header does not contain substring";
                    }
                    break;
                case "exists":
                    passed = !string.IsNullOrEmpty(actual);
                    message = passed ? "Header exists" : "Header missing";
                    break;
                default:
                    passed = false;
                    message = "Unknown operator: " + op;
                    break;
            }
            return RecordAssertion(passed, actual, expected, message, "header");
        }

        // =====================================================================
        // SubTask 5.6: assert_response_time
        // =====================================================================

        private static void RegisterAssertResponseTime()
        {
            ToolRegistry.Register("assert_response_time",
                "断言上一步响应耗时不超过 maxMs。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["maxMs"] = new JObject { ["type"] = "integer", ["description"] = "最大允许耗时（毫秒）" }
                    },
                    ["required"] = new JArray { "maxMs" }
                },
                (args) => AssertResponseTimeImpl(args));
        }

        private static string AssertResponseTimeImpl(JObject args)
        {
            long maxMs = args["maxMs"] != null ? args["maxMs"].Value<long>() : 0L;

            TestResponse resp = SessionState.LastResponse;
            if (resp == null) return NoResponseResult("response_time");

            long actual = resp.ElapsedMs;
            bool passed = actual <= maxMs;
            string message = passed ? "Response time within limit" : "Response time exceeds limit";
            return RecordAssertion(passed, actual.ToString() + "ms", maxMs.ToString() + "ms", message, "response_time");
        }

        // =====================================================================
        // SubTask 5.7: assert_body_contains
        // =====================================================================

        private static void RegisterAssertBodyContains()
        {
            ToolRegistry.Register("assert_body_contains",
                "断言上一步响应体包含 substring。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["substring"] = new JObject { ["type"] = "string", ["description"] = "期望包含的子串" },
                        ["caseSensitive"] = new JObject { ["type"] = "boolean", ["description"] = "是否区分大小写（默认 true）" }
                    },
                    ["required"] = new JArray { "substring" }
                },
                (args) => AssertBodyContainsImpl(args));
        }

        private static string AssertBodyContainsImpl(JObject args)
        {
            string substring = args["substring"] != null ? args["substring"].ToString() : null;
            bool caseSensitive = args["caseSensitive"] == null || args["caseSensitive"].Value<bool>();

            TestResponse resp = SessionState.LastResponse;
            if (resp == null) return NoResponseResult("body_contains");
            if (substring == null)
            {
                return RecordAssertion(false, resp.Body, null, "Missing required argument: substring", "body_contains");
            }

            string body = resp.Body ?? "";
            string actualText = caseSensitive ? body : body.ToLowerInvariant();
            string expectedText = caseSensitive ? substring : substring.ToLowerInvariant();
            bool passed = actualText.IndexOf(expectedText, StringComparison.Ordinal) >= 0;
            string message = passed ? "Body contains substring" : "Body does not contain substring";
            return RecordAssertion(passed, body, substring, message, "body_contains");
        }

        // =====================================================================
        // SubTask 5.8a: assert_body_empty
        // =====================================================================

        private static void RegisterAssertBodyEmpty()
        {
            ToolRegistry.Register("assert_body_empty",
                "断言上一步响应体为空（长度为 0 或仅空白字符）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => AssertBodyEmptyImpl(args));
        }

        private static string AssertBodyEmptyImpl(JObject args)
        {
            TestResponse resp = SessionState.LastResponse;
            if (resp == null) return NoResponseResult("body_empty");

            string body = resp.Body ?? "";
            bool passed = string.IsNullOrWhiteSpace(body);
            string message = passed ? "Body is empty" : "Body is not empty";
            return RecordAssertion(passed, body, "(empty)", message, "body_empty");
        }

        // =====================================================================
        // SubTask 5.8b: assert_body_not_empty
        // =====================================================================

        private static void RegisterAssertBodyNotEmpty()
        {
            ToolRegistry.Register("assert_body_not_empty",
                "断言上一步响应体非空。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => AssertBodyNotEmptyImpl(args));
        }

        private static string AssertBodyNotEmptyImpl(JObject args)
        {
            TestResponse resp = SessionState.LastResponse;
            if (resp == null) return NoResponseResult("body_not_empty");

            string body = resp.Body ?? "";
            bool passed = !string.IsNullOrWhiteSpace(body);
            string message = passed ? "Body is not empty" : "Body is empty";
            return RecordAssertion(passed, body, "(non-empty)", message, "body_not_empty");
        }

        // =====================================================================
        // JToken 辅助方法
        // =====================================================================

        /// <summary>
        /// 格式化 JToken 为字符串用于 actual/expected 展示。
        /// null 或 JSON null 返回 null，其他类型用 ToString。
        /// </summary>
        private static string FormatToken(JToken token)
        {
            if (token == null) return null;
            if (token.Type == JTokenType.Null) return null;
            return token.ToString();
        }

        /// <summary>
        /// 判断 JToken 是否为空（null/JSON null/空字符串/空数组/空对象）。
        /// </summary>
        private static bool IsEmptyToken(JToken token)
        {
            if (token == null) return true;
            if (token.Type == JTokenType.Null) return true;
            if (token.Type == JTokenType.String)
            {
                return string.IsNullOrEmpty(token.ToString());
            }
            if (token.Type == JTokenType.Array || token.Type == JTokenType.Object)
            {
                return !token.HasValues;
            }
            return false;
        }

        /// <summary>
        /// 比较两个 JToken 是否相等（支持类型转换）。
        /// 两个 null/JSON null 视为相等；数字按数值比较；其他按字符串比较。
        /// </summary>
        private static bool TokensEqual(JToken actual, JToken expected)
        {
            bool actualIsNull = actual == null || actual.Type == JTokenType.Null;
            bool expectedIsNull = expected == null || expected.Type == JTokenType.Null;
            if (actualIsNull && expectedIsNull) return true;
            if (actualIsNull || expectedIsNull) return false;

            double? actualNum = TryGetNumber(actual);
            double? expectedNum = TryGetNumber(expected);
            if (actualNum.HasValue && expectedNum.HasValue)
            {
                return Math.Abs(actualNum.Value - expectedNum.Value) < 1e-9;
            }

            return string.Equals(actual.ToString(), expected.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// 尝试将 JToken 转换为 double（整数/浮点/数字字符串）。
        /// </summary>
        private static double? TryGetNumber(JToken token)
        {
            if (token == null) return null;
            if (token.Type == JTokenType.Integer)
            {
                return (double)token.Value<long>();
            }
            if (token.Type == JTokenType.Float)
            {
                return token.Value<double>();
            }
            if (token.Type == JTokenType.String)
            {
                double d;
                if (double.TryParse(token.ToString(), out d)) return d;
            }
            return null;
        }
    }
}
