
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artalk.Xmpp;
using Godot;
using Artalk.Xmpp.Client;
using Artalk.Xmpp.Im;
using godotfive.scripts.interfaces;
using MessageEventArgs = Artalk.Xmpp.Im.MessageEventArgs;

/*
 * Class dedicated to manage messages from and to the XMPP server
 *
 *		XMPPComunicationManager -> ComunicationManager
 *			-> ComunicationComponent
 *				->XMPPComunicationComponent (Create base abstract class/ Interface)
 */


public class CommandInfo
{
	public string commandName;
	public string[] data;
}

public partial class XMPPCommunicationComponent : Node
{
	private static XMPPCommunicationComponent instance;
	public static XMPPCommunicationComponent GetInstance()
	{ 
		return instance;
	}

	[ExportGroup("XMPP Configuration")] [Export]
	private string ServerName = "";

	[Export] private string UserName = "";
	[Export] private string Password = "";

	[ExportGroup("Debug variables")] [Export]
	private bool ShouldStoreMessages = false;

	[Export] private bool Verbose = true;

	[Signal]
	public delegate void OnMessageReceivedEventHandler(string senderID, string commandType, string[] commandData);

	private Dictionary<string, IMessageReceiver> MessageReceivers;

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
			GD.PrintErr("[XMPPComunicationComponent::Ready] Tried to create a XMPPCommunicationComponent, but there's already one");
			QueueFree();
			return;
		}

		MessageReceivers = new Dictionary<string, IMessageReceiver>();
		
	}

	public void StartXMPPClient()
	{
		XmppClient = new ArtalkXmppClient(ServerName, UserName, Password);

		XmppClient.Message += OnNewMessage;
		try
		{
			XmppClient.Connect();

		}
		catch (Exception e)
		{
			GD.PrintErr($"[XMPPComunicationComponent::StartXMPPClient] Found exception {e} when trying to connect to the XMPP server");
		}
		if (XmppClient.Connected == false)
		{
			GD.PushError($"[XMPPComunicationComponent::StartXMPPClient] Could not connect to server {ServerName}");
			return;
		}
		GD.Print($"[XMPPComunicationComponent::StartXMPPClient] Started XMPP Server {UserName}@{ServerName}");
	}

	private void OnNewMessage(object sender, MessageEventArgs messageEventArgs)
	{
		if (Verbose)
		{
			GD.Print($"[XMPPComunicationComponent::OnNewMessage] Message received from {messageEventArgs.Jid} : {messageEventArgs.Message.Body}");
		}

		string senderJID = messageEventArgs.Jid.ToString();

		var parsedCommand = Utilities.Files.ParseJson<CommandInfo>(messageEventArgs.Message.Body);
		if (parsedCommand.commandName == "")
		{
			GD.PushWarning($"[XMPPComunicationComponent::OnNewMessage] Couldn't parse message {messageEventArgs}");
			return;
		}

		
		//This is a background thread, so a signal emitted here won't arrive to any other node. Instead, we use
		//CallDeferred to send it at end of frame
		CallDeferred(
			"PropagateMessage",
			senderJID,
			parsedCommand.commandName,
			parsedCommand.data
		);
	}

	private void InternalSendMessage(Message message, bool skipLogmessage)
	{
		Debug.Assert(XmppClient.Connected, "Error: XmppClient is not connected!");

		if (Verbose && !skipLogmessage)
		{
			GD.Print($"[XMPPComunicationComponent::InternalSentMessage] Message sent to {message.To} : {message.Body}");
		}
		XmppClient.SendMessage(message);
	}

	public static void SendMessage(string messageData, Jid to, string metadataInfo)
	{
		var message = new Message(to: to, body: messageData, type: MessageType.Chat);
		AddMessageMetadata(metadataInfo, ref message);
		bool shouldSkipMessage = metadataInfo.Equals("image");
		instance.InternalSendMessage(message, shouldSkipMessage);
	}

	private void PropagateMessage(string senderJID, string commandType, string[] data)
	{
		var parsedCommand = new CommandInfo
		{
			commandName = commandType,
			data = data
		};
		if (parsedCommand.commandName == "create")
		{
			MessageReceivers["EntityManager"].ReceiveMessage(parsedCommand,senderJID);
		}
		else
		{
			string agentName = GetAgentNameFromSenderJID(senderJID);
			MessageReceivers.TryGetValue(agentName, out IMessageReceiver messageTarget);
			if (messageTarget == null)
			{
				return;
			}
			messageTarget.ReceiveMessage(parsedCommand, senderJID);
		}
	}

	public bool RegisterNewMessageReceiver(string InName, IMessageReceiver Receiver)
	{
		if (InName.Length == 0)
		{
			
			return false;
		}

		if (Receiver == null)
		{
			return false;
		}
	
		if (MessageReceivers.ContainsKey(InName))
		{
			return false;
		}
		return InternalRegisterMessageReceiver(InName, Receiver);
	}

	private bool InternalRegisterMessageReceiver(string InName, IMessageReceiver Receiver)
	{
		return MessageReceivers.TryAdd(InName, Receiver);
	}

	private string GetAgentNameFromSenderJID(string senderJid)
	{
		int cuttingPoint = senderJid.IndexOf("@", StringComparison.Ordinal);
		string agentName = senderJid.Substring(0, cuttingPoint);
		return agentName;
	}


	private static void AddMessageMetadata(string metadataInfo, ref Message message)
	{
		var x = message.Data.OwnerDocument.CreateElement("x", "jabber:x:data");
		x.SetAttribute("type", "form");
		var t = x.OwnerDocument.CreateElement("title");
		t.InnerText = "spade:x:metadata";
		var f = x.OwnerDocument.CreateElement("field");
		f.SetAttribute("var", "five");
		f.SetAttribute("type", "text-single");
		var v = f.OwnerDocument.CreateElement("value");
		v.InnerText = metadataInfo;

		f.AppendChild(v);
		x.AppendChild(f);
		x.AppendChild(t);
		message.Data.AppendChild(x);
	}
}
