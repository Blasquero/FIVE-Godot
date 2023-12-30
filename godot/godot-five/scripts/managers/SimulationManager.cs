using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;
using Newtonsoft.Json;



public partial class SimulationManager : Node
{
    [ExportCategory("Managers")] 
    [Export] private XMPPCommunicationManager CommunicationManager;
    [Export] private MapManager MapManager;
    [Export] private EntityManager EntityManager;

    [ExportCategory("Configuration files")] [Export]
    private string JsonMapConfigFilePath;
    
    
    #region Godot Overrides
    
    public override void _Ready()
    {

        ParseJSONMapConfigInfo();
        return;
        MapManager.OnMapGenerated += OnMapGenerated;
        GD.Print("Starting map generation");
        MapManager.StartMapGeneration();
    }
    
    #endregion

    //Parse the map info from the JSON file and send it to the relevant managers
    private void ParseJSONMapConfigInfo()
    {
        if (!FileAccess.FileExists(JsonMapConfigFilePath))
        {
            GD.PushError($"Error: File {JsonMapConfigFilePath} doesn't exist");
            return;
        }
        
        FileAccess mapFileAccess = FileAccess.Open(JsonMapConfigFilePath, FileAccess.ModeFlags.Read);
        Error openingError = mapFileAccess.GetError();
       
        if (openingError != Error.Ok)
        {
            return;
        }

        string fileContents = mapFileAccess.GetAsText();
        var mapConfiguration = JsonConvert.DeserializeObject<MapConfiguration>(fileContents);
        Utilities.Math.OrientVector3(ref mapConfiguration.origin);
        MapManager.SetMapOrigin(mapConfiguration.origin);
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
            CommunicationManager.StartXMPPClient();
        }
        else
        {
            GD.PrintErr($"Map population failed with error {((MapPopulationError)populationResult).ToString()}");
        }
    }
    #endregion

}
