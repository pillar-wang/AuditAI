using System;
using System.IO;
using System.Reflection;

public class ExtractFont
{
    [STAThread]
    public static void Main()
    {
        try
        {
            Assembly asm = Assembly.LoadFile(@"e:\lq\AuditAI\AuditAI\bin\Debug\net48\CommonControls.dll");
            var names = asm.GetManifestResourceNames();
            Console.WriteLine("Resources: " + names.Length);
            foreach (var n in names) Console.WriteLine("  " + n);
            foreach (var n in names)
            {
                if (n.IndexOf("Phosphor", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf(".ttf", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    using (Stream s = asm.GetManifestResourceStream(n))
                    using (FileStream fs = File.Create(@"e:\lq\AuditAI\_icodump\extracted.ttf"))
                    {
                        s.CopyTo(fs);
                        Console.WriteLine("EXTRACTED: " + n + " size=" + s.Length);
                    }
                    return;
                }
            }
            Console.WriteLine("NO TTF FOUND");
        }
        catch (Exception ex) { Console.WriteLine("ERROR: " + ex); }
    }
}
