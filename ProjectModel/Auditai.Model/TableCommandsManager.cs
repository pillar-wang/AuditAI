using System;
using System.Collections.Generic;

namespace Auditai.Model;

public class TableCommandsManager
{
	private Table _table;

	private Stack<CommandBase> _undo = new Stack<CommandBase>();

	private Stack<CommandBase> _redo = new Stack<CommandBase>();

	public bool CanUndo => _undo.Count > 0;

	public bool CanRedo => _redo.Count > 0;

	/// <summary>
	/// 命令栈变化时触发（ExecuteCommand/Undo/Redo 后）
	/// </summary>
	public event EventHandler StackChanged;

	internal TableCommandsManager(Table table)
	{
		_table = table;
	}

	public void Undo()
	{
		if (CanUndo)
		{
			CommandBase commandBase = _undo.Pop();
			_redo.Push(commandBase);
			commandBase.Undo();
			StackChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public void Redo()
	{
		if (CanRedo)
		{
			CommandBase commandBase = _redo.Pop();
			_undo.Push(commandBase);
			commandBase.Execute();
			StackChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public void ExecuteCommand(CommandBase command)
	{
		command.Execute();
		_redo.Clear();
		_undo.Push(command);
		StackChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// 清空撤销/重做栈。在表格重新加载（LoadAndReturn）或从云端 Pull/Merge 后调用，
	/// 防止历史命令引用已不存在的 Cell/Row 对象。
	/// </summary>
	public void Clear()
	{
		_undo.Clear();
		_redo.Clear();
		StackChanged?.Invoke(this, EventArgs.Empty);
	}
}
