﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿namespace AuditAI.McpServer.Protocol
{
    /// <summary>
    /// JSON-RPC 2.0 标准错误码
    /// </summary>
    public static class JsonRpcError
    {
        public const int ParseError = -32700;
        public const int InvalidRequest = -32600;
        public const int MethodNotFound = -32601;
        public const int InvalidParams = -32602;
        public const int InternalError = -32603;

        public static string GetMessage(int code)
        {
            switch (code)
            {
                case ParseError: return "Parse error";
                case InvalidRequest: return "Invalid Request";
                case MethodNotFound: return "Method not found";
                case InvalidParams: return "Invalid params";
                case InternalError: return "Internal error";
                default: return "Unknown error";
            }
        }
    }
}
