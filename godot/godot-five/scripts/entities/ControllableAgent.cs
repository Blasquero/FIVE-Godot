using Artalk.Xmpp;
using Godot;
using godotfive.scripts.interfaces;
using Newtonsoft.Json;

public partial class ControllableAgent : CharacterBody3D, IMessageReceiver
{
	[ExportCategory("Configuration")] [Export]
	private float MovementSpeed = 5f;

	private string OwnerJID;

	public void SetOwnerJID(string OwnerJid)
	{
		OwnerJID = OwnerJid;
	}

	
	public string GetOwnerJID() => OwnerJID;

	private NavigationAgent3D NavAgent3D;

	private float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		base._Ready();

		NavAgent3D = GetNode<NavigationAgent3D>("NavigationAgent3D");
		if (NavAgent3D == null)
		{
			//TODO: Log error
			return;
		}
		
		NavAgent3D.MaxSpeed = MovementSpeed;
		if (NavAgent3D.AvoidanceEnabled)
		{
			NavAgent3D.VelocityComputed += OnVelocityComputed;
		}
	}

	public void SetName(string InName)
	{
		Name = InName;
		XMPPCommunicationManager.GetInstance().RegisterNewMessageReceiver(Name, this);
	}

	#region Navigation Control
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (NavAgent3D.IsNavigationFinished())
		{
			//TODO: SEnd message with position to owner
			Utilities.Messages.SendMessage(new Jid(OwnerJID), GlobalPosition.ToString());
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


	public void ReceiveMessage(CommandInfo CommandData, string SenderID)
	{
		string commandType = CommandData.commandName;

		if (commandType == "move_agent")
		{
			float[] parsedArray = Utilities.Messages.ParseArrayFromMessage(ref CommandData.data[1]);
			if (parsedArray.Length != 3)
			{
				return;
			}

			var targetPosition = new Vector3(parsedArray[0], parsedArray[1], parsedArray[2]);
			Utilities.Math.OrientVector3(ref targetPosition);
			NavAgent3D.TargetPosition = targetPosition;
		}
	}
}