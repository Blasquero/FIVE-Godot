using Godot;

public partial class SimulationManager : Node
{
    [ExportCategory("Managers")]
    [Export] private XMPPCommunicationManager CommunicationManager;
    [Export] private MapManager MapManager;
    [Export] private EntityManager EntityManager;

    [ExportCategory("Configuration files")] 
    [Export] private string JsonMapConfigFilePath;
    [Export] private string GodotUnityFoldersFilePath;

    private static UnityToGodotFolder FoldersConfig;
    public static ref UnityToGodotFolder GetFoldersConfig() => ref FoldersConfig;
    private static MapConfiguration MapConfigData;
    public static ref MapConfiguration GetMapConfigurationData() => ref MapConfigData;

    #region Godot Overrides

    public override void _Ready()
    {
        CommunicationManager.StartXMPPClient();

        if (!ParseGodotUnityFolders())
        {
            return;
        }
        if (!ParseJSONMapConfigInfo())
        {
            return;
        }

        MapManager.OnMapGenerated += OnMapGenerated;
        GD.Print("Starting map generation");
        MapManager.StartMapGeneration();
    }

    #endregion

    private bool ParseJSONMapConfigInfo()
    {
        MapConfigData = Utilities.Files.ParseJsonFile<MapConfiguration>(JsonMapConfigFilePath, out Error outError);
        if (outError != Error.Ok)
        {
            return false;
        }

        MapConfigData.InitLetterToPrefabMapping();
        MapConfigData.ArrayLetterToPrefabMapping();

        Utilities.Math.OrientVector3(ref MapConfigData.origin);
        return true;
    }

    private bool ParseGodotUnityFolders()
    {
        FoldersConfig =
            Utilities.Files.ParseJsonFile<UnityToGodotFolder>(GodotUnityFoldersFilePath, out Error outError);
        if (outError != Error.Ok)
        {
            return false;
        }

        return true;
    }
    
    #region Signal Handlers

    private void OnMapGenerated(int generationResult)
    {
        if ((MapGenerationError)generationResult == MapGenerationError.OK)
        {
            GD.Print("Map Generated without issues. Starting map population");
            EntityManager.OnMapPopulationFinished += OnMapPopulationFinished;
            EntityManager.StartMapPopulation();
        }
        else
        {
            GD.PushError($"Map generation failed with errors {((MapGenerationError)generationResult).ToString()} ");
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
            GD.PushError($"Map population failed with error {((MapPopulationError)populationResult).ToString()}");
        }
    }

    #endregion
}