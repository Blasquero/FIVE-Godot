using Godot;
using System;

public partial class Player : Area2D
{
	
	[Signal] public delegate void HitEventHandler();
	[Export] public int Speed { get; private set; } = 400;
	public Vector2 ScreenSize { get; private set; } = Vector2.Zero;

	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;
		Hide();
	}

	public override void _Process(double delta)
	{
		var velocity = Vector2.Zero;

		if (Input.IsActionPressed("move_right"))
		{
			velocity.X += 1;
		}
		if (Input.IsActionPressed("move_left"))
		{
			velocity.X -= 1;
		}
		if (Input.IsActionPressed("move_down"))
		{
			velocity.Y += 1;
		}
		if (Input.IsActionPressed("move_up"))
		{
			velocity.Y -= 1;
		}

		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (velocity.Length() > 0)
		{
			velocity = velocity.Normalized() * Speed;
			animatedSprite2D.Play();
			if (velocity.X != 0)
			{
				animatedSprite2D.Animation = "walk";
				animatedSprite2D.FlipV = false;
				animatedSprite2D.FlipH = velocity.X < 0;
			}

			if (velocity.Y != 0)
			{
				animatedSprite2D.Animation = "up";
				animatedSprite2D.FlipV = velocity.Y > 0;
			}
		}
		else
		{
			animatedSprite2D.Stop();
		}

		Vector2 newPosition = Position + velocity * (float)delta;
		Position = new Vector2(
			Mathf.Clamp(newPosition.X, 0, ScreenSize.X),
			Mathf.Clamp(newPosition.Y, 0, ScreenSize.Y)
		);
	}

	private void OnBodyEntered(Node2D otherBody)
	{
		Hide();
		EmitSignal(SignalName.Hit);
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(
			CollisionShape2D.PropertyName.Disabled,
			true
			);
	}

	public void Start(Vector2 startPosition)
	{
		Position = startPosition;
		Show();
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}
}
