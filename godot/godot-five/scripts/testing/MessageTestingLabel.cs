using Godot;

public partial class MessageTestingLabel : Label3D, IMessageReceiverInterface
{

	private static readonly string TestDummyLabel = "DummyReceiverJID";
	
	public override void _Ready()
	{
		base._Ready();
		((IMessageReceiverInterface)this).SubscribeToMessageReceiver();
	}
	#region IMessageReceiverInterface Overrides
	public void ProcessReceivedMessage(string senderID, string commandType, string commandData)
	{
		if (!senderID.Equals(TestDummyLabel))
		{
			return;
		}
		Text = commandData;
	}
	#endregion
}
