using Godot;

public partial class MySprite2D : Sprite2D
{
	private int _speed = 1800;
	private float _angularSpeed = Mathf.Pi;

	public override void _Process(double delta)
	{
		Rotation += _angularSpeed * (float)delta;
		var velocity = Vector2.Up.Rotated(Rotation) * _speed;

		Position += velocity * (float)delta;
	}
}
