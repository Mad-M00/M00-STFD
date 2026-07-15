using M00STFD.Core;

namespace M00STFD.Game;

internal sealed class UnityDoorLog : IDoorLog
{
	public void Info(string message)
	{
		Log.Out("[STFD] " + message);
	}

	public void Warning(string message)
	{
		Log.Warning("[STFD] " + message);
	}
}
