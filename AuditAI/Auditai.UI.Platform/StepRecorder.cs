using System;
using System.Collections.Generic;

namespace Auditai.UI.Platform;

public class StepRecorder<Key>
{
	private Stack<Key> _leftStack = new Stack<Key>();

	private Stack<Key> _rightStack = new Stack<Key>();

	private StepContext<Key> context;

	public bool CanBack => _leftStack.Count > 0;

	public bool CanForward => _rightStack.Count > 1;

	public event EventHandler AfterOperation;

	public StepRecorder(StepContext<Key> context)
	{
		this.context = context;
	}

	public void Forward()
	{
		if (CanForward)
		{
			_leftStack.Push(_rightStack.Pop());
			context?.Restore?.Invoke(_rightStack.Peek());
			OnAfterOperation();
		}
	}

	public void Back()
	{
		if (CanBack)
		{
			_rightStack.Push(_leftStack.Pop());
			context?.Restore?.Invoke(_rightStack.Peek());
			OnAfterOperation();
		}
	}

	public void New(Key key)
	{
		if (_rightStack.Count > 0)
		{
			_leftStack.Push(_rightStack.Pop());
		}
		_rightStack.Clear();
		_rightStack.Push(key);
		OnAfterOperation();
	}

	public void Clear()
	{
		_leftStack.Clear();
		_rightStack.Clear();
	}

	protected void OnAfterOperation()
	{
		this.AfterOperation?.Invoke(this, EventArgs.Empty);
	}
}
