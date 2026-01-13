using Godot;
using Nekoblocks.LocalPlayer;

namespace Nekoblocks.Networking;

public partial class Player : Node
{
	public int Id;
	public string Username;
	public LocalPlayerCharacter Character { get; private set; }
	
	public override void _EnterTree()
	{
		// Sync the authority here so inputs only work for the owner
		SetMultiplayerAuthority(Id);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public async void SetCharacter(NodePath characterPath)
	{
		if (Multiplayer.GetRemoteSenderId() != 1 && Multiplayer.GetRemoteSenderId() != 0) return;
		
		while (!HasNode(characterPath))
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		Character = GetNode<LocalPlayerCharacter>(characterPath);
	
		var camera = Character.GetNode<Camera3D>("Camera");
		camera.Current = IsMultiplayerAuthority();
	
		if (!IsMultiplayerAuthority())
		{
			Character.SetPhysicsProcess(false);
			Character.SetProcess(false);
		}
	}
}
