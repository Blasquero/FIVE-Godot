using System;
using Godot;
using Artalk.Xmpp.Client;
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

	static void OnNewMessage(object sender, MessageEventArgs messageEventArgs)
	{
		GD.Print($"Message received from {messageEventArgs.Jid} : {messageEventArgs.Message.Body}");
	}
	
}
