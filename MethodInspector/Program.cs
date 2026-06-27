using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

var path = args[0];
var typeName = args[1];
var methodFilter = args.Length > 2 ? args[2] : null;

var asm = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadSymbols = false });
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

// methodFilter is a method name; print IL
var method = type.Methods.FirstOrDefault(m => m.Name == methodFilter);
if (method == null) { System.Console.Error.WriteLine($"Method '{methodFilter}' not found"); return; }

System.Console.WriteLine($"--- {method.FullName} ---");
System.Console.WriteLine($"HasBody={method.HasBody} RVA=0x{method.RVA:X8} IL={method.Body?.Instructions.Count}");
if (!method.HasBody) return;

// Print locals
if (method.Body.Variables.Count > 0)
{
    System.Console.WriteLine("Locals:");
    for (int idx = 0; idx < method.Body.Variables.Count; idx++)
    {
        System.Console.WriteLine($"  [{idx}] {method.Body.Variables[idx].VariableType.FullName}");
    }
}

// Print exception handlers
if (method.Body.ExceptionHandlers.Count > 0)
{
    System.Console.WriteLine("ExceptionHandlers:");
    foreach (var h in method.Body.ExceptionHandlers)
    {
        System.Console.WriteLine($"  {h.HandlerType} try=IL_{h.TryStart?.Offset:X4}..IL_{h.TryEnd?.Offset:X4} handler=IL_{h.HandlerStart?.Offset:X4}..IL_{h.HandlerEnd?.Offset:X4}");
    }
}

foreach (var insn in method.Body.Instructions)
{
    var operand = insn.Operand?.ToString()?.Replace("\n", "\\n");
    if (operand?.Length > 200) operand = operand[..200] + "...";
    System.Console.WriteLine($"IL_{insn.Offset:X4}: {insn.OpCode,-14} {operand}");
}
