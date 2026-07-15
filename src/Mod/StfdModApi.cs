using System.Reflection;
using HarmonyLib;
using M00STFD.Game;

namespace M00STFD.Mod;

public class StfdModApi : IModApi
{
	public void InitMod(global::Mod modInstance)
	{
		StfdMod.Runtime = ModRuntime.Create(modInstance.Path);
		var harmony = new Harmony("com.m00.stfd");
		harmony.PatchAll(Assembly.GetExecutingAssembly());
	}
}
