using System;
using Godot;
using Nekoblocks.Networking;
using Nekoblocks.Scripts;

namespace Nekoblocks.Limbo;

public partial class Limbo : Node
{
	[Export] public Control Loading;
	private Label _loadingLabel;
	
	[Export] public Control Login;
	private LineEdit _serverIp;
	
	private readonly RobloxMapLoader _robloxMapLoader = new();
	
	public override void _Ready()
	{
		_loadingLabel = Loading.GetNode<Label>("Label");
		
		var loginButton = Login.GetNode<Button>("LoginButton");
		loginButton.Pressed += ConnectToServer;
		var serverButton = Login.GetNode<Button>("ServerButton");
		serverButton.Pressed += StartServer;
		var editorButton = Login.GetNode<Button>("EditorButton");
		editorButton.Pressed += () => GetTree().ChangeSceneToFile("res://Scenes/Engine/Editor.tscn");
		
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
			var fileDialog = new FileDialog();
			AddChild(fileDialog);
			fileDialog.UseNativeDialog = true;
			fileDialog.Filters = ["*.rbxl ; Roblox Place File"];
			fileDialog.FileFilterToggleEnabled = false;
			fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
			fileDialog.FileSelected += (path) =>
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
				
				_robloxMapLoader.Load(path);
			
				_loadingLabel.Text = "Server started!";
				QueueFree();
			};
			fileDialog.Canceled += () => fileDialog.QueueFree();
			fileDialog.Show();
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
