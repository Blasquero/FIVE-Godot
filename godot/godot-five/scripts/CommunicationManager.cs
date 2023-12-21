using System;
using Artalk.Xmpp;
using Godot;
using Artalk.Xmpp.Client;
using Artalk.Xmpp.Im;
using MessageEventArgs = Artalk.Xmpp.Im.MessageEventArgs;


public partial class CommunicationManager : Node
{
	[ExportGroup("XMPP Configuration")] 

	[Export] private String ServerName = "";
	[Export] private String UserName = "";
	[Export] private String Password = "";

	private ArtalkXmppClient XmppClient = null;
	public override void _Ready()
	{
		XmppClient = new ArtalkXmppClient(ServerName, UserName, Password);
	
		XmppClient.Message += OnNewMessage;
		XmppClient.Connect();
	}

	void OnNewMessage(object sender, MessageEventArgs messageEventArgs)
	{
		GD.Print($"Message received from {messageEventArgs.Jid} : {messageEventArgs.Message.Body}");
		
		//IMPORTANT: Mark messages as MessageType.Chat or else it won't be received
		XmppClient.SendMessage("edblase@jabbers.one", "Hello", type:MessageType.Chat);
	}
	
	
}
