using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Artalk.Xmpp.Client;
using Artalk.Xmpp.Im;
using MessageEventArgs = Artalk.Xmpp.Im.MessageEventArgs;

/*
 * Class dedicated to manage messages from and to the XMPP server
 */

public class CommandInfo
{
	public string commandName;
	public string[] data;
}

public partial class XMPPCommunicationManager : Node
{
	private static XMPPCommunicationManager instance;
	public static XMPPCommunicationManager GetInstance() => instance;

	[ExportGroup("XMPP Configuration")] [Export]
	private string ServerName = "";

	[Export] private string UserName = "";
	[Export] private string Password = "";

	[ExportGroup("Debug variables")] [Export]
	private bool ShouldStoreMessages = false;

	[Export] private bool Verbose = true;
	[Export] private bool TestSendingAndReceiving = false;
	[Export] private MessageTestingLabel TestingLabel = null;

	[Signal]
	public delegate void OnMessageReceivedEventHandler(string senderID, string commandType, string commandData);

	private static ArtalkXmppClient XmppClient = null;

	public override void _Ready()
	{
		base._Ready();
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			GD.PrintErr("Tried to create a XMPPCommunicationManager, but there's already one");
		}
	}

	public void StartXMPPClient()
	{
		XmppClient = new ArtalkXmppClient(ServerName, UserName, Password);

		XmppClient.Message += OnNewMessage;
		XmppClient.Connect();
		if (XmppClient.Connected == false)
		{
			GD.PushError($"CommunicationManager::_Ready: Could not connect to server {ServerName}");
		}
	}

	private void OnNewMessage(object sender, MessageEventArgs messageEventArgs)
	{
		if (Verbose)
		{
			GD.Print($"Message received from {messageEventArgs.Jid} : {messageEventArgs.Message.Body}");
		}

		var parsedCommand = Utilities.Files.ParseJson<CommandInfo>(messageEventArgs.Message.Body);
		if (parsedCommand.commandName=="")
		{
			GD.PushError($"Couldn't parse message {messageEventArgs.ToString()}");
			return;
		}

		if (TestSendingAndReceiving)
		{
			GD.PushWarning("TestSendingAndReceiving is true. Calling testing function");
			string dummyReceiverJID = "DummyReceiverJID";
			EmitSignal(
				SignalName.OnMessageReceived,
				dummyReceiverJID,
				parsedCommand.commandName,
				parsedCommand.data
			);
			return;
		}

		EmitSignal(
			SignalName.OnMessageReceived,
			messageEventArgs.Jid.ToString(),
			parsedCommand.commandName,
			parsedCommand.data
		);
	}

	private void InternalSendMessage(Message message)
	{
		Debug.Assert(XmppClient.Connected, "Error: XmppClient is not connected!");

		XmppClient.SendMessage(message);
	}
	
	public static void SendMessage(Message body)
	{
		instance.InternalSendMessage(body);
	}
}