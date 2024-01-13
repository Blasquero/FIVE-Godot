using Godot;

public partial class MessageTestingLabel : Label3D, IMessageReceiverInterface
{
	#region IMessageReceiverInterface Overrides
	public void ProcessReceivedMessage(string messageBody)
	{
		Text = messageBody;
	}
	#endregion
	
}
