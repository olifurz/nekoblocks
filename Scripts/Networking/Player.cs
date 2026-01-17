using System;
using Godot;
using Nekoblocks.LocalPlayer;

namespace Nekoblocks.Networking;

public partial class Player : Node
{
	public int Id;
	public string Username;
	public Character Character { get; private set; }

	public Control PlayerGui { get; private set; }

	public override void _EnterTree()
	{
		Id = int.Parse(Name); // Assuming Name is set to Peer ID
		PlayerGui = GetNode<Control>("PlayerGui");

		if (!IsMultiplayerAuthority())
		{
			PlayerGui.QueueFree();
		}
		else
		{
			Chat.Instance.RegisterLocalPlayer(this); 
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public async void SetCharacter(NodePath characterPath)
	{
		if (Multiplayer.GetRemoteSenderId() != 1 && Multiplayer.GetRemoteSenderId() != 0) return;
		
		var timeout = 120;
		while (!GetTree().Root.HasNode(characterPath) && timeout > 0)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			timeout--;
		}
		if (timeout <= 0)
		{
			throw new NullReferenceException("$\"Failed to find character at {characterPath} for Player {Id}\"");
		}

		Character = GetNode<Character>(characterPath);
	
		var camera = Character.GetNode<Camera3D>("Camera");
		camera.Current = IsMultiplayerAuthority();
	
		if (!IsMultiplayerAuthority())
		{
			Character.SetPhysicsProcess(false);
			Character.SetProcess(false);
		}
	}
}
