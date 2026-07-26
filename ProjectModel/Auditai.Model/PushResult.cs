namespace Auditai.Model;

public enum PushResult
{
	Success,
	OutOfDate,
	NoContent,
	/// <summary>表格被其他用户锁定（节点级强锁），本次 Push 被服务端拒绝。</summary>
	Locked
}
