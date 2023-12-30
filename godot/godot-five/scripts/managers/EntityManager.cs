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
    private static readonly EntityManager instance = new EntityManager();

    [Signal]
    public delegate void OnMapPopulationFinishedEventHandler(int populationResult);
    
    public void StartMapPopulation(in MapInfo mapInfo)
    {
        EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.OK);
    }
}
