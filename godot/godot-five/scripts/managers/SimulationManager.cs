using Godot;

public partial class SimulationManager : Node
{
    [ExportCategory("Managers")] 
    [Export] private XMPPCommunicationManager CommunicationManager;
    [Export] private MapManager MapManager;
    [Export] private EntityManager EntityManager;
    
    [ExportCategory("Configuration files")] [Export]
    private string JsonMapConfigFilePath;
    
    private static MapConfigurationData mapConfigData;
    public static ref MapConfigurationData GetMapConfigurationData() => ref mapConfigData;
    
    #region Godot Overrides
    
    public override void _Ready()
    {
        CommunicationManager.StartXMPPClient();

        if (!ParseJSONMapConfigInfo())
        {
            return;
        }
        
        MapManager.OnMapGenerated += OnMapGenerated;
        GD.Print("Starting map generation");
        MapManager.StartMapGeneration();
    }
    
    #endregion

    //Parse the map info from the JSON file and send the info to the relevant managers
    private bool ParseJSONMapConfigInfo()
    {
        mapConfigData = Utilities.Files.ParseJsonFile<MapConfigurationData>(JsonMapConfigFilePath, out Error outError);
        if (outError != Error.Ok)
        {
            return false;
        }

        Utilities.Math.OrientVector3(ref mapConfigData.origin);
        EntityManager.SetMapConfigurationData(ref mapConfigData);
        return true;
    }
    
    #region Signal Handlers
    
    private void OnMapGenerated(int generationResult)
    {
        if ((MapGenerationError)generationResult == MapGenerationError.OK)
        {
            GD.Print("Map Generated without issues. Starting map population");
            EntityManager.OnMapPopulationFinished += OnMapPopulationFinished;
            MapInfo mapInfo = MapManager.GetMapInfo();
            EntityManager.StartMapPopulation(mapInfo);
        }
        else
        {
            GD.PrintErr($"Map generation failed with errors {((MapGenerationError)generationResult).ToString()} ");
        }
    }

    private void OnMapPopulationFinished(int populationResult)
    {
        if ((MapPopulationError)populationResult == MapPopulationError.OK)
        {
            GD.Print("Map populated without issues. Starting communications");
        }
        else
        {
            GD.PrintErr($"Map population failed with error {((MapPopulationError)populationResult).ToString()}");
        }
    }
    #endregion

}
