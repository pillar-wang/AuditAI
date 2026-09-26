using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

// 只做语法级（Parse）校验：不引用被检文件的依赖，因此不会产生"缺类型"类语义错误。
// 用途：在无法整体编译的仓库里，确认改动后的文件本身语法合法（括号/引号/插值/局部函数等）。
var files = args.Length > 0
    ? args
    : throw new ArgumentException("usage: syntaxcheck <file.cs> [file.cs ...]");

var encoding = new UTF8Encoding(false, false);
var totalErrors = 0;

foreach (var file in files)
{
    if (!File.Exists(file))
    {
        Console.WriteLine($"[MISSING] {file}");
        totalErrors++;
        continue;
    }

    var text = File.ReadAllText(file, Encoding.UTF8);
    var tree = CSharpSyntaxTree.ParseText(
        text,
        new CSharpParseOptions(LanguageVersion.CSharp12),
        path: file,
        encoding: encoding);

    var diags = tree.GetDiagnostics()
        .Where(d => d.Severity == DiagnosticSeverity.Error)
        .OrderBy(d => d.Location.GetLineSpan().StartLinePosition.Line)
        .ToList();

    if (diags.Count == 0)
    {
        Console.WriteLine($"[OK]      {Path.GetFileName(file)}  ({text.Length} chars, {tree.GetRoot().GetText().Lines.Count} lines)");
    }
    else
    {
        Console.WriteLine($"[SYNTAX]  {Path.GetFileName(file)}  -> {diags.Count} error(s)");
        foreach (var d in diags)
        {
            var pos = d.Location.GetLineSpan().StartLinePosition;
            Console.WriteLine($"    line {pos.Line + 1}, col {pos.Character + 1}: {d.Id} {d.GetMessage()}");
        }
        totalErrors += diags.Count;
    }
}

Console.WriteLine();
Console.WriteLine(totalErrors == 0
    ? "==== 语法校验通过：0 error ===="
    : $"==== 语法校验失败：{totalErrors} error(s) ====");

return totalErrors == 0 ? 0 : 1;
