using System;
using System.Collections.Generic;
using System.Diagnostics;
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

	[ExportGroup("Debug variables")] 
	[Export] private bool ShouldStoreMessages = false;
	[Export] private bool Verbose = false;

	private ArtalkXmppClient XmppClient = null;
	private List<Message> ReceivedMessages = new List<Message>();
	private List<Message> SentMessages = new List<Message>();
	private MessageTestingButton TestingButton;
	public override void _Ready()
	{
		XmppClient = new ArtalkXmppClient(ServerName, UserName, Password);
	
		XmppClient.Message += OnNewMessage;
		XmppClient.Connect();
		if (XmppClient.Connected == false)
		{
			GD.PushError($"CommunicationManager::_Ready: Could not connect to server {ServerName}");
		}
		TestingButton = GetNode<MessageTestingButton>("TestButton");
		TestingButton.OnMessageReceived("Hello");

	}

	private void OnNewMessage(object sender, MessageEventArgs messageEventArgs)
	{
		if (Verbose)
		{
			GD.Print($"Message received from {messageEventArgs.Jid} : {messageEventArgs.Message.Body}");
		}
		
		//This is ran on a background thread, so we need to call OnMessageReceived via CallDeferred
		//Todo: Revisit this and check option b: Store message in a list and send to appropriate node during _Process 
		Variant[] functionParameters = {messageEventArgs.Message.Body};
		TestingButton.CallDeferred("OnMessageReceived", functionParameters);

		if (ShouldStoreMessages)
		{
			ReceivedMessages.Add(messageEventArgs.Message);
		}
		
		SendMessage(messageEventArgs.Jid, "Hello, message received");
		
	}

	//TODO: Expand function as more complex message are required
	private void SendMessage(Jid to, string body)
	{
		Debug.Assert(XmppClient.Connected, "Error: XmppClient is not connected!");
		
		//IMPORTANT: Mark messages as MessageType.Chat or else they won't be received
		var messageToSend = new Message(to, body, type: MessageType.Chat)
		{
			From = XmppClient.Jid
		};
		XmppClient.SendMessage(messageToSend);

		if (ShouldStoreMessages)
		{
			SentMessages.Add(messageToSend);
		}
	}
	
}
