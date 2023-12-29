using Godot;

public partial class MessageTestingButton : Button, IMessageReceiverInterface
{

	#region GodotOverrides

	public override void _Ready()
	{
		Utilities.Avatars.RegisterAvatarAsAvailable(this);
	}

	#endregion
	#region IMessageReceiverInterface Overrides
	public void ProcessReceivedMessage(string messageBody)
	{
		Text = messageBody;
	}
	#endregion
	
}
