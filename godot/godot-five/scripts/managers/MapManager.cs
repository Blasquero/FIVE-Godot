using System.Collections.Generic;
using System.Linq;
using Godot;

public enum MapGenerationError
{
    OK,
    FileError,
    ParsingError,
}

public readonly struct MapInfo
{
    private readonly Vector2I mapSize;
    public Vector2I GetMapSize() => mapSize;

    //Store the map as an array of rows
    /*
     * e.g a field like
     *  AAAA
     *  BBBB
     *  CCCC
     *
     * will be stored as an array [3][4] where array[0] will be the first row (AAAA)
     */
    private readonly char[,] mapSymbolMatrix;
    public char[,] GetMapSymbolMatrix() => mapSymbolMatrix;
    
  public MapInfo(Vector2I mapSize, in char[,] mapSymbolMatrix)
    {
        this.mapSize = mapSize;
        this.mapSymbolMatrix = mapSymbolMatrix;
    }
}

// Class in charge of parsing the map information from the map text file and scaling the ground
/*
 * TODO: look into turning this async?
 * TODO: Move map population to a different class?
 */

public partial class MapManager : Node
{
    private static readonly MapManager instance = new MapManager();
    
    [ExportGroup("Map Configuration")] 
    [Export] private string PathToTextFile;
    [Export] private float SquareSize = 1f;
    [Export] private StaticBody3D groundBody;
    
    
    //Signals can only use Variant types, so we need to send it as an int and cast it on the signal handler
    //More info: https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_variant.html#variant-compatible-types
    [Signal]
    public delegate void OnMapGeneratedEventHandler(int mapGenerationError);
    
    private string mapFileContent;
    private FileAccess mapFileAccess;

    private MapInfo mapInfo;
    public ref readonly MapInfo GetMapInfo() =>  ref mapInfo;

    private Vector3 mapOrigin = Vector3.Zero;
    public Vector3 GetMapOrigin() => mapOrigin;
    public void SetMapOrigin(Vector3 newMapOrigin) => mapOrigin = newMapOrigin;
    
    public void StartMapGeneration()
    {
        Error readingFileError = Utilities.Files.GetFileContent(PathToTextFile, out string fileContent);
        
        if (readingFileError!= Error.Ok)
        {
            EmitSignal(SignalName.OnMapGenerated, (int)MapGenerationError.FileError);
            return;
        }

        if (!ParseMapInfo(fileContent))
        {
            EmitSignal(SignalName.OnMapGenerated, (int)MapGenerationError.ParsingError);
            return;
        }

        ResizeGround();
        EmitSignal(SignalName.OnMapGenerated, (int)MapGenerationError.OK);
    }

    #region File Reading and Parsing
    
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
    
    //Parse the file text until we have it line by line
    private bool ParseMapInfo(in string fileContents)
    {   
        if (fileContents.Length == 0)
        {
            GD.PushError("Map file was empty");
            return false;
        }
        
        //Remove empty spaces 
        string cleanFileText = fileContents.Replace(" ", "");

        SplitMapInfo(cleanFileText, out List<string> listInfo);
        
        StoreMapInfo(listInfo);
       
        return true;
    }

    private void SplitMapInfo(in string cleanFileText,out List<string> listOfLines)
    {
        //Split the map info into lines
        string[] arrayLines = cleanFileText.Split("\n");
        
        //Convert to a list so it's easier to remove empty lines
        listOfLines = arrayLines.ToList();
        listOfLines.RemoveAll(line => line.Length == 0);
    }
    
    private void StoreMapInfo(in List<string> listStrings)
    { 
        var mapSize = new Vector2I(listStrings[0].Length, listStrings.Count);
        char[,] symbolMap = new char[mapSize.Y,mapSize.X];
        for (int i = 0; i < mapSize.Y; i++)
        {
            for (int j = 0; j < mapSize.X; j++)
            {
                symbolMap[i,j] = listStrings[i][j];
            }
        }
        
        mapInfo = new MapInfo(mapSize, symbolMap);
    }
    #endregion

    private void ResizeGround()
    {
        Vector2 mapSize = mapInfo.GetMapSize();
        var newGroundScale = new Vector3(mapSize.X * SquareSize, groundBody.Scale.Y, mapSize.Y * SquareSize);
        groundBody.Scale = newGroundScale;
    }
}
