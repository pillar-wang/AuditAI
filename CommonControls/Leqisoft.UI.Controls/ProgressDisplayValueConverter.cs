namespace Leqisoft.UI.Controls;

public interface ProgressDisplayValueConverter
{
	void StartTimer();

	float GetProgressDislayValue(ProgressSnapshotData progressRealValue);
}
