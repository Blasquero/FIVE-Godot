using Godot;
using System;

public partial class NavigationMovement : CharacterBody3D
{
    [Export] private float MovementSpeed = 5f;

    private NavigationAgent3D NavAgent;
    private float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    
    public override void _Ready()
    {
        base._Ready();
        NavAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        if (NavAgent.AvoidanceEnabled)
        {
            NavAgent.VelocityComputed += OnVelocityComputed;
        }
    }

    public void SetTargetPosition(Vector3 newTargetPosition)
    {
        NavAgent.TargetPosition = newTargetPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (NavAgent.IsNavigationFinished())
        {
            return;
        }

        Vector3 nextPathPosition = NavAgent.GetNextPathPosition();
        Vector3 newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * MovementSpeed;
        newVelocity.Y = 0;
        LookAt(GlobalPosition+newVelocity, Vector3.Up);
        newVelocity.Y = -Gravity;
        if (NavAgent.AvoidanceEnabled)
        {
            NavAgent.Velocity = newVelocity;
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
}
