using M00STFD.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace M00STFD.Game;

// Carries a lockpicking noise from the picking client to the authority.
// The client pre-scales the noise for sneaking (it knows crouch and perk
// state); the server sanitises the numbers (radius capped at its own
// config's loudest noise, chance at certainty) and runs the zombie alert
// where the AI actually lives. Discovered by NetPackageManager's normal
// type scan, so it works as long as server and client both run this mod —
// which lockpicking already requires. It lives in the Game layer (not
// Mod) because DoorNoiseService sends it; ProcessPackage is an entry
// point like a Harmony patch and delegates straight back to the service.
//
// NOTE: the class name is the wire identity (package ids are negotiated
// by name at connect). Renaming it is a protocol break between mod
// versions.
[Preserve]
public class NetPackageStfdLockNoise : NetPackage
{
	private Vector3 center;
	private float radius;
	private float alertChance;
	private int instigatorEntityId;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageStfdLockNoise Setup(Vector3 _center, NoiseProfile _noise, int _instigatorEntityId)
	{
		center = _center;
		radius = _noise.Radius;
		alertChance = _noise.AlertChance;
		instigatorEntityId = _instigatorEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		center = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		radius = _reader.ReadSingle();
		alertChance = _reader.ReadSingle();
		instigatorEntityId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(center.x);
		_writer.Write(center.y);
		_writer.Write(center.z);
		_writer.Write(radius);
		_writer.Write(alertChance);
		_writer.Write(instigatorEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || StfdMod.Runtime == null)
		{
			return;
		}
		// A package claiming another player's entity id is dropped.
		if (!ValidEntityIdForSender(instigatorEntityId))
		{
			return;
		}
		DoorNoiseService noise = StfdMod.Runtime.Noise;
		NoiseProfile sanitised = LockNoisePlanner.ClampIncoming(
			new NoiseProfile(radius, alertChance), noise.MaxConfiguredRadius);
		noise.AlertZombies(_world, center, sanitised);
	}

	public override int GetLength()
	{
		return 24;
	}
}
