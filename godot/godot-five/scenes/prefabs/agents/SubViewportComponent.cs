using Godot;

public partial class SubViewportComponent : SubViewport
{
	[Export] private Camera3D ChildCamera;

	public override void _Ready()
	{
		if (ChildCamera != null)
		{
			ChildCamera.MakeCurrent();
		}
	}

	public void MoveCamera(Vector3 positionChange)
	{
		ChildCamera.Position += positionChange;
	}
}
