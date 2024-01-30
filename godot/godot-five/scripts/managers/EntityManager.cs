using System.Diagnostics;
using Godot;

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
    
    
    public void StartMapPopulation()
    {
        mapConfigData = Utilities.ConfigData.GetMapConfigurationData();
        mapLayout = Utilities.ConfigData.GetMapInfo();
        if (mapConfigData == null || mapLayout.GetMapSize() == Vector2I.Zero)
        {
            // Log error
            EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.CantAccessConfigData);
        }

        //Start parsing the matrix of entities and spawning

        //---------------UNTESTED CODE-----------//
        char[,] mapEntities = mapLayout.GetMapSymbolMatrix();
        
        for (int x = 0; x < mapEntities.GetLength(0); x++)
        {
            for (int y = 0; y < mapEntities.GetLength(1); ++y)
            {
                string entitySymbol = mapEntities[x, y].ToString();
                if (entitySymbol == " ")
                {
                    //Skip empty spaces
                    continue;
                }
                
                if (!mapConfigData.SymbolToPrefabMapping.ContainsKey(entitySymbol))
                {
                    GD.PrintErr($"Found entity symbol {entitySymbol} without an associated prefab");
                    EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.MissingSymbol);
                    continue;
                }

                string prefabFolder = mapConfigData.SymbolToPrefabMapping[entitySymbol].dataFolder;
              
                //TODO: Handle prefabs without data folder (spawner, tractor)
                Vector2 offset = new Vector2(x, y) * mapConfigData.distance;
                Vector3 entityLocation = mapConfigData.origin + new Vector3(offset.X, 0, -offset.Y);
                Node3D newInstance = SpawnNewEntity(prefabFolder, entityLocation);
                if (newInstance == null)
                {
                    GD.PrintErr($"Could not create entity of type {prefabFolder}");
                    EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.EntityNotCreated);
                }
            }
        }
        
        EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.OK);
    }
    
    private Node3D SpawnNewEntity(string entityPath, Vector3 entityLocation)
    {
        Debug.Assert(ResourceLoader.Exists(entityPath));
        var instance = ResourceLoader.Load<PackedScene>(entityPath).Instantiate() as Node3D;
        if (instance == null)
        {
            return null;
        }
        instance.Position = entityLocation;
        //Attachear la instance al suelo?
        return instance;
    }
}
