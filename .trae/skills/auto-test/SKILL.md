---
name: "auto-test"
description: "Automates building and running unit tests for the LeqiAudit project with detailed pass/fail reporting. Invoke when user asks to run tests, check test status, or verify code changes."
---

# Auto Test

Automates the full test lifecycle for the LeqiAudit project: build, run, and report results.

## Overview

This skill executes the following steps:
1. **Build** the test project (LeqiAudit.Tests)
2. **Run** all unit tests
3. **Analyze** results and report pass/fail/skip counts
4. **Show** detailed failure information if any tests fail
5. **Summarize** the overall test status

## When to Invoke

- User says "run tests", "测试一下", "检查测试", "跑测试"
- User wants to verify code changes don't break existing tests
- After fixing bugs or making code modifications
- Before committing changes

## Execution Steps

### Step 1: Build the test project

```powershell
dotnet build "LeqiAudit.Tests\LeqiAudit.Tests.csproj"
```

Check output for:
- `Build succeeded` - continue
- Any `error CS####` - stop and report build errors
- Warnings are acceptable, note them briefly

### Step 2: Run all tests with verbose output

```powershell
dotnet test "LeqiAudit.Tests\LeqiAudit.Tests.csproj" --no-build --verbosity normal
```

### Step 3: Analyze Results

Parse the summary line at the end of test output:

```
测试摘要: 总计: XXX, 失败: YYY, 成功: ZZZ, 已跳过: WWW
```

### Step 4: Report Results

Format the report as:

```
═══════════════════════════════════════
  Auto Test Report
═══════════════════════════════════════
  Build:       ✅ Passed (or ❌ Failed)
  Total:       {total}
  Passed:      {passed}
  Failed:      {failed}
  Skipped:     {skipped}
  Duration:    {duration}
═══════════════════════════════════════
```

If there are failures, also show the failure details from the xUnit output (test names that failed and their error messages).

### Step 5: Optional - Detailed Failure Analysis

If the user wants to investigate specific test failures:
1. Read the failing test file
2. Check the relevant source code
3. Suggest fixes

## Important Notes

- The test project targets `.NET Framework 4.6.2` (`net462`) with .NET SDK
- Tests use xUnit v2.5.3, Moq, and FluentAssertions
- The test project references FilterModel and LedgerModel as project references
- DTO types are included via source file links (not project references)
- Building from scratch may require `dotnet restore` first if packages are missing
- The solution file is `LeqiAudit.sln`

## Failure Recovery

If build fails:
1. Check if NuGet packages need restoring: `dotnet restore LeqiAudit.sln`
2. Check for duplicate file references in the .csproj
3. Report specific compiler errors to the user