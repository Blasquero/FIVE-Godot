using Godot;

public partial class SimulationManager : Node
{
    [ExportCategory("Managers")] 

    [Export] private XMPPCommunicationManager CommunicationManager;
    [Export] private MapManager MapManager;
    [Export] private Node AvatarManager;

    #region Godot Overrides
    public override void _Ready()
    {
        MapManager.OnMapGenerated += OnMapGenerated;
        MapManager.StartMapGeneration();
    }
    #endregion

    #region Signal Handlers
    
    private void OnMapGenerated(bool succeed)
    {
        if (succeed)
        {
            GD.Print("Map Generated  without issues");
            CommunicationManager.StartXMPPClient();
        }
    }
    #endregion

}
