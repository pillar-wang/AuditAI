﻿using System;
using System.Collections.Generic;

namespace Leqisoft.Model;

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
}
