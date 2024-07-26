using Artalk.Xmpp;
using Godot;
using godotfive.scripts.interfaces;
using Newtonsoft.Json;

public partial class ControllableAgent : CharacterBody3D, IMessageReceiver
{
	[ExportCategory("Configuration")] [Export]
	private float MovementSpeed = 5f;

	[Export] private SubViewportComponent CameraComponent;
	[Export] private MeshController MeshComponent;
	[Export] private NavigationAgent3D NavAgent3D;
	[Export] private Camera3D Camera;

	private string OwnerJID;
	private bool SentDestinationArrivalMessage = false;

	public void Init(string inOwnerJID, string agentName)
	{
		Name = agentName;
		OwnerJID = inOwnerJID;
		Utilities.Messages.SendMessage(OwnerJID, JsonConvert.SerializeObject(GlobalPosition));
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

		Camera.ClearCurrent();

		CameraComponent.SetWorld3d(GetWorld3D());
		CameraComponent.OnPictureReady += OnImageToSend;
	}

	#region Navigation Control

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (NavAgent3D.IsNavigationFinished() && !SentDestinationArrivalMessage)
		{
			Vector3 CorrectedPosition = Utilities.Math.OrientVector3(GlobalPosition);
			string PositionAsString = Utilities.Messages.CreateMessageFromVector3(ref CorrectedPosition);
			Utilities.Messages.SendMessage(new Jid(OwnerJID), PositionAsString);
			SentDestinationArrivalMessage = true;
			return;
		}

		if (NavAgent3D.IsNavigationFinished())
		{
			return;
		}

		Vector3 nextPathPosition = NavAgent3D.GetNextPathPosition();
		Vector3 newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * MovementSpeed;
		newVelocity.Y = 0;
		LookAt(GlobalPosition + newVelocity, Vector3.Up);
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

	private void OnImageToSend(Image imageToSend)
	{
		if (imageToSend == null)
		{
			return;
		}

		Utilities.Messages.SendImage(Name, imageToSend);
	}
	public void ReceiveMessage(CommandInfo CommandData, string SenderID)
	{
		string commandType = CommandData.commandName;


		if (commandType == "moveTo")
		{
			float[] parsedArray =
				Utilities.Messages.ParseArrayFromMessage(ref CommandData.data[1], out bool succeed, 3);
			if (!succeed)
			{
				return;
			}

			var targetPosition = new Vector3(parsedArray[0], parsedArray[1], parsedArray[2]);
			NavAgent3D.TargetPosition = Utilities.Math.OrientVector3(targetPosition);
			SentDestinationArrivalMessage = false;
			return;
		}

		if (commandType == "color")
		{
			float[] parsedArray =
				Utilities.Messages.ParseArrayFromMessage(ref CommandData.data[1], out bool succeed, 4);
			if (!succeed)
			{
				return;
			}

			var newColor = new Color(parsedArray[0], parsedArray[1], parsedArray[2], parsedArray[3]);
			MeshComponent.ChangeMeshColor(newColor);
			return;
		}

		if (commandType == "cameraFov")
		{
			float[] parsedArray =
				Utilities.Messages.ParseArrayFromMessage(ref CommandData.data[1], out bool succeed, 1);
			if (!succeed)
			{
				return;
			}

			Camera.Fov = parsedArray[0];
			return;
		}

		if (commandType == "cameraMove")
		{
			float[] parsedArray =
				Utilities.Messages.ParseArrayFromMessage(ref CommandData.data[1], out bool succeed, 3);
			if (!succeed)
			{
				return;
			}

			var positionChange = new Vector3(parsedArray[0], parsedArray[1], parsedArray[2]);
			positionChange = Utilities.Math.OrientVector3(positionChange);
			//Special Case: Since the mesh of the tractor is rotated 180 degrees and we work with local space,
			//we don't need to rotate the vector 
			CameraComponent.MoveCamera(positionChange);
			return;
		}

		if (commandType == "cameraRotate")
		{
			float[] parsedArray =
				Utilities.Messages.ParseArrayFromMessage(ref CommandData.data[1], out bool succeed, 1);
			if (!succeed)
			{
				return;
			}

			float rotation = System.Math.Clamp(parsedArray[0], 0, 360);
			CameraComponent.Rotatecamera(rotation);
			return;
		}

		if (commandType == "image")
		{
			float[] parsedArray =
				Utilities.Messages.ParseArrayFromMessage(ref CommandData.data[1], out bool succeed, 1);
			if (!succeed)
			{
				return;
			}
			float timeSeconds = parsedArray[0];
			CameraComponent.SetPictureTimer(timeSeconds);
		}
	}
}
