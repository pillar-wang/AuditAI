﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Concurrent;
using System.Threading;

namespace AuditAI.McpServer.Threading
{
    /// <summary>
    /// STA 线程池
    /// 提供专用 STA 线程执行需要 COM 组件（如 TX TextControl）的操作
    /// </summary>
    public static class StaThreadPool
    {
        private static readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
        private static readonly Thread _staThread;
        private static readonly AutoResetEvent _idleSignal = new AutoResetEvent(false);

        static StaThreadPool()
        {
            _staThread = new Thread(StaThreadProc)
            {
                IsBackground = true,
                Name = "STA-Worker"
            };
            _staThread.SetApartmentState(ApartmentState.STA);
            _staThread.Start();
        }

        /// <summary>
        /// STA 线程主循环
        /// </summary>
        private static void StaThreadProc()
        {
            try
            {
                foreach (var action in _queue.GetConsumingEnumerable())
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[STA] 线程执行异常: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[STA] 线程致命异常: {ex}");
            }
        }

        /// <summary>
        /// 将操作提交到 STA 线程执行（异步，不等待结果）
        /// </summary>
        public static void QueueAction(Action action)
        {
            _queue.Add(action);
        }

        /// <summary>
        /// 将操作提交到 STA 线程执行（同步，等待结果）
        /// </summary>
        public static void QueueActionSync(Action action)
        {
            Exception error = null;
            using (var done = new ManualResetEventSlim(false))
            {
                _queue.Add(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                    finally
                    {
                        done.Set();
                    }
                });
                done.Wait();
            }
            if (error != null)
                throw error;
        }
    }

    /// <summary>
    /// STA 线程执行器
    /// 提供同步执行并返回结果的 API
    /// </summary>
    public static class StaRunner
    {
        /// <summary>
        /// 在 STA 线程上执行操作并返回结果
        /// </summary>
        public static T Run<T>(Func<T> func)
        {
            T result = default(T);
            Exception error = null;
            using (var done = new ManualResetEventSlim(false))
            {
                StaThreadPool.QueueAction(() =>
                {
                    try
                    {
                        result = func();
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                    finally
                    {
                        done.Set();
                    }
                });
                done.Wait();
            }
            if (error != null)
                throw error;
            return result;
        }

        /// <summary>
        /// 在 STA 线程上执行操作（无返回值）
        /// </summary>
        public static void Run(Action action)
        {
            StaThreadPool.QueueActionSync(action);
        }
    }
}
