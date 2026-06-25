using Auditai.DTO;

namespace Auditai.UI.Platform;

public interface IProgressDisplayStringFormatter
{
	ProgressInfo OnGetFormProgressInfo(FormProgressFrameUpdater updater);
}
