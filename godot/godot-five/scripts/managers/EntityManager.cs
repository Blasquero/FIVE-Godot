using System;
using System.Diagnostics;
using Godot;
using Godot.Collections;

/*
 * Class in charge of managing entities on the map, spawning and assigning them to agents
 * TODO: Check how to store the base prefabs (spawner, tractor, base tree?)
 */

public enum MapPopulationError
{
    OK,
    CantAccessConfigData,
    MissingSymbol,
    EntityNotCreated
}

public struct BasePrefab
{
    public char symbol;
    public PackedScene PackedScene;
}

public partial class EntityManager : Node
{
    [Signal]
    public delegate void OnMapPopulationFinishedEventHandler(int populationResult);

    private MapConfiguration mapConfigData = null;
    private MapInfo mapLayout;

    [Export] private Node3D Ground;
    
    //TODO: Check about turning this into a resource
    [Export] private Dictionary<string, string> BasePrefabs;

    public void StartMapPopulation()
    {
        mapConfigData = Utilities.ConfigData.GetMapConfigurationData();
        mapLayout = Utilities.ConfigData.GetMapInfo();
        if (mapConfigData == null || mapLayout.GetMapSize() == Vector2I.Zero)
        {
            //TODO: Log error
            EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.CantAccessConfigData);
        }

        char[,] mapEntities = mapLayout.GetMapSymbolMatrix();
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
                    //TODO: Log Error
                    continue;
                }
                SpawnNewEntity(entityPath, entityLocation);
            }
        }

        EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.OK);
    }

    private Vector3 CalculateEntityLocation(int x, int y)
    {
        Vector2 offset = new Vector2(x, y) * mapConfigData.distance;
        Vector3 entityLocation = mapConfigData.origin + new Vector3(offset.X, 0, -offset.Y);
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
        if (!mapConfigData.SymbolToPrefabMapping.ContainsKey(symbolChar.ToString()))
        {
            pathFound = false;
            return string.Empty;
        }
        
        entityPath = mapConfigData.SymbolToPrefabMapping[symbolChar.ToString()].dataFolder;
        pathFound = (entityPath != null);
        return entityPath;
        
    }
    
    private Node3D SpawnNewEntity(string entityPath, Vector3 entityLocation)
    {
        Debug.Assert(ResourceLoader.Exists(entityPath));
        var instance = ResourceLoader.Load<PackedScene>(entityPath).Instantiate() as Node3D;
        if (instance == null)
        {
            return null;
        }

        Ground.AddChild(instance);
        instance.GlobalPosition = entityLocation;
        instance.Scale = Ground.Scale;
        Ground.AddChild(instance);
        
        return instance;
    }
}