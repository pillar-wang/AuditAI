namespace Leqisoft.Model;

public abstract class CommandBase
{
	public abstract void Execute();

	public abstract void Undo();
}
