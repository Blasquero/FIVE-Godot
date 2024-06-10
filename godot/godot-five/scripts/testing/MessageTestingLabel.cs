using Godot;

public partial class MessageTestingLabel : Label3D
{

	private static readonly string TestDummyLabel = "DummyReceiverJID";
	
	public override void _Ready()
	{
		base._Ready();
		XMPPCommunicationManager.GetInstance().OnMessageReceived += ProcessReceivedMessage;
	}

	private void ProcessReceivedMessage(string senderID, string commandType, string[] commandData)
	{
		if (!senderID.Equals(TestDummyLabel))
		{
			return;
		}
		Text = commandData[0];
	}
}
