using System.Collections.Generic;
using Godot;
using Utilities;
using Math = System.Math;

public partial class TextureChangerComponent : Node3D
{
	[Export] private MeshInstance3D[] TextureChangeableMeshes;


	public void ApplyRandomTextureToMeshes(List<ImageTexture> texturesToApply)
	{
		if (texturesToApply.Count == 0)
		{
			return;
		}
		foreach (MeshInstance3D mesh in TextureChangeableMeshes)
		{
			int selectedTexture =(int) (GD.Randi() % texturesToApply.Count);
			ApplyTexture(texturesToApply[selectedTexture], mesh);
		}
	}

	public void ApplyTexture(ImageTexture imageTexture, MeshInstance3D meshToApplyTo)
	{
		var newMaterial = new StandardMaterial3D();
		newMaterial.AlbedoTexture = imageTexture;
		newMaterial.Uv1Scale= new Vector3(3, 2, 1);

		meshToApplyTo.SetSurfaceOverrideMaterial(0, newMaterial);
	}
}
