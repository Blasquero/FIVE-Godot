using Artalk.Xmpp;
using Godot;

public partial class NavigationMovement : Node3D
{
    [Export] private float MovementSpeed = 5f;

    private float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private Node3D ParentNode;
    private Vector3 TargetPosition;
    private bool IsNavigationFinished = true;
    private bool SentDestinationArrivalMessage = true;
    private string OwnerJID;
    private string AgentName;
    
    public override void _Ready()
    {
        base._Ready();
        ParentNode = GetParent<Node3D>();
    }

    public void SetTargetPosition(Vector3 newTargetPosition)
    {
        newTargetPosition.Y = ParentNode.GlobalPosition.Y;
        TargetPosition = newTargetPosition;
        GD.Print($"{OwnerJID}: Setting movement target to {TargetPosition}");
        SentDestinationArrivalMessage = false;
        IsNavigationFinished = false;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (IsNavigationFinished && !SentDestinationArrivalMessage)
        {
            Utilities.Messages.SendCommandMessage(new Jid(OwnerJID), GlobalPosition);
            SentDestinationArrivalMessage = true;
            GD.Print($"{OwnerJID}: Arrived to position {GlobalPosition}");
            return;
        }
        if (IsNavigationFinished)
        {
            return;
        }

        Vector3 NextPosition = ParentNode.GlobalPosition.MoveToward(TargetPosition, (float)(delta * MovementSpeed));
        ParentNode.GlobalPosition = NextPosition;
        if (ParentNode.GlobalPosition != TargetPosition)
        {
            ParentNode.LookAt(TargetPosition, Vector3.Up, true);
        }

        if (NextPosition == TargetPosition)
        {
            IsNavigationFinished = true;
        }
    }

   
    public void SetOwnerJIDAndName(string inOwnerJID, string inAgentName)
    {
        OwnerJID = inOwnerJID;
        AgentName = inAgentName;
    }
}
