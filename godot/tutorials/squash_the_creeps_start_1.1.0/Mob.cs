using Godot;
using System;

public partial class Mob : CharacterBody3D
{
    [Export] public int MinimumSpeed { get; private set; } = 10;
    [Export] public int MaximumSpeed { get; private set; } = 20;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        MoveAndSlide();
    }

    public void Initialize(Vector3 startPosition, Vector3 targetPosition)
    {
        LookAtFromPosition(startPosition, targetPosition, Vector3.Up);
        RotateY((float)GD.RandRange(-Mathf.Pi / 4.0, Mathf.Pi / 4.0));
        int randomSpeed = GD.RandRange(MinimumSpeed, MaximumSpeed);
        Velocity = Vector3.Forward * randomSpeed;
        Velocity = Velocity.Rotated(Vector3.Up, Rotation.Y);
    }

    private void OnVisibilityNotifierScreenExited()
    {
        QueueFree();
    }
}
