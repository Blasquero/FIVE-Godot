using Artalk.Xmpp;
using Godot;
using godotfive.scripts.interfaces;

public partial class ControllableAgent : CharacterBody3D, IMessageReceiver
{
	[ExportCategory("Configuration")] [Export]
	private float MovementSpeed = 1f;

	[Export] private MeshController MeshComponent;
	[Export] private NavigationAgent3D NavAgent3D;
	[Export] private CameraManagerComponent CameraManagerComponent;
	[Export] private NavigationMovement NavigationMovementcomponent;
	[Export] private Label3D NameLabel;
	
	private string OwnerJID;
	private bool SentDestinationArrivalMessage = true;

	public void Init(string inOwnerJID, string agentName)
	{
		Name = agentName.ToLower();
		NameLabel.Text = Name;
		OwnerJID = inOwnerJID;
		CameraManagerComponent.SetOwnerJid(inOwnerJID);
		Utilities.Messages.RegisterMessageReceiver(Name, this);
		Utilities.Messages.SendCommandMessage(OwnerJID, GlobalPosition);
	}
	
	public string GetOwnerJID()
	{
		return OwnerJID;
	}

	private float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		base._Ready();

		NavAgent3D.MaxSpeed = MovementSpeed;
		if (NavAgent3D.AvoidanceEnabled)
		{
			NavAgent3D.VelocityComputed += OnVelocityComputed;
		}
	}

	#region Navigation Control

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (NavAgent3D.IsNavigationFinished() && !SentDestinationArrivalMessage)
		{
			Utilities.Messages.SendCommandMessage(new Jid(OwnerJID), GlobalPosition);
			SentDestinationArrivalMessage = true;
			GD.Print($"{Name}: Arrived to position {GlobalPosition}");
			return;
		}

		if (NavAgent3D.IsNavigationFinished())
		{
			return;
		}

		Vector3 nextPathPosition = NavAgent3D.GetNextPathPosition();
		Vector3 newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * MovementSpeed;
		newVelocity.Y = 0;
		Vector3 LookAtTarget = GlobalPosition + newVelocity;
		if (LookAtTarget != GlobalPosition)
		{
			LookAt(LookAtTarget, Vector3.Up, true);
		}
		
		newVelocity.Y = -Gravity;
		if (NavAgent3D.AvoidanceEnabled)
		{
			NavAgent3D.Velocity = newVelocity;
		}
		else
		{
			OnVelocityComputed(newVelocity);
		}
	}

	private void OnVelocityComputed(Vector3 safeVelocity)
	{
		Velocity = safeVelocity;
		MoveAndSlide();
	}

	#endregion

	public void ReceiveMessage(CommandInfo CommandData, string SenderID)
	{
		string commandType = CommandData.commandName;

		switch (commandType)
		{
			case "moveTo":
				MoveToPosition(CommandData);
				break;
			case "color":
				ChangeColor(CommandData);
				break;
			case "cameraFov":
				ChangeCameraFov(CommandData);
				break;
			case "cameraMove":
				MoveCamera(CommandData);
				break;
			case "cameraRotate":
				RotateCamera(CommandData);
				break;
			case "image":
				TakeImage(CommandData);
				break;
			default:
				GD.PushWarning($"[ControllableAgent::ReceiveMessage] Agent {Name} received unrecognize command {commandType}");
				break;
		}

	}

	#region Camera Commmands
	
	private void TakeImage(CommandInfo CommandData)
	{
		int cameraIdx = CommandData.data[0].ToInt();
		float timerSeconds = CommandData.data[1].ToFloat();

		SubViewportComponent subViewport = CameraManagerComponent.GetCamera(cameraIdx);
		subViewport.SetPictureTimer(timerSeconds);
	}

	private void RotateCamera(CommandInfo CommandData)
	{
		int cameraIdx = CommandData.data[0].ToInt();
		int cameraAxis = CommandData.data[1].ToInt();
		float cameraDegrees = CommandData.data[2].ToFloat();
		cameraDegrees = System.Math.Clamp(cameraDegrees, 0, 360);
		
		SubViewportComponent subViewport = CameraManagerComponent.GetCamera(cameraIdx);
		subViewport.Rotatecamera(cameraAxis, cameraDegrees);
	}

	private void MoveCamera(CommandInfo CommandData)
	{
		int cameraIdx = CommandData.data[0].ToInt();
		int cameraAxis = CommandData.data[1].ToInt();
		float cameraMovement = CommandData.data[2].ToFloat();
		
		SubViewportComponent subViewport = CameraManagerComponent.GetCamera(cameraIdx);
		subViewport.MoveCamera(cameraAxis, cameraMovement);
	}

	private void ChangeCameraFov(CommandInfo CommandData)
	{
		int cameraIdx = CommandData.data[0].ToInt();
		float cameraFov = CommandData.data[1].ToFloat();
		

		SubViewportComponent subViewport = CameraManagerComponent.GetCamera(cameraIdx);
		subViewport.SetCameraFov(cameraFov);
	}

	#endregion
	

	private void ChangeColor(CommandInfo CommandData)
	{
		Color parsedColor =
			Utilities.Messages.ParseColorFromMessage(ref CommandData.data[0], out bool succeed);
		if (!succeed)
		{
			return;
		}

		MeshComponent.ChangeMeshColor(parsedColor);
	}

	private void MoveToPosition(CommandInfo CommandData)
	{
		Vector3 parsedVector3 =
			Utilities.Messages.ParseVector3FromMessage(ref CommandData.data[0], out bool succeed);
		if (!succeed)
		{
			return;
		}

		NavAgent3D.TargetPosition = parsedVector3;
		GD.Print($"{Name}: Setting movement target to {NavAgent3D.TargetPosition}");
		SentDestinationArrivalMessage = false;
	}
}
