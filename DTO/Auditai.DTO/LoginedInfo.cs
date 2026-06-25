namespace Auditai.DTO;

public class LoginedInfo
{
	public UserToken UserToken { get; set; }

	public string MachineCode { get; set; }

	public string ProcessId { get; set; }

	public long userId => UserToken.UserId;
}
