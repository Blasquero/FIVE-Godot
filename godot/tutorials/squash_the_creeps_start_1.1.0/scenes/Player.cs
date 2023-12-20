using Godot;

public partial class Player : CharacterBody3D
{
    [Export] public int Speed { get; protected set; } = 14;
    [Export] public int FallAcceleration { get; protected set; }= 75;

    private Vector3 targetVelocity = Vector3.Zero;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Vector3 newDirection = GetUpdatedDirection();
        if (newDirection != Vector3.Zero)
        {
            GetNode<Node3D>("Pivot").LookAt(Position + newDirection, Vector3.Up);
        }
        UpdateSpeed(newDirection, delta);
    }

    private void UpdateSpeed(Vector3 newDirection, double delta)
    {
        targetVelocity.X = newDirection.X * Speed;
        targetVelocity.Z = newDirection.Z * Speed;
        if (!IsOnFloor())
        {
            targetVelocity.Y = -FallAcceleration * (float)delta;
        }

        Velocity = targetVelocity;
        MoveAndSlide();
    }
    private Vector3 GetUpdatedDirection()
    {
        var direction = Vector3.Zero;
        if (Input.IsActionPressed("move_left"))
        {
            direction.X -= 1.0f;
        }

        if (Input.IsActionPressed("move_right"))
        {
            direction.X += 1.0f;
        }

        if (Input.IsActionPressed("move_up"))
        {
            direction.Z -= 1.0f;
        }

        if (Input.IsActionPressed("move_down"))
        {
            direction.Z += 1.0f;
        }

        return direction.Normalized();
    }
}