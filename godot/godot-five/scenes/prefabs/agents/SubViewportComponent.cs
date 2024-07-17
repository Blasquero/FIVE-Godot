using Godot;

public partial class SubViewportComponent : SubViewport
{
	[Export] private Camera3D ChildCamera;
	[Export] private Timer TimerChild;
	[Signal]
	public delegate void OnPictureReadyEventHandler(Image picture);

	private float TimeBetweenPictures = 0;
	
	public override void _Ready()
	{
		if (ChildCamera == null)
		{
			GD.PrintErr("[SubViewportComponent] ERROR: Missing child Camera3D");
			return;
		}
		ChildCamera.MakeCurrent();
		TimerChild.Timeout += SendImage;

	}

	public void MoveCamera(Vector3 positionChange)
	{
		ChildCamera.Position += positionChange;
	}

	public void Rotatecamera(float rotationDegrees)
	{
		ChildCamera.GlobalRotation=new Vector3(0, rotationDegrees, 0);
	}

	public void SetWorld3d(World3D newWorld)
	{
		World3D = newWorld;
	}

	public void SendImage()
	{
		Image image = GetTexture().GetImage();
		EmitSignal(SignalName.OnPictureReady, image);
	}

	public void SetPictureTimer(float seconds)
	{
		if (seconds > 0)
		{
			TimerChild.Stop();
			TimerChild.Start(seconds);
			return;
		}

		if (seconds == 0)
		{
			SendImage();
			return;
		}

		if (seconds < 0)
		{
			TimerChild.Stop();
			return;
		}
	}
	
}
