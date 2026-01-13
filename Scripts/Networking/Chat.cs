using System.Linq;
using Godot;

namespace Nekoblocks.Networking;

public partial class Chat : Node
{
	public static Chat Instance { get; private set; }
	
	private RichTextLabel _chat;
	private LineEdit _input;
	
	public override void _EnterTree()
	{
		Instance = this;
	}

	///////////////////////////////////////////////
	////////////// Global Functions ///////////////
	///////////////////////////////////////////////

	///////////////////////////////////////////////
	/////////////// Client Functions //////////////
	///////////////////////////////////////////////
	
	public void RegisterLocalPlayer(Player player)
	{
		if (!NetworkUtils.IsPlayer) return;
		
		var playerGui = player.PlayerGui;
		_chat = playerGui.GetNode<RichTextLabel>("Chat");
		_input = playerGui.GetNode<LineEdit>("Chatbox");
			
		_input.TextSubmitted += SendMessage;
	}
	private void SendMessage(string message)
	{
		RpcId(1, nameof(RequestMessageOnServer), message);
	}
	
	[Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ReceiveMessage(string user, string message)
	{
		if (!NetworkUtils.IsPlayer) return;

		_chat.AppendText($"\n{user}: {message}");
		_input.Text = string.Empty;
	}
	
	///////////////////////////////////////////////
	/////////////// Server Functions //////////////
	///////////////////////////////////////////////

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RequestMessageOnServer(string message)
	{
		if (!Multiplayer.IsServer()) return;
		
		switch (message.Length)
		{
			case 0:
				return;
			case > 200:
				message = message.Substring(0, 200);
				break;
		}

		string username = NetworkUtils.GetUsernameFromId(Multiplayer.GetRemoteSenderId());
		Rpc(nameof(ReceiveMessage), username, message);
	}
}