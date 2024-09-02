using Godot;
/*
 * Class in charge of managing the simulation, and calling other managers to populate the world, start communications...
 */

public partial class SimulationManager : Node
{
	[ExportCategory("Managers")] 
	[Export] private XMPPCommunicationComponent XMPPCommunicationComponent;
	[Export] private MapManager MapManager;
	[Export] private EntityManager EntityManager;
	[Export] private TCPCommunicationComponent TCPCommunicationComponent;


	[ExportCategory("Configuration files")] [Export]
	private string JsonMapConfigFilePath;

	[Export] private string GodotUnityFoldersFilePath;

	private static UnityToGodotFolder FoldersConfig;
	public static ref UnityToGodotFolder GetFoldersConfig()
	{
		return ref FoldersConfig;
	}
	private static MapConfiguration MapConfigData;
	public static ref MapConfiguration GetMapConfigurationData()
	{
		return ref MapConfigData;
	}

	private static readonly bool RUNDEBUGCODE = true;
	private static readonly bool GETPERFORMANCEMETRICS = true;
	private float StartSimulationTime = 0;
	private static SimulationManager instance = null;

	public static SimulationManager GetInstance()
	{
		return instance;
	}
	
	#region Godot Overrides

	public override void _Ready()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			GD.PushWarning("[SimulationManager::OnReady] Found an existing instance of SimulationManager");
			QueueFree();
			return;
		}
		
		//TODO: Check instances of managers
		//Cheap way to test snippets of code. Useful to get JSons of objects, test functions with fake results...
		if (RUNDEBUGCODE)
		{
			PreGenerationCode();
		}

		if (GETPERFORMANCEMETRICS)
		{
			StartSimulationTime = Time.GetTicksMsec();
		}
		if (!ParseGodotUnityFolders())
		{
			return;
		}

		ParseJSONMapConfigInfo();

		MapManager.OnMapGenerated += OnMapGenerated;
		GD.Print("[SimulationManager::Ready] Starting map generation");
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
			GD.PushWarning("[SimulationManager::ParseJSONMapConfigInfo]: Symbol to prefab mapping is empty. Check map_config.json");
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
				"[SimulationManager::ParseGodotUnityFolders] Godot data path or unity data path are empty. Please review folders_config.json."
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
			GD.Print($"[SimulationManager::OnMapGenerated] Map Generation returned {((MapGenerationError)generationResult).ToString()}. Starting map population");
			EntityManager.OnMapPopulationFinished += OnMapPopulationFinished;
			EntityManager.StartMapPopulation();
		}
		else
		{
			GD.PushError($"[SimulationManager::OnMapGenerated] Map generation failed with errors {((MapGenerationError)generationResult).ToString()} ");
		}
	}

	private void OnMapPopulationFinished(int populationResult)
	{
		if ((MapPopulationError)populationResult == MapPopulationError.OK)
		{
			GD.Print("[SimulationManager::OnMapPopulationFinished] Map populated without issues. Starting communications");
			//XMPPCommunicationComponent.StartXMPPClient();
		}
		else
		{
			GD.PushWarning($"[SimulationManager::OnMapPopulationFinished] Map population finished with error {((MapPopulationError)populationResult).ToString()}");
		}

		if (RUNDEBUGCODE)
		{
			PostGenerationCode();
		}

		if (GETPERFORMANCEMETRICS)
		{
			PrintPerformanceMetrics();
		}
	}

	#endregion

	private void PreGenerationCode()
	{
	}

	private void PostGenerationCode()
	{
		return;
	}

	private void PrintPerformanceMetrics()
	{
		GD.Print($" Time spent: {Time.GetTicksMsec() - StartSimulationTime} msec");
		GD.Print($" FPS:{Performance.GetMonitor(Performance.Monitor.TimeFps)}");
		GD.Print($" Static Memory:{Performance.GetMonitor(Performance.Monitor.MemoryStatic)}");
		GD.Print($" Object Count :{Performance.GetMonitor(Performance.Monitor.ObjectCount)}");
		GD.Print($" GPU Memory:{Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed)}");
	}
}