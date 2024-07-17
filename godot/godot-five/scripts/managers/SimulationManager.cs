using Godot;
/*
 * Class in charge of managing the simulation, and calling other managers to populate the world, start communications...
 *  TODO: Unhardcode command names, move them to a table enum CommandType -> string CommandName
 * 
 */
public partial class SimulationManager : Node
{
	[ExportCategory("Managers")] 
	[Export] private XMPPCommunicationManager CommunicationManager;
	[Export] private MapManager MapManager;
	[Export] private EntityManager EntityManager;

	[ExportCategory("Configuration files")] [Export]
	private string JsonMapConfigFilePath;

	[Export] private string GodotUnityFoldersFilePath;

	private static UnityToGodotFolder FoldersConfig;
	public static ref UnityToGodotFolder GetFoldersConfig() => ref FoldersConfig;
	private static MapConfiguration MapConfigData;
	public static ref MapConfiguration GetMapConfigurationData() => ref MapConfigData;

	private static readonly bool RUNDEBUGCODE = true;
	
	#region Godot Overrides

	public override void _Ready()
	{

		CommunicationManager.StartXMPPClient();
		//Cheap way to test snippets of code. Useful to get JSons of objects, test functions with fake results...
		if (RUNDEBUGCODE)
		{
			PreGenerationCode();
		}
		
		if (!ParseGodotUnityFolders())
		{
			return;
		}

		ParseJSONMapConfigInfo();

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

		if (MapConfigData.symbolToPrefabMap == null)
		{
			GD.PushWarning("WARNING: Symbol to prefab mapping is empty. Check map_config.json");
			GD.PushWarning("Continuing simulation");
			return false;
		}

		MapConfigData.InitLetterToPrefabMapping();
		MapConfigData.ArrayLetterToPrefabMapping();

		MapConfigData.origin = Utilities.Math.OrientVector3(MapConfigData.origin);
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

		if (string.IsNullOrEmpty(FoldersConfig.GodotDataFolder) || string.IsNullOrEmpty(FoldersConfig.UnityDataFolder))
		{
			GD.PushError(
				"ERROR: Godot data path or unity data path are empty. Please review folders_config.json. Halting simulation"
			);
			return false;
		}

		return true;
	}

	#region Signal Handlers

	private void OnMapGenerated(int generationResult)
	{
		if ((MapGenerationError)generationResult <= MapGenerationError.CompletedWithLightWarnings)
		{
			GD.Print($"Map Generation returned {((MapGenerationError)generationResult).ToString()}. Starting map population");
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

		if (RUNDEBUGCODE)
		{
			PostGenerationCode();
		}
	}

	#endregion

	private void PreGenerationCode()
	{
		return;
	}

	private void PostGenerationCode()
	{
		return;
	}
}