using Godot;
using Godot.Collections;

/*
 * Class in charge of sending images to the agents via TCP
 */
public partial class TCPCommunicationComponent : Node
{
	[Export] private string address = "127.0.0.1";
	[Export] private ushort port = 6067;

	private TcpServer imagesServer = new();
	private Array<ImageTCPConnection> unknownClients = new();
	private Dictionary<string, ImageTCPConnection> clients = new();

	private static TCPCommunicationComponent instance = null;

	public static TCPCommunicationComponent GetInstance()
	{
		return instance;
	} 
	
	public void InitServer()
	{
		base._Ready();
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			GD.PushWarning("[TCPCommunicationComponent::InitServer] Found an existing instance of TCPCommunicationComponent");
			QueueFree();
			return;
		}
		Error error = imagesServer.Listen(port, address);
		if (error != Error.Ok)
		{
			GD.PrintErr($"[TCPCommunicationComponent::InitServer]: Error {error.ToString()} when trying to set up TCP server");
			return;
		}
		GD.Print($"[TCPCommunicationManager::Ready] Imange TCP server online in {address}:{port}");
		
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (!imagesServer.IsConnectionAvailable())
		{
			return;
		}

		StreamPeerTcp newConnection = imagesServer.TakeConnection();
		if (newConnection == null)
		{
			return;
		}
		
		var imageConnection = new ImageTCPConnection(newConnection);
		AddChild(imageConnection);
		imageConnection.OnNameOfAgentReceived += OnImageConnectionReceivedName;
	}

	private void OnImageConnectionReceivedName(ImageTCPConnection connection, string agentName)
	{
		if (clients.ContainsKey(agentName))
		{
			return;
		}
		clients.Add(agentName, connection);
		connection.OnNameOfAgentReceived -= OnImageConnectionReceivedName;
	}

	public static bool SendImageToAgent(string agentName, Image imageToSend)
	{
		if (agentName.Length == 0 || imageToSend == null)
		{
			return false;
		}

		if (!instance.clients.TryGetValue(agentName, out ImageTCPConnection agentConnection))
		{
			return false;
		}

		if (agentConnection == null)
		{
			return false;
		}

		string encodedImage = EncodeImageToBase64(imageToSend);
		return agentConnection.SendImage(encodedImage);
	}

	private static string EncodeImageToBase64(Image imageToEncode)
	{
		byte[] imageAsBytes = imageToEncode.SaveJpgToBuffer();
		string base64Image = Marshalls.RawToBase64(imageAsBytes);

		return base64Image;
	}
}
