using Godot;
using System;

public partial class DummyEntity : Node3D
{

    #region Godot overrides

    
    
    public override void _Ready()
    {
        var childrenLabel = GetChild(0) as Label3D;
        if (childrenLabel == null)
        {
            return;
        }

        childrenLabel.Text = Name;
    }

    #endregion
}
