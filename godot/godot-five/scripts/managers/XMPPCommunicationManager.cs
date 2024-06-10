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

	[Signal]
	public delegate void OnMessageReceivedEventHandler(string senderID, string commandType, string[] commandData);

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

		if (TestSendingAndReceiving)
		{
			GD.PushWarning("TestSendingAndReceiving is true. Calling testing function");
			string dummyReceiverJID = "DummyReceiverJID";
			string[] dummyData = new[] { messageEventArgs.Message.Body };
			CallDeferred("PropagateMessage", dummyReceiverJID, "TestSendingAndReceive", dummyData);
			return;
		}

		var parsedCommand = Utilities.Files.ParseJson<CommandInfo>(messageEventArgs.Message.Body);
		if (parsedCommand.commandName == "")
		{
			GD.PushError($"Couldn't parse message {messageEventArgs.ToString()}");
			return;
		}

		//This is a background thread, so a signal emitted here won't arrive to any other node. Instead, we use
		//CallDeferred to send it at EOF
		//TODO: Check if this causes a delay and test option b) Queue message and propagate it during _Process
		CallDeferred(
			"PropagateMessage",
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

	private void PropagateMessage(string senderId, string commandType, string[] data)
	{
		EmitSignal(SignalName.OnMessageReceived, senderId, commandType, data);
	}
}