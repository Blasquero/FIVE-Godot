using Godot;
using System;

public partial class MessageTestingButton : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var MessageCommunication = GetNode<CommunicationManager>("/root/CommunicationManager");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnMessageReceived(string body)
	{
		Text = body;
	}
}
