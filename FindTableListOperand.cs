﻿﻿﻿﻿using System;
using System.Reflection;
using System.IO;

class FindTableListOperand
{
    static void Main()
    {
        string exePath = @"e:\lq\AuditAI\AuditAI\bin\Debug\net462\AuditAI.exe";
        if (!File.Exists(exePath))
        {
            Console.WriteLine("File not found: " + exePath);
            return;
        }
        try
        {
            Assembly a = Assembly.ReflectionOnlyLoadFrom(exePath);
            foreach (Type t in a.GetTypes())
            {
                if (t.Name.Contains("TableList"))
                {
                    Console.WriteLine(t.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
