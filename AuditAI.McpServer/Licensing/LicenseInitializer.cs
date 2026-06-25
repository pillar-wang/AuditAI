﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace AuditAI.McpServer.Licensing
{
    /// <summary>
    /// 无头许可初始化器
    /// 在无 UI 环境下初始化 C1 Studio 和 TX TextControl 许可
    /// </summary>
    public static class LicenseInitializer
    {
        private static bool _initialized;

        /// <summary>
        /// 初始化所有许可（C1 + TX TextControl）
        /// 应在 Program.Main() 早期调用，在任何 C1/TX 组件使用之前
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            SuppressC1EvalDialog();
            Console.Error.WriteLine("[Licensing] C1 许可补丁已应用");
        }

        // ★ C1 评估版抑制方案（双层防护）
        // 方案一：通过 AssemblyLoad 事件 patch C1 组件的许可证字段
        // 方案二：Win32 窗口扫描作为后备方案

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        private const uint WM_CLOSE = 0x0010;

        private static void SuppressC1EvalDialog()
        {
            // 方案一：注册 AssemblyLoad 事件，在 C1 程序集加载时 patch 许可证字段
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

            // 预加载所有 C1 程序集，确保在组件使用前就完成 patch
            // 这样可以避免首次使用时弹窗（AssemblyLoad 事件可能在组件静态构造函数之后才触发）
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                var c1Assemblies = System.IO.Directory.GetFiles(appDir, "C1.*.dll")
                    .Concat(System.IO.Directory.GetFiles(appDir, "C1.Win.*.dll"))
                    .Concat(System.IO.Directory.GetFiles(appDir, "C1.C1*.dll"))
                    .Distinct()
                    .ToArray();
                foreach (var dllPath in c1Assemblies)
                {
                    try
                    {
                        var asm = Assembly.LoadFrom(dllPath);
                        PatchC1LicenseFields(asm);
                    }
                    catch { }
                }
            }
            catch { }

            // Patch 已经加载的 C1 程序集
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = asm.GetName().Name;
                if (name.StartsWith("C1.Win") || name.StartsWith("C1.C1"))
                {
                    PatchC1LicenseFields(asm);
                }
            }

            // 方案二：后台线程扫描窗口作为后备方案
            var thread = new System.Threading.Thread(() =>
            {
                uint currentPid = (uint)Process.GetCurrentProcess().Id;
                while (true)
                {
                    try
                    {
                        EnumWindows((hWnd, lParam) =>
                        {
                            try
                            {
                                GetWindowThreadProcessId(hWnd, out uint pid);
                                if (pid != currentPid) return true;

                                var sb = new System.Text.StringBuilder(256);
                                GetWindowText(hWnd, sb, 256);
                                string title = sb.ToString();

                                if (title.Contains("ComponentOne") ||
                                    title.Contains("评估") ||
                                    title.IndexOf("Evaluation", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    title.IndexOf("Trial", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    title.IndexOf("License", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                                }
                            }
                            catch { }
                            return true;
                        }, IntPtr.Zero);
                    }
                    catch { }
                    System.Threading.Thread.Sleep(300);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            try
            {
                Assembly loadedAssembly = args.LoadedAssembly;
                string assemblyName = loadedAssembly.GetName().Name;

                // 处理 C1 相关的程序集
                if (assemblyName.StartsWith("C1.Win") || assemblyName.StartsWith("C1.C1"))
                {
                    PatchC1LicenseFields(loadedAssembly);
                }

                // 处理 TXTextControl 程序集：patch 许可证验证
                if (assemblyName.StartsWith("TXTextControl"))
                {
                    PatchTXTextControlLicense(loadedAssembly);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[Licensing] 程序集许可证 Patch 失败: " + ex);
            }
        }

        private static void PatchTXTextControlLicense(Assembly assembly)
        {
            try
            {
                // 遍历 TXTextControl 程序集中的所有类型，找到许可证验证相关的类
                // TXTextControl 使用混淆的命名空间 ᜁ.ᝉ 做许可证验证
                // 其 GetLicense 方法调用 LicenseManager.Validate，抛出 LicenseException
                // 我们需要替换其内部的 License 字段

                foreach (Type type in assembly.GetTypes())
                {
                    string typeName = type.FullName ?? "";

                    // 查找许可证提供者类（混淆名包含 ᝉ 或实现 LicenseProvider）
                    if (type.BaseType != null && type.BaseType.FullName == "System.ComponentModel.LicenseProvider")
                    {
                        // 找到继承 LicenseProvider 的类，尝试设置其缓存的 License 字段
                        foreach (var field in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public))
                        {
                            try
                            {
                                if (field.FieldType == typeof(System.ComponentModel.License))
                                {
                                    field.SetValue(null, new TXFakeLicense());
                                }
                            }
                            catch { }
                        }
                    }

                    // 查找 ServerTextControl 类，设置其静态 license 字段
                    if (type.Name == "ServerTextControl")
                    {
                        foreach (var field in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public))
                        {
                            try
                            {
                                if (field.FieldType == typeof(System.ComponentModel.License))
                                {
                                    field.SetValue(null, new TXFakeLicense());
                                }
                            }
                            catch { }
                        }
                    }
                }

                // 关键：Hook LicenseManager 的内部验证逻辑
                // 通过反射替换 licenseCache，使 Validate 总是成功
                // 注意：McpServer 项目未直接引用 TXTextControl.Server.dll，
                // 故通过反射按类型名查找 ServerTextControl（与 typeof 运行时等价）
                try
                {
                    var lmType = typeof(System.ComponentModel.LicenseManager);

                    // 找到内部的 Hashtable licenseCache 字段
                    var cacheField = lmType.GetField("licenseCache",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                    if (cacheField != null)
                    {
                        var cache = cacheField.GetValue(null) as System.Collections.Hashtable;
                        if (cache != null)
                        {
                            // 预注册 ServerTextControl 的许可证
                            Type serverTextControlType = FindServerTextControlType();
                            if (serverTextControlType != null)
                            {
                                cache[serverTextControlType] = new TXFakeLicense();
                            }
                        }
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[Licensing] TXTextControl License Patch failed: " + ex);
            }
        }

        /// <summary>
        /// 通过反射查找 TXTextControl.ServerTextControl 类型。
        /// McpServer 项目未直接引用 TXTextControl.Server.dll，故运行时按类型名查找，
        /// 必要时从应用程序目录加载 TXTextControl.Server.dll。
        /// </summary>
        private static Type FindServerTextControlType()
        {
            // 优先从已加载的程序集中查找
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType("TXTextControl.ServerTextControl");
                if (t != null) return t;
            }
            // 尝试从应用程序目录加载 TXTextControl.Server.dll
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string serverDll = System.IO.Path.Combine(appDir, "TXTextControl.Server.dll");
                if (System.IO.File.Exists(serverDll))
                {
                    var asm = Assembly.LoadFrom(serverDll);
                    return asm.GetType("TXTextControl.ServerTextControl");
                }
            }
            catch { }
            return null;
        }

        /// <summary>假的 License 对象，用于绕过 TXTextControl 许可证验证</summary>
        private class TXFakeLicense : System.ComponentModel.License
        {
            public override string LicenseKey => "TXTextControl.Valid.License";
            public override void Dispose()
            {
                // 无需释放资源，此对象为绕过许可证验证的虚假实现。
                // License.Dispose 是抽象方法，不能调用 base.Dispose()。
            }
        }

        private static void PatchC1LicenseFields(Assembly assembly)
        {
            PatchC1LicenseFieldsCore(assembly);
        }

        /// <summary>
        /// C1 许可证 Patch 核心逻辑（已修正命名空间和字段名）。
        /// 反编译 C1.Win.C1Command.4.dll 确认实际命名空间为 C1.Util.Licensing（不是 C1.License）。
        /// </summary>
        private static void PatchC1LicenseFieldsCore(Assembly assembly)
        {
            try
            {
                // 1. Patch ProviderInfo.m_a = true（Nag 已显示标志，跳过弹窗逻辑）
                // 反编译确认：ProviderInfo.Validate() 中 if (!ProviderInfo.m_a) 控制是否调用 Nag() 弹窗
                // 设置为 true 后，Validate() 会进入 else 分支，不再弹窗
                Type providerInfoType = assembly.GetType("C1.Util.Licensing.ProviderInfo");
                if (providerInfoType != null)
                {
                    // 原始 C1 DLL 中字段名为 "a"（经 Mono.Cecil 反编译验证），不是 "m_a"
                    FieldInfo maField = providerInfoType.GetField("a",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    if (maField != null)
                    {
                        maField.SetValue(null, true);
                    }
                }

                // 2. Patch SafeLicenseContext.a（Hashtable 缓存）预填许可证密钥
                // 反编译确认：ValidateRuntime() 通过 SafeLicenseContext.GetSavedLicenseKey() 获取密钥
                // 若返回 null 则判定为 Unlicensed；预填可使 ValidateRuntime 返回 Valid
                Type safeLicenseContextType = assembly.GetType("C1.Util.Licensing.SafeLicenseContext");
                if (safeLicenseContextType != null)
                {
                    // 原始 C1 DLL 中字段名为 "a"（Hashtable），不是 "m_a"
                    FieldInfo cacheField = safeLicenseContextType.GetField("a",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    if (cacheField != null)
                    {
                        var cache = cacheField.GetValue(null) as System.Collections.Hashtable;
                        if (cache == null)
                        {
                            cache = new System.Collections.Hashtable();
                            cacheField.SetValue(null, cache);
                        }
                        // 标记已处理（具体类型密钥在首次 Validate 时由 .licenses 资源填充）
                    }
                }

                // 3. 触发 ProductLicense 类型初始化（确保静态构造函数已执行）
                // 注意：ProductLicense 没有"许可证状态"静态字段，p 是加密用的 byte[]，不应修改
                Type productLicenseType = assembly.GetType("C1.Util.Licensing.ProductLicense");
                if (productLicenseType != null)
                {
                    try
                    {
                        // 触发静态构造函数，避免后续首次访问时引发竞态
                        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(productLicenseType.TypeHandle);
                    }
                    catch { }
                }

                // 4. Patch LicenseInfo 的评估天数字段默认值
                // 反编译确认：LicenseInfo.evalDaysElapsed 默认 int.MaxValue，EvaluationDaysLeft = 30 - evalDaysElapsed
                // 当 evalDaysElapsed = int.MaxValue 时 EvaluationDaysLeft 为负数，触发 GetStop = true 并抛 LicenseException
                // 由于 evalDaysElapsed 是实例字段，无法静态 patch，但 ProviderInfo.m_a=true 已能阻止弹窗和异常
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[Licensing] C1 License Patch failed for assembly: " + assembly.FullName + " - " + ex);
            }
        }
    }
}
