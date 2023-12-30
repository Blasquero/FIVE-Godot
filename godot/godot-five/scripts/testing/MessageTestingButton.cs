using Godot;

public partial class MessageTestingButton : Button, IMessageReceiverInterface
{
	#region IMessageReceiverInterface Overrides
	public void ProcessReceivedMessage(string messageBody)
	{
		Text = messageBody;
	}
	#endregion
	
}
