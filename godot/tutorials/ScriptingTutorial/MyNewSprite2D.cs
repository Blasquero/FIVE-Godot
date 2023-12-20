using Godot;

namespace ScriptingTutorial
{
	public partial class MyNewSprite2D : Sprite2D
	{
		private int _speed = 100;
		private float _angularSpeed = Mathf.Pi;

		[Signal]
		public delegate void MySignalEventHandler(bool argument1, bool argument2);
		public override void _Process(double delta)
		{
			int direction = 0;
			if (Input.IsActionPressed("ui_left"))
			{
				direction = -1;
			}
			else if (Input.IsActionPressed("ui_right"))
			{
				direction = 1;
			}
			Rotation += _angularSpeed * direction * (float)delta;
			Vector2 velocity = Vector2.Zero;
			if (Input.IsActionPressed("ui_up"))
			{
				velocity = Vector2.Up.Rotated(Rotation) * _speed;
			}

			if (Input.IsActionPressed("ui_down"))
			{
				velocity = Vector2.Up.Rotated(Rotation) * _speed * -1.0f;
			}
			Position += velocity * (float)delta;
		}
	}
	
}
