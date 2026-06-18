using Leqisoft.DTO;

namespace Leqisoft.UI.Platform;

public interface IProgressDisplayStringFormatter
{
	ProgressInfo OnGetFormProgressInfo(FormProgressFrameUpdater updater);
}
