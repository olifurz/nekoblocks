using System;
using System.Linq;
using Godot;
using Nekoblocks.LocalPlayer;
using Nekoblocks.Scripts;
using RobloxFiles;

namespace Nekoblocks.Networking;

/// <summary>
/// Manages most low-level networking functionality.
/// Default port 8192
/// </summary>
public partial class NetworkManager : Node
{
	public static NetworkManager Instance { get; private set; }
	
	private ENetMultiplayerPeer _peer = new();

	private MultiplayerSpawner _playerSpawner;
	private MultiplayerSpawner _characterSpawner;
	public override void _Ready()
	{
		// --- Server & Client (Common) ---
		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;

		// --- Client Only ---
		Multiplayer.ConnectedToServer += OnConnectedToServer;
		Multiplayer.ConnectionFailed += OnConnectionFailed;
		Multiplayer.ServerDisconnected += OnServerDisconnected;
		
		_playerSpawner = GetNode<MultiplayerSpawner>("PlayerSpawner");
		_playerSpawner.SpawnFunction = Callable.From<Variant, Node>(SpawnPlayer);
		
		_characterSpawner = GetNode<MultiplayerSpawner>("CharacterSpawner");
		_characterSpawner.SpawnFunction = Callable.From<Variant, Node>(SpawnCharacter);
	}
	
	public override void _Process(double delta)
	{
		Instance = this;
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
		InitPlayer((int)id);
	}
	
	private void OnPeerDisconnected(long id)
	{
		if (id == 1) return;
		GetNode("%Players").GetChildren()
			.OfType<Player>()
			.FirstOrDefault(p => p.Id == id)?
			.QueueFree();
	}
	
	private Node SpawnPlayer(Variant data)
	{
		var dict = data.AsGodotDictionary();
		var id = dict["id"].AsInt32();
		
		var scene = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/Player.tscn");
		var player = (Player)scene.Instantiate();
		
		player.Name = id.ToString();
		player.Id = id;
		player.SetMultiplayerAuthority(id);
		
		return player;
	}

	private Node SpawnCharacter(Variant data)
	{
		var dict = data.AsGodotDictionary();
		var id = dict["id"].AsInt32();
		var scene = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/Character.tscn");
		var character = (Character)scene.Instantiate();
		
		character.Name = id.ToString();
		character.Position = dict["position"].AsVector3();
		character.SetMultiplayerAuthority(id);
		
		return character;
	}
	
	///////////////////////////////////////////////
	//////////////// Host Functions ///////////////
	///////////////////////////////////////////////

	public void StartHost()
	{
		StartServer();
		
		InitPlayer(1);
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

	private void InitPlayer(int id, string username = "Neko")
	{
		if (!Multiplayer.IsServer()) return;

		var spawnLocation = GetNode<Node3D>("%SpawnLocation").GlobalPosition;
		var data = new Godot.Collections.Dictionary 
		{
			{ "id", id },
			{ "username", username },
			{ "position", spawnLocation }
		};

		var player = _playerSpawner.Spawn(data);
		var character = _characterSpawner.Spawn(data);
		CallDeferred(MethodName.LinkPlayerAndCharacter, player, character);
	}
	
	private void LinkPlayerAndCharacter(Player player, Character character)
	{
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
