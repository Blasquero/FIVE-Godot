using Artalk.Xmpp;
using Godot;

public partial class MessageTestingLabel : Label3D
{

	private static readonly string TestCommandName = "command_test_messages_send_receive";
	
	public override void _Ready()
	{
		base._Ready();
		XMPPCommunicationComponent.GetInstance().OnMessageReceived += ProcessReceivedMessage;
	}

	private void ProcessReceivedMessage(string senderID, string commandType, string[] commandData)
	{
		if (!commandType.Equals(TestCommandName))
		{
			return;
		}

		Text = "";
		foreach (string data in commandData)
		{
			Text += (" " + data);
		}
		
		//Utilities.Messages.SendMessage(new Jid(senderID), "Message Received and read");
	}
}
