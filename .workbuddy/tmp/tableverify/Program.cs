using System;
using System.Collections.Generic;
using System.Reflection;
using Auditai.Model;

using Id64 = Auditai.DTO.Id64;

using Row = Auditai.Model.Row;
using Column = Auditai.Model.Column;
using Cell = Auditai.Model.Cell;

// Table.EnsureAllCellsExist 的"行主序不变式"验证。
//
// 断言的不变式（CellCollection.GetCollectionIndex）：
//   Cells._list[i] 必须对应 (Rows[i / Columns.Count], Columns[i % Columns.Count])
//   Table.this[row,col] => Cells.Get(row,col) => _list[row * Columns.Count + col]
//
// 夹具：3 行 × 2 列，Cells._list 里制造一个"中间空洞"（缺 (1,0) 的格，且它不是列表末尾），
// 这正是 Syncer.Merge 之后 "Rows*Cols != Cells.Count" 触发补全时的形态。
//
// 期望：补全后 table[1,1] 必须仍是绑定到 (row1,col1) 的那个格。
//   旧实现（缺格追加到末尾）→ 列表变成 [r0c0,r0c1,r1c1,r2c0,r2c1,新r1c0]
//                              → table[1,1] 取到 index 3 = r2c0 的内容（静默错位）
//   新实现（按行主序重建）    → 列表变成 [r0c0,r0c1,新r1c0,r1c1,r2c0,r2c1]
//                              → table[1,1] = r1c1（正确）
internal static class Program
{
	private const BindingFlags AnyInst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static int _failed;

	private static int Main()
	{
		Console.OutputEncoding = System.Text.Encoding.UTF8;

		// 走未初始化对象：new Project() 会连带构造 DataReferenceManager / CrossProjectFormulaStore /
		// TableManager 等一堆管理器（含目录创建与 SQLite 初始化），夹具不需要，且会引入无关崩溃面。
		// MakeNewCell() 只用到 Project.GetNextId()（纯计数器）与静态 Project.Current。
		var writer = new System.IO.StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
		Console.SetOut(writer);
		Console.Error.WriteLine("[step] 构造 Project/Table");
		Project project = (Project)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Project));
		Project.Current = project;
		Table table = new Table();
		// MakeNewCell() 会经 Table.Project => TreeNode.Project => Group.Project 取项目，
		// 故把这条链用未初始化对象补齐（TreeNode/Group 的 ctor 同样会连带构造大量管理器）。
		var node = (TreeTableNode)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TreeTableNode));
		var group = (TreeGroup)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TreeGroup));
		SetProp(group, "Project", project);
		SetProp(node, "Group", group);
		table.TreeNode = node;
		Console.Error.WriteLine("[step] Table ok");

		Row[] rows = new Row[3];
		for (int i = 0; i < 3; i++)
		{
			rows[i] = (Row)Activator.CreateInstance(typeof(Row), nonPublic: true);
			SetProp(rows[i], "Id", new Id64(1, i + 1));   // 行 Id 必须唯一，否则位置键会互相覆盖
			SetProp(rows[i], "Table", table);
			ListOf(GetProp(table, "Rows")).Add(rows[i]);
		}
		Column[] cols = new Column[2];
		for (int j = 0; j < 2; j++)
		{
			cols[j] = (Column)Activator.CreateInstance(typeof(Column), nonPublic: true);
			SetProp(cols[j], "Id", new Id64(2, j + 1));   // 列 Id 同理
			SetProp(cols[j], "Table", table);
			ListOf(GetProp(table, "Columns")).Add(cols[j]);
		}
		Console.Error.WriteLine("[step] 行/列就绪");
		Invoke(GetProp(table, "Rows"), "ResetIndex");
		Invoke(GetProp(table, "Columns"), "ResetIndex");
		Console.Error.WriteLine("[step] ResetIndex ok");

		// 建 6 个格，唯独不放入 (1,0) —— 制造"中间空洞"
		System.Collections.IList cells = ListOf(GetProp(table, "Cells"));
		var expected = new Dictionary<string, Cell>();
		int seq = 0;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				if (i == 1 && j == 0) continue;
				Cell cell = (Cell)Activator.CreateInstance(typeof(Cell), nonPublic: true);
				SetProp(cell, "Id", new Id64(1, ++seq));
				SetProp(cell, "Row", rows[i]);
				SetProp(cell, "Column", cols[j]);
				SetProp(cell, "Value", $"r{i}c{j}");
				cells.Add(cell);
				expected[$"r{i}c{j}"] = cell;
			}
		}

		Console.WriteLine($"补全前: Rows={rows.Length} Cols={cols.Length} Cells={cells.Count}（缺 (1,0)）");
		Console.WriteLine();

		Console.Error.WriteLine("[step] 调用 EnsureAllCellsExist");
		Invoke(table, "EnsureAllCellsExist");
		Console.Error.WriteLine("[step] EnsureAllCellsExist 返回");

		Console.WriteLine($"补全后: Cells={cells.Count}（期望 {(rows.Length * cols.Length)}）");

		// 按位置索引读回，核对每一格是否落到自己的位置
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				Cell got = (Cell)Invoke(table, "get_Item", i, j);
				string label = got == null ? "<null>" : Convert.ToString(GetProp(got, "Value"));
				if (i == 1 && j == 0)
				{
					// 补出来的新格：不参与内容比对，但必须"是格且绑在 (1,0) 上"
					bool bound = got != null
						&& ReferenceEquals(GetProp(got, "Row"), rows[1])
						&& ReferenceEquals(GetProp(got, "Column"), cols[0]);
					Console.WriteLine($"[{(bound ? " OK " : "FAIL")}] table[1,0] -> 新补格, 绑定 (1,0)={bound}");
					if (!bound) _failed++;
					continue;
				}
				Cell want = expected[$"r{i}c{j}"];
				bool ok = ReferenceEquals(got, want);
				Console.WriteLine($"[{(ok ? " OK " : "FAIL")}] table[{i},{j}] -> Value={label,-8} 期望=r{i}c{j}{(ok ? "" : "   ← 位置索引错位，读到了别的格")}");
				if (!ok) _failed++;
			}
		}

		Console.WriteLine();
		DedupeScenario();

		Console.WriteLine();
		Console.WriteLine(_failed == 0
			? "==== 全部通过 ===="
			: $"==== {_failed} 项失败 ====");
		return _failed == 0 ? 0 : 1;
	}

	/// <summary>
	/// 场景二：同一 (Row, Column) 位置重复 Cell 时，必须保留"用户编辑过的"那个。
	/// 重复来源见 Table.TryRepairCellCountBeforeSave 注释：自动补全的空白格与 Pull 下发的真实格共存。
	/// 这里放一个"有内容且 Status=New"的格 + 一个"空白且 Status=Synced"的格，期望保留前者。
	/// </summary>
	private static void DedupeScenario()
	{
		Console.WriteLine("--- 场景二：同位置重复格取舍 ---");
		Table table = new Table();
		var row = (Row)Activator.CreateInstance(typeof(Row), nonPublic: true);
		SetProp(row, "Id", new Id64(3, 1));
		SetProp(row, "Table", table);
		ListOf(GetProp(table, "Rows")).Add(row);
		var col = (Column)Activator.CreateInstance(typeof(Column), nonPublic: true);
		SetProp(col, "Id", new Id64(4, 1));
		SetProp(col, "Table", table);
		ListOf(GetProp(table, "Columns")).Add(col);
		Invoke(GetProp(table, "Rows"), "ResetIndex");
		Invoke(GetProp(table, "Columns"), "ResetIndex");

		Cell real = NewCell(row, col, new Id64(5, 1), "用户录入的真实数据", SyncStatus.New);
		Cell blank = NewCell(row, col, new Id64(5, 2), "", SyncStatus.Synced);
		System.Collections.IList cells = ListOf(GetProp(table, "Cells"));
		cells.Add(blank);   // 空白格排在前面，若实现按"先到先得"会错误地保留它
		cells.Add(real);

		Invoke(table, "EnsureAllCellsExist");

		Cell survived = (Cell)Invoke(table, "get_Item", 0, 0);
		bool ok = ReferenceEquals(survived, real);
		Console.WriteLine($"[{(ok ? " OK " : "FAIL")}] 保留的是 Value='{Convert.ToString(GetProp(survived, "Value"))}'（期望保留用户编辑过的 '用户录入的真实数据'）");
		bool countOk = cells.Count == 1;
		Console.WriteLine($"[{(countOk ? " OK " : "FAIL")}] 重复格已去重，Cells={cells.Count}（期望 1）");
		if (!ok) _failed++;
		if (!countOk) _failed++;
	}

	private static Cell NewCell(Row row, Column col, Id64 id, string value, SyncStatus status)
	{
		Cell cell = (Cell)Activator.CreateInstance(typeof(Cell), nonPublic: true);
		SetProp(cell, "Id", id);
		SetProp(cell, "Row", row);
		SetProp(cell, "Column", col);
		SetProp(cell, "Value", value);
		SetProp(cell, "Status", status);
		return cell;
	}

	// ---------- 反射工具 ----------

	private static object GetProp(object target, string name)
	{
		PropertyInfo p = target.GetType().GetProperty(name, AnyInst);
		if (p == null) throw new InvalidOperationException("找不到属性 " + name);
		return p.GetValue(target);
	}

	private static void SetProp(object target, string name, object value)
	{
		PropertyInfo p = target.GetType().GetProperty(name, AnyInst);
		if (p == null) throw new InvalidOperationException("找不到属性 " + name);
		p.SetValue(target, value);
	}

	private static object Invoke(object target, string name, params object[] args)
	{
		Type t = target.GetType();
		if (name == "get_Item")
		{
			PropertyInfo idx = t.GetProperty("Item", AnyInst, null, null, new[] { typeof(int), typeof(int) }, null);
			return idx.GetValue(target, args);
		}
		MethodInfo m = t.GetMethod(name, AnyInst);
		if (m == null) throw new InvalidOperationException("找不到方法 " + name);
		try
		{
			return m.Invoke(target, args);
		}
		catch (TargetInvocationException tie)
		{
			Console.Error.WriteLine($"[被调方法 {name} 抛出] {tie.InnerException?.GetType().Name}: {tie.InnerException?.Message}");
			throw;
		}
	}

	private static System.Collections.IList ListOf(object collection)
	{
		FieldInfo f = collection.GetType().GetField("_list", AnyInst);
		if (f == null) throw new InvalidOperationException("找不到字段 _list on " + collection.GetType().Name);
		return (System.Collections.IList)f.GetValue(collection);
	}
}
