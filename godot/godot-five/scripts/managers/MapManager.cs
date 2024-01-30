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
    
  public MapInfo(in char[,] mapSymbolMatrix)
    {
        this.mapSize = new Vector2I(mapSymbolMatrix.GetLength(0), mapSymbolMatrix.GetLength(1));
        this.mapSymbolMatrix = mapSymbolMatrix;
    }
}

// Class in charge of parsing the map information from the map text file and scaling the ground
/*
 * TODO: look into turning this async?
 */

public partial class MapManager : Node
{
    [ExportGroup("Map Configuration")] 
    [Export] private string PathToTextFile = "";
    [Export(PropertyHint.NodeType,"StaticBody3D")] private StaticBody3D GroundBody = null;
    [Export] private bool AdaptGroundSize = false;

    //Signals can only use Variant types, so we need to send it as an int and cast it on the signal handler
    //More info: https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_variant.html#variant-compatible-types
    [Signal]
    public delegate void OnMapGeneratedEventHandler(int mapGenerationError);
    
    private string mapFileContent;
    private FileAccess mapFileAccess;

    private static MapInfo mapInfo;
    public static ref MapInfo GetMapInfo() =>  ref mapInfo;

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
        if (AdaptGroundSize)
        {
            AlignMapToOrigin();
        }
        
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
        SplitMapInfo(fileContents, out List<string> listInfo);
        
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
        
        mapInfo = new MapInfo(symbolMap);
    }
    #endregion

    private void ResizeGround()
    {
        if (!AdaptGroundSize)
        {
            GroundBody.Scale = new Vector3(500, GroundBody.Scale.Y, 500);
            return;
        }
        
        MapConfiguration mapConfigData = Utilities.ConfigData.GetMapConfigurationData();
        Vector2 mapSize = mapInfo.GetMapSize();
        var newGroundScale = new Vector3(mapSize.X * mapConfigData.distance.X, GroundBody.Scale.Y, mapSize.Y * mapConfigData.distance.Y);
        GroundBody.Scale = newGroundScale;
    }

    private void AlignMapToOrigin()
    {
        MapConfiguration mapConfigData = Utilities.ConfigData.GetMapConfigurationData();
        Vector3 newGroundPosition =
            mapConfigData.origin + new Vector3(GroundBody.Scale.X / 2, 0, GroundBody.Scale.Z / 2);
        GroundBody.Position = newGroundPosition;
        //Adding some padding so the map is slightly larger than the field we are representing
        GroundBody.Scale = GroundBody.Scale * new Vector3(1.2f, 1f, 1.2f);
    }
}
