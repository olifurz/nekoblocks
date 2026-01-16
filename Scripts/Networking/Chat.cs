using System.Linq;
using Godot;
using RobloxFiles;

namespace Nekoblocks.Networking;

public partial class Chat : Node
{
	public static Chat Instance { get; private set; }
	
	private RichTextLabel _chat;
	private LineEdit _input;
	
	public override void _EnterTree()
	{
		Instance = this;

		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;
	}

	///////////////////////////////////////////////
	////////////// Global Functions ///////////////
	///////////////////////////////////////////////

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("chat_focus"))
		{
			_input.GrabFocus();
		}
	}

	private void OnPeerConnected(long id)
	{
		SendMessage($"[style color=#A6A6A6][i]{NetworkUtils.GetPlayerFromId((int)id)} joined the game[/i][/style]");
	}

	private void OnPeerDisconnected(long id)
	{
		SendMessage($"[style color=#A6A6A6][i]{NetworkUtils.GetPlayerFromId((int)id)} left the game[/i][/style]");
	}

	///////////////////////////////////////////////
	/////////////// Client Functions //////////////
	///////////////////////////////////////////////
	
	public void RegisterLocalPlayer(Player player)
	{
		if (!NetworkUtils.IsPlayer) return;
		
		var playerGui = player.PlayerGui;
		_chat = playerGui.GetNode<RichTextLabel>("%Chatbox");
		_input = playerGui.GetNode<LineEdit>("%Chatbar");
			
		_input.TextSubmitted += SendMessage;
	}
	private void SendMessage(string message)
	{
		RpcId(1, nameof(RequestMessageOnServer), message);
		_input.ReleaseFocus();
		_input.Text = string.Empty;
	}
	
	[Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ReceiveMessage(string message, string user = null)
	{
		if (!NetworkUtils.IsPlayer) return;

		_chat.AppendText(user != null ? $"\n{user}: {message}" : $"{message}");
	}
	
	///////////////////////////////////////////////
	/////////////// Server Functions //////////////
	///////////////////////////////////////////////

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RequestMessageOnServer(string message)
	{
		switch (message.Length)
		{
			case 0:
				return;
			case > 200:
				message = message.Substring(0, 200);
				break;
		}

		string username = null;
		if (!Multiplayer.IsServer()) // If the server is sending it, don't bother with a username
		{
			username = NetworkUtils.GetUsernameFromId(Multiplayer.GetRemoteSenderId());
		}
		Rpc(nameof(ReceiveMessage), message, username);
	}
}