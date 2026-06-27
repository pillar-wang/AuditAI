using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

var path = args[0];
var typeName = args[1];
var methodFilter = args.Length > 2 ? args[2] : null;

// Optional output file as last argument (mode "TO_FILE")
string outputFile = null;
if (args.Length > 3 && args[^1].EndsWith(".il"))
{
    outputFile = args[^1];
}

var asm = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadSymbols = false });

// Helper: write a single line to either console or file buffer
var outputLines = outputFile != null ? new List<string>() : null;
void Emit(string line)
{
    if (outputLines != null) outputLines.Add(line);
    else System.Console.WriteLine(line);
}

if (typeName == "_" && methodFilter == "LIST_TYPES")
{
    var pattern = args.Length > 3 ? args[3] : "";
    foreach (var t in asm.MainModule.Types.Where(t => t.Name.Contains(pattern) && !t.Name.Contains("<>")).OrderBy(t => t.FullName))
    {
        var emptyMethods = t.Methods.Where(m => m.HasBody && m.Body.Instructions.Count <= 1).Select(m => m.Name).ToList();
        var flag = emptyMethods.Count > 0 ? $" [EMPTY: {string.Join(",", emptyMethods)}]" : "";
        System.Console.WriteLine($"{t.FullName} (Methods={t.Methods.Count}){flag}");
    }
    return;
}

var type = asm.MainModule.GetType(typeName);
if (type == null) { System.Console.Error.WriteLine($"Type '{typeName}' not found"); return; }

if (methodFilter == "LIST_ALL")
{
    foreach (var m in type.Methods.OrderBy(m => m.Name))
    {
        System.Console.WriteLine($"  {m.Name} (HasBody={m.HasBody}, IL={(m.HasBody ? m.Body.Instructions.Count.ToString() : "N/A")})");
    }
    return;
}

if (methodFilter == "LIST_NESTED")
{
    foreach (var nt in type.NestedTypes)
    {
        System.Console.WriteLine($"NESTED: {nt.FullName}");
        foreach (var m in nt.Methods.OrderBy(m => m.Name))
        {
            System.Console.WriteLine($"    {m.Name} (HasBody={m.HasBody}, IL={(m.HasBody ? m.Body.Instructions.Count.ToString() : "N/A")})");
        }
    }
    return;
}

// DUMP_TYPE: dump all methods (IL) of a type, optionally to file
if (methodFilter == "DUMP_TYPE")
{
    // Print fields first
    Emit($"=== TYPE: {type.FullName} ===");
    Emit($"Fields ({type.Fields.Count}):");
    foreach (var f in type.Fields.OrderBy(f => f.Name))
    {
        Emit($"  {f.FieldType.FullName} {f.Name}");
    }
    Emit("");

    foreach (var m in type.Methods.OrderBy(m => m.Name))
    {
        Emit($"--- {m.FullName} ---");
        Emit($"HasBody={m.HasBody} RVA=0x{m.RVA:X8} IL={m.Body?.Instructions.Count}");
        if (!m.HasBody) { Emit(""); continue; }

        if (m.Body.Variables.Count > 0)
        {
            Emit("Locals:");
            for (int idx = 0; idx < m.Body.Variables.Count; idx++)
            {
                Emit($"  [{idx}] {m.Body.Variables[idx].VariableType.FullName}");
            }
        }
        if (m.Body.ExceptionHandlers.Count > 0)
        {
            Emit("ExceptionHandlers:");
            foreach (var h in m.Body.ExceptionHandlers)
            {
                Emit($"  {h.HandlerType} try=IL_{h.TryStart?.Offset:X4}..IL_{h.TryEnd?.Offset:X4} handler=IL_{h.HandlerStart?.Offset:X4}..IL_{h.HandlerEnd?.Offset:X4}");
            }
        }
        foreach (var insn in m.Body.Instructions)
        {
            string operand;
            if (insn.OpCode == OpCodes.Ldstr && insn.Operand is string s)
            {
                var escaped = s.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
                operand = "\"" + escaped + "\"";
            }
            else
            {
                operand = insn.Operand?.ToString()?.Replace("\n", "\\n");
            }
            Emit($"IL_{insn.Offset:X4}: {insn.OpCode,-14} {operand}");
        }
        Emit("");
    }

    if (outputFile != null)
    {
        System.IO.File.WriteAllLines(outputFile, outputLines);
        System.Console.WriteLine($"Written {outputLines.Count} lines to {outputFile}");
    }
    return;
}

// methodFilter is a method name; print IL
var method = type.Methods.FirstOrDefault(m => m.Name == methodFilter);
if (method == null) { System.Console.Error.WriteLine($"Method '{methodFilter}' not found"); return; }

Emit($"--- {method.FullName} ---");
Emit($"HasBody={method.HasBody} RVA=0x{method.RVA:X8} IL={method.Body?.Instructions.Count}");
if (!method.HasBody) return;

// Print locals
if (method.Body.Variables.Count > 0)
{
    Emit("Locals:");
    for (int idx = 0; idx < method.Body.Variables.Count; idx++)
    {
        Emit($"  [{idx}] {method.Body.Variables[idx].VariableType.FullName}");
    }
}

// Print exception handlers
if (method.Body.ExceptionHandlers.Count > 0)
{
    Emit("ExceptionHandlers:");
    foreach (var h in method.Body.ExceptionHandlers)
    {
        Emit($"  {h.HandlerType} try=IL_{h.TryStart?.Offset:X4}..IL_{h.TryEnd?.Offset:X4} handler=IL_{h.HandlerStart?.Offset:X4}..IL_{h.HandlerEnd?.Offset:X4}");
    }
}

foreach (var insn in method.Body.Instructions)
{
    string operand;
    if (insn.OpCode == OpCodes.Ldstr && insn.Operand is string s)
    {
        var escaped = s.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        operand = "\"" + escaped + "\"";
    }
    else
    {
        operand = insn.Operand?.ToString()?.Replace("\n", "\\n");
        if (operand?.Length > 200) operand = operand[..200] + "...";
    }
    Emit($"IL_{insn.Offset:X4}: {insn.OpCode,-14} {operand}");
}

if (outputFile != null)
{
    System.IO.File.WriteAllLines(outputFile, outputLines);
    System.Console.WriteLine($"Written {outputLines.Count} lines to {outputFile}");
}
