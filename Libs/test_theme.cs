using System;
using System.IO;
using System.Reflection;

class Test {
    static void Main() {
        var asm = Assembly.LoadFrom(@"e:\lq\AuditAI\Libs\ThemeResource.dll");
        var type = asm.GetType("Auditai.ThemeResource.Properties.Resource1");
        var prop = type.GetProperty("auditai_Office2013LightGray");
        var val = prop.GetValue(null);
        byte[] bytes = val as byte[];
        Console.WriteLine("Type: " + (val?.GetType().FullName ?? "null"));
        Console.WriteLine("Length: " + (bytes?.Length ?? 0));
        if (bytes != null && bytes.Length > 0) {
            // Check if first few bytes look like XML
            string header = System.Text.Encoding.UTF8.GetString(bytes, 0, Math.Min(100, bytes.Length));
            Console.WriteLine("Header: " + header);
        }
    }
}
