using Godot;

internal struct MapInfo
{
    private Vector2 mapSize;
    public Vector2 GetMapSize() => mapSize;

    public MapInfo()
    {
        mapSize = Vector2.Zero;
    }

    public MapInfo(Vector2 mapSize)
    {
        this.mapSize = mapSize;
    }
}

// Class in charge of generating and populating the map from the text file
/*
 * TODO: mirar de hacer esto async?
 */

public partial class MapManager : Node
{
    [ExportGroup("Map Configuration")] 
    [Export] 
    private string PathToTextFile;
    [Export] 
    private float SquareSize = 1f;
    [Export] 
    private StaticBody3D groundBody;
    
    [Signal]
    public delegate void OnMapGeneratedEventHandler(bool succeeded);
    
    private string mapFileContent;
    private FileAccess mapFileAccess;
    private MapInfo mapInfo;
    
    public void StartMapGeneration()
    {
        if (!TryGetMapFileContent())
        {
            EmitSignal(SignalName.OnMapGenerated, false);
            return;
        }

        if (!ParseMapInfo())
        {
            EmitSignal(SignalName.OnMapGenerated, false);
            return;
        }

        if (!GenerateMap())
        {
            EmitSignal(SignalName.OnMapGenerated, false);
            return;
        }

        EmitSignal(SignalName.OnMapGenerated, true);
    } 
    
    private bool TryGetMapFileContent()
    {
        if (!FileAccess.FileExists(PathToTextFile))
        {
            GD.PushError($"Error: File {PathToTextFile} doesn't exist");
            return false;
        }
        
        mapFileAccess = FileAccess.Open(PathToTextFile, FileAccess.ModeFlags.Read);
        Error openingError = mapFileAccess.GetError();
       
        if (openingError == Error.Ok)
        {
            return true;
        }
        GD.PrintErr($"Found error {openingError.ToString()} when trying to open file {PathToTextFile} ");
        return false;
    }

    private bool ParseMapInfo()
    {   
        //Parse the file text until we have it line by line
        
        //Read the full text
        string fileText = mapFileAccess.GetAsText();
        if (fileText.Length == 0)
        {
            GD.PushError("Map file was empty");
            return false;
        }
        //Remove empty spaces 
        string cleanFileText = fileText.Replace(" ", "");
        //Split the map info into lines
        string[] arrayLines = cleanFileText.Split("\n");

        Vector2 mapSize = new Vector2(arrayLines[0].Length, arrayLines.Length - 1);
        mapInfo = new MapInfo(mapSize);
        return true;
    }

    private bool GenerateMap()
    {
        ResizeGround();
        return true;
    }

    private void ResizeGround()
    {
        Vector2 mapSize = mapInfo.GetMapSize();
        Vector3 newGroundScale = new Vector3(mapSize.X * SquareSize, groundBody.Scale.Y, mapSize.Y * SquareSize);
        groundBody.Scale = newGroundScale;
    }
}
