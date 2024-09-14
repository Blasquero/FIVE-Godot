using Godot;
using System;
using Artalk.Xmpp;

public partial class CameraManagerComponent : Node3D
{
	[Export] private SubViewportComponent[] cameras;

	private Jid OwnerJid = null;
	
	public override void _Ready()
	{
		base._Ready();
		foreach (SubViewportComponent camera in cameras)
		{
			camera.OnPictureReady += OnImageToSend;
		}
	}

	public SubViewportComponent GetCamera(int cameraIndex)
	{
		if (cameraIndex > cameras.Length)
		{
			return null;
		}

		return cameras[cameraIndex];
	}
	
	private void OnImageToSend(Image imageToSend, SubViewportComponent cameraSender)
	{
		if (imageToSend == null)
		{
			return;
		}
		
		byte[] imageAsBytes = imageToSend.SaveJpgToBuffer();
		string base64Image = Marshalls.RawToBase64(imageAsBytes);
		var imageData = new ImageData
		{
			cameraIndex = 0,
			imageBase64 = base64Image,
			dateTimeUTC = DateTime.Now
		};
		Utilities.Messages.SendImageMessage(OwnerJid, imageData);
	}

	public void SetOwnerJid(Jid ownerJid)
	{
		OwnerJid = ownerJid;
	}
}
