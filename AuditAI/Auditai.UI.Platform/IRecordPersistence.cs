using System.Collections.Generic;

namespace Auditai.UI.Platform;

public interface IRecordPersistence
{
    void AddRecord(params object[] args);
    void Save(params object[] args);
    void Load(params object[] args);
    IEnumerable<object> GetRecords(params object[] args);
}