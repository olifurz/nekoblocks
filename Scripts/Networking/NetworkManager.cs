using System;
using System.Linq;
using Godot;
using Nekoblocks.LocalPlayer;

namespace Nekoblocks.Networking;

/// <summary>
/// Manages most low-level networking functionality.
/// Default port 8192
/// </summary>
public partial class NetworkManager : Node
{
	private ENetMultiplayerPeer _peer = new ENetMultiplayerPeer();
	public override void _Ready()
	{
		// --- Server & Client (Common) ---
		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;

		// --- Client Only ---
		Multiplayer.ConnectedToServer += OnConnectedToServer;
		Multiplayer.ConnectionFailed += OnConnectionFailed;
		Multiplayer.ServerDisconnected += OnServerDisconnected;
	}
	
	public override void _Process(double delta)
	{
		
	}

	private void Terminate()
	{
		Multiplayer.MultiplayerPeer = null;
	}
	
	///////////////////////////////////////////////
	////////////// Global Functions ///////////////
	///////////////////////////////////////////////

	private void OnPeerConnected(long id)
	{
		if (id == 1) return;
		AddPlayer((int)id);
	}
	
	private void OnPeerDisconnected(long id)
	{
		if (id == 1) return;
		GetNode("%Players").GetChildren()
			.OfType<Player>()
			.FirstOrDefault(p => p.Id == id)?
			.QueueFree();
	}
	
	///////////////////////////////////////////////
	//////////////// Host Functions ///////////////
	///////////////////////////////////////////////

	public void StartHost()
	{
		StartServer();
		
		AddPlayer(1);
	}
	
	
	///////////////////////////////////////////////
	/////////////// Server Functions //////////////
	///////////////////////////////////////////////
	
	public void StartServer(int port = 8192)
	{
		GD.Print("Starting server at port " + port);
		_peer.CreateServer(port);
		Multiplayer.MultiplayerPeer = _peer;
		
		GD.Print("Server created");
	}

	private void AddPlayer(int id)
	{
		if (!Multiplayer.IsServer()) return;
		if (GetNode("%Players").HasNode(id.ToString())) return;
		
		var playerScene = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/Player.tscn");
		var characterScene = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/Character.tscn");
		
		var player = (Player)playerScene.Instantiate();
		var character = (LocalPlayerCharacter)characterScene.Instantiate();

		player.Id = id;
		player.Name = id.ToString();
		character.Name = "Char_" + id;
		
		player.SetMultiplayerAuthority(id);
		character.SetMultiplayerAuthority(id);

		var workspace = GetNode("%Workspace");
		GetNode("%Players").AddChild(player);
		workspace.AddChild(character);
		var spawnLocation = workspace.GetNodeOrNull<Node3D>("SpawnLocation");
		if (spawnLocation != null) character.Position = spawnLocation.Position;

		player.Rpc(nameof(Player.SetCharacter), character.GetPath());
	}
	
	///////////////////////////////////////////////
	/////////////// Client Functions //////////////
	///////////////////////////////////////////////
	
	public void StartClient(string ip, int port = 8192)
	{
		GD.Print("Starting client at " + ip + ":" + port);
		_peer.CreateClient(ip ?? "127.0.0.1", port);
		Multiplayer.MultiplayerPeer = _peer;
	}

	private void OnConnectedToServer()
	{
		GD.Print("Connected to server");
	}

	private void OnConnectionFailed()
	{
		ThrowError(new Exception("Failed to connect to the server"));
	}

	private void OnServerDisconnected()
	{
		ThrowMessage("Disconnected from the server");
	}

	private void ThrowError(Exception ex)
	{
		var limboScene = ResourceLoader.Load<PackedScene>("res://Scenes/Engine/Limbo.tscn");
		var limbo = (Limbo.Limbo)limboScene.Instantiate();
		GetTree().Root.AddChild(limbo);
		limbo.ThrowError(ex);
		QueueFree();
	}
	
	private void ThrowMessage(string message)
	{
		var limboScene = ResourceLoader.Load<PackedScene>("res://Scenes/Engine/Limbo.tscn");
		var limbo = (Limbo.Limbo)limboScene.Instantiate();
		GetTree().Root.AddChild(limbo);
		limbo.ThrowMessage(message);
		QueueFree();
	}
}