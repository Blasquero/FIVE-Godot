using Godot;
using Godot.Collections;
/*
 * TODO: See if we can find a non-recursive way to change color of mesh.
 */
public partial class MeshController : Node3D
{

	public void ChangeMeshColor(Color NewColor)
	{
		var newMaterial = new StandardMaterial3D();
		newMaterial.AlbedoColor = NewColor;
		
		ChangeChildrenMeshColor(this, ref newMaterial);
	}

	private void ChangeChildrenMeshColor(Node node, ref StandardMaterial3D newMaterial)
	{
		Array<Node> childrenChildNodes = node.GetChildren(true);
		foreach (Node children in childrenChildNodes)
		{
			var childrenAsMesh = children as MeshInstance3D;
			if(childrenAsMesh !=null)
			{
				childrenAsMesh.SetSurfaceOverrideMaterial(0, newMaterial);
			}
			ChangeChildrenMeshColor(children, ref newMaterial);
		}
	}
}
