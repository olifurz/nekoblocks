using Godot;

namespace Nekoblocks.World;

public partial class SoundManager : Node
{
	public static SoundManager Instance { get; private set; }
	public override void _Ready()
	{
		Instance = this;
	}

	/// <summary>
	/// Play sound on LocalPlayer
	/// </summary>
	/// <param name="fileName">File path</param>
	/// <param name="volume">Volume of sound</param>
	public void PlayLocal(string fileName, float volume = 1)
	{
		var player = new AudioStreamPlayer();
		Workspace.Instance.AddChild(player);
		player.Stream = GD.Load<AudioStream>(fileName);
		player.VolumeLinear = volume;
		player.Finished += () => player.QueueFree();
		player.Play();
	}

	/// <summary>
	/// Play sound in world space
	/// </summary>
	/// <param name="fileName">File path</param>
	/// <param name="position">Position to play the sound at</param>
	/// <param name="volume">Volume of sound</param>
	/// <param name="maxDistance">Maximum distance of sound</param>
	public void PlayAtPosition(string fileName, Vector3 position, float volume = 1, float maxDistance = 0)
	{
		var player = new AudioStreamPlayer3D();
		Workspace.Instance.AddChild(player);
		player.GlobalPosition = position;
		player.MaxDistance = maxDistance;
		player.Stream = GD.Load<AudioStream>(fileName);
		player.VolumeLinear = volume;
		player.Finished += () => player.QueueFree();
		player.Play();
	}
}