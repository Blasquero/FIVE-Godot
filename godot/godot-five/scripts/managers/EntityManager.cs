using System;
using System.Diagnostics;
using Godot;
using Godot.Collections;

/*
 * Class in charge of managing entities on the map, spawning and assigning them to agents
 * TODO: Look into registering the spawned entities for ease of access/manipulation
 */

public enum MapPopulationError
{
    OK,
    CantAccessConfigData,
    MissingSymbol,
    EntityNotCreated
}

public partial class EntityManager : Node
{
    [Signal]
    public delegate void OnMapPopulationFinishedEventHandler(int populationResult);

    private MapConfiguration MapConfigData = null;
    private MapInfo MapLayout;

    [Export] private Node3D Ground;
    
    //TODO: Check about turning this into a resource
    [Export] private Dictionary<string, string> BasePrefabs;

    public void StartMapPopulation()
    {
        MapConfigData = Utilities.ConfigData.GetMapConfigurationData();
        MapLayout = Utilities.ConfigData.GetMapInfo();
        if (MapConfigData == null || MapLayout.GetMapSize() == Vector2I.Zero)
        {
            GD.PushError("Map config is empty or the map size is 0");
            EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.CantAccessConfigData);
        }

        char[,] mapEntities = MapLayout.GetMapSymbolMatrix();
        for (int x = 0; x < mapEntities.GetLength(0); ++x)
        {
            for (int y = 0; y < mapEntities.GetLength(1); ++y)
            {
                char entityChar = mapEntities[x, y];
                if (entityChar == ' ')
                {
                    //Skipping empty spaces
                    continue;
                }
                Vector3 entityLocation = CalculateEntityLocation(x,y);
                string entityPath = GetEntityPath(entityChar, out bool pathFound);
                if (!pathFound || entityPath == null)
                {
                    GD.PushError($"Found symbol '{entityChar}' without an associated prefab. Skipping");
                    continue;
                }
                Node3D newEntity = SpawnNewEntity(entityPath, entityLocation);
                if (newEntity ==null)
                {
                    GD.PushError($"Couldn't instantiate scene in path {entityPath} associated to symbol {entityChar}");
                    continue;
                }
                Ground.AddChild(newEntity);
                newEntity.GlobalPosition = entityLocation;
            }
        }

        EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.OK);
    }

    private Vector3 CalculateEntityLocation(int x, int y)
    {
        Vector2 offset = new Vector2(x, y) * MapConfigData.distance;
        Vector3 entityLocation = MapConfigData.origin + new Vector3(offset.X, 0, -offset.Y);
        return entityLocation;
    }

    private string GetEntityPath(char symbolChar, out bool pathFound)
    {
        string entityPath;
        if (BasePrefabs.ContainsKey(symbolChar.ToString()))
        {
            entityPath = BasePrefabs[symbolChar.ToString()];
            pathFound = (entityPath != null);
            return entityPath;
        }
        if (!MapConfigData.SymbolToPrefabMapping.ContainsKey(symbolChar.ToString()))
        {
            pathFound = false;
            return string.Empty;
        }
        
        entityPath = MapConfigData.SymbolToPrefabMapping[symbolChar.ToString()].DataFolder;
        pathFound = (entityPath != null);
        return entityPath;
        
    }
    
    private Node3D SpawnNewEntity(string entityPath, Vector3 entityLocation)
    {
        Debug.Assert(ResourceLoader.Exists(entityPath));
        var instance = ResourceLoader.Load<PackedScene>(entityPath).Instantiate() as Node3D;
        return instance;
    }
}