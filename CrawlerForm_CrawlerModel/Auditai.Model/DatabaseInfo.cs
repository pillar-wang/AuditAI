using System;

namespace Auditai.Model;

public class DatabaseInfo : ICloneable
{
	public LSDb.DbProvider DatabaseType { get; set; }

	public string DataSource { get; set; }

	public string Name { get; set; }

	public string User { get; set; }

	public string Password { get; set; }

	public bool IntegratedSecurity { get; set; }

	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	public DatabaseInfo Clone()
	{
		return (DatabaseInfo)((ICloneable)this).Clone();
	}
}
