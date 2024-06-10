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

	[Export] private string GodotUnityFoldersFilePath;

	private static UnityToGodotFolder FoldersConfig;
	public static ref UnityToGodotFolder GetFoldersConfig() => ref FoldersConfig;
	private static MapConfiguration MapConfigData;
	public static ref MapConfiguration GetMapConfigurationData() => ref MapConfigData;

	private static bool RUNDEBUGCODE = true;
	
	#region Godot Overrides

	public override void _Ready()
	{
		//TODO: Move this to end of map generation
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

		Utilities.Math.OrientVector3(ref MapConfigData.origin);
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
		CommandInfo testCommand = new CommandInfo();
		testCommand.commandName = "command_creante";
		testCommand.data = new string[3];
		testCommand.data[0] = "agent1";
		testCommand.data[1] = "Tractor";
		testCommand.data[2] = "Spawner 1";

		string JSon = JsonConvert.SerializeObject(testCommand);
		return;
	}

	private void PostGenerationCode()
	{
		return;
	}
}