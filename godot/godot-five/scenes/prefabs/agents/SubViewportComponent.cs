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
			GD.PushWarning("[SubViewportComponent::Ready] Missing child Camera3D");
			return;
		}
		ChildCamera.MakeCurrent();
		TimerChild.Timeout += SendImage;

	}

	public void SetCameraFov(float Fov)
	{
		ChildCamera.Fov = Fov;
	}

	public void MoveCamera(int cameraAxis, float cameraMovement)
	{
		if (cameraAxis < 0 || cameraAxis > 2)
		{
			return;
		}

		
		if (cameraAxis == 2)
		{
			cameraMovement *= -1;
		}
		
		Vector3 movementVector = Vector3.Zero;
		movementVector[cameraAxis] = cameraMovement;
		
		ChildCamera.Position += movementVector;
		
	}

	public void Rotatecamera(int cameraAxis, float rotationDegrees)
	{
		if (cameraAxis < 0 || cameraAxis > 2)
		{
			return;
		}
		
		Vector3 degreesChange = Vector3.Zero;
		degreesChange[cameraAxis] = rotationDegrees;
		
		//TODO: Check if this is global or local rotation
		ChildCamera.RotationDegrees += degreesChange;
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
		}
	}
	
}
