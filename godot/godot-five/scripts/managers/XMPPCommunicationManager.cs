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
public partial class XMPPCommunicationManager : Node
{

    private static XMPPCommunicationManager instance;
    public static XMPPCommunicationManager GetInstance() => instance;

    [ExportGroup("XMPP Configuration")] 
    [Export] private string ServerName = "";
    [Export] private string UserName = "";
    [Export] private string Password = "";

    [ExportGroup("Debug variables")] 
    [Export] private bool ShouldStoreMessages = false;
    [Export] private bool Verbose = true;
    [Export] private bool TestSendingAndReceiving = false;
    [Export] private MessageTestingLabel TestingLabel = null;
    
    private static ArtalkXmppClient XmppClient = null;
    private List<Message> ReceivedMessages = new List<Message>();
    private List<Message> SentMessages = new List<Message>();


    public override void _Ready()
    {
        base._Ready();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            GD.PrintErr("Tried to create a XMPPCommunicationManager, but there's already oen");
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
            TestSendingAndReceivingMessage(messageEventArgs);
            return;
        }
        SendMessageToNode(messageEventArgs);
    }

    private void InternalSendMessage(Message message)
    {
        Debug.Assert(XmppClient.Connected, "Error: XmppClient is not connected!");

        XmppClient.SendMessage(message);
        if (ShouldStoreMessages)
        {
            SentMessages.Add(message);
        }
    }

    private void SendMessageToNode(MessageEventArgs messageArgs)
    {

        //This is ran on a background thread, so we need to call OnMessageReceived via CallDeferred
        // con: CallDeferred is a GodotObject method, so we cannot directly use the interface
        //Todo: Revisit this and check option b: Store message in a list and send to appropriate node during _Process 
        Debug.Assert(TestingLabel is IMessageReceiverInterface,
            "CommunicationManager::OnNewMessage: Tried to send a message to an object that does not implement IMessageReceiverInterface"
        );
        
        Variant[] functionParameters = { messageArgs.Message.Body };
        TestingLabel.CallDeferred("ProcessReceivedMessage", functionParameters);

    }

    private void TestSendingAndReceivingMessage(MessageEventArgs messageArgs)
    {
        Debug.Assert(TestingLabel is IMessageReceiverInterface,
            "CommunicationManager::OnNewMessage: Tried to send a message to an object that does not implement IMessageReceiverInterface"
        );
        
        Variant[] functionParameters = { messageArgs.Message.Body };
        TestingLabel.CallDeferred("ProcessReceivedMessage", functionParameters);
        
        Utilities.Messages.SendMessage(messageArgs.Jid,"Dummy reply. Message received and read");
    }
    
    public static void SendMessage(Message body)
    {
        instance.InternalSendMessage(body);
    }
}