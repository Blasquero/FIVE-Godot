using Godot;
using ConnectionStatus = Godot.StreamPeerTcp.Status;

public partial class ImageTCPConnection : Node
{
	private StreamPeerTcp connection;
	private bool listenToIncomingMessages = true;

	[Signal]
	public delegate void OnNameOfAgentReceivedEventHandler(ImageTCPConnection TcpConnection, string agentName);

	//Empty constructor required by Godot
	public ImageTCPConnection()
	{
		connection = new StreamPeerTcp();
	}

	public ImageTCPConnection(StreamPeerTcp inConnection)
	{
		connection = inConnection;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		connection.Poll();
		ConnectionStatus status = connection.GetStatus();
		if (status == ConnectionStatus.Error)
		{
			GD.PushWarning(
				$"[ImageTCPConnection::Process] Found error on connection with {connection.GetConnectedHost()}:{connection.GetConnectedPort()}. Closing connection"
			);
			connection.DisconnectFromHost();
			QueueFree();
			return;
		}

		// ReSharper thinks tat this check is always true, so disabling code revision for this instance
		// ReSharper disable once ConditionIsAlwaysTrueOrFalse
		if (status == ConnectionStatus.None)
		{
			GD.Print(
				$"[ImageTCPConnection::Process] Client {connection.GetConnectedHost()}:{connection.GetConnectedPort()} is disconnected"
			);
			QueueFree();
			return;
		}

		if (!listenToIncomingMessages)
		{
			return;
		}

		int availableData = connection.GetAvailableBytes();
		if (availableData == 0)
		{
			return;
		}

		string dataString = connection.GetString(availableData);
		EmitSignal(SignalName.OnNameOfAgentReceived, this, dataString);
		listenToIncomingMessages = false;
		SendImage("Name received: " + dataString);
	}

	public bool SendImage(string base64Image)
	{
		Error error = connection.PutData(base64Image.ToAsciiBuffer());
		if (error != Error.Ok)
		{
			GD.PushWarning(
				$"[ImageTCPConnection::SendImage] Error {error} while sending image to client {connection.GetConnectedHost()}:{connection.GetConnectedPort()}"
			);
		}

		return error == Error.Ok;
	}
}