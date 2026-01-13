using System;
using Godot;
using Nekoblocks.Networking;

namespace Nekoblocks.Limbo;

public partial class Limbo : Node
{
	[Export] public Control Loading;
	private Label _loadingLabel;
	
	[Export] public Control Login;
	private Button _loginButton;
	private Button _serverButton;
	private LineEdit _serverIp;
	
	public override void _Ready()
	{
		_loadingLabel = Loading.GetNode<Label>("Label");
		
		_loginButton = Login.GetNode<Button>("LoginButton");
		_loginButton.Pressed += ConnectToServer;
		_serverButton = Login.GetNode<Button>("ServerButton");
		_serverButton.Pressed += StartServer;
		
		_serverIp = Login.GetNode<LineEdit>("ServerIP");
		
		Loading.Visible = false;
		Login.Visible = true;
		
		Multiplayer.ConnectedToServer += OnConnectedOK;
		Multiplayer.ConnectionFailed += OnConnectionFail;
		Multiplayer.ServerDisconnected += OnServerLost;
	}
	
	private void OnConnectedOK()
	{
		_loadingLabel.Text = "Connected!";
	}

	private void OnConnectionFail()
	{
		ThrowError(new Exception("Could not reach the server. Check the IP or your firewall."));
	}

	private void OnServerLost()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Limbo.tscn"); 
		OS.Alert("Connection to host lost.", "Disconnected");
	}

	private void ConnectToServer()
	{
		try
		{
			Login.Visible = false;
			Loading.Visible = true;
			_loadingLabel.Text = "Preparing world";
		
			var gameScene = ResourceLoader.Load<PackedScene>("res://Scenes/Engine/Game.tscn");
			var game = gameScene.Instantiate();
			
			GetTree().Root.AddChild(game);

			var networkManager = game.GetNode<NetworkManager>("%NetworkManager");
			_loadingLabel.Text = "Starting client";
			networkManager.StartClient(_serverIp.Text);
			
			_loadingLabel.Text = "Loading game...";
			
			QueueFree();
			
		}
		catch (Exception e)
		{
			ThrowError(e);
		}
	}

	private void StartServer()
	{
		try
		{
			Login.Visible = false;
			Loading.Visible = true;
			_loadingLabel.Text = "Preparing world";

			var gameScene = ResourceLoader.Load<PackedScene>("res://Scenes/Engine/Game.tscn");
			var game = gameScene.Instantiate();
			
			GetTree().Root.AddChild(game);
			GetTree().CurrentScene = game;


			var networkManager = game.GetNode<NetworkManager>("%NetworkManager");
			_loadingLabel.Text = "Starting server";
			networkManager.StartHost();
			
			_loadingLabel.Text = "Server started!";
			QueueFree();
		}
		catch (Exception e)
		{
			ThrowError(e);
		}
	}

	public void ThrowMessage(string message)
	{
		Login.Visible = false;
		Loading.Visible = true;
		_loadingLabel.Text = message;
	}
	public void ThrowError(Exception error)
	{
		ThrowMessage(error.Message + "\n\n Please close the client and report the issue if it persists.");
		throw error;
	}
}
