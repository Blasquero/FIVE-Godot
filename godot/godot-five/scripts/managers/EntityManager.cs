using Godot;

/*
 * Class in charge of managing entities on the map, spawning and assigning them to agents
 */

public enum MapPopulationError
{
    OK,
}

public partial class EntityManager : Node
{
    [ExportGroup("Base Prefabs")] 
    [Export] private Resource spawnerResource;
    [Export] private Resource treeResource;
    
    [Signal]
    public delegate void OnMapPopulationFinishedEventHandler(int populationResult);

    private MapConfiguration mapConfigData = null;
    public void SetMapConfigurationData(ref MapConfiguration newMapConfigData) => mapConfigData = newMapConfigData;
    
    public void StartMapPopulation(in MapInfo mapInfo)
    {
        EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.OK);
    }
}
