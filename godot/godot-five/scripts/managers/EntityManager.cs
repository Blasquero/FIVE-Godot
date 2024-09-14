using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using godotfive.scripts.interfaces;

/*
 * Class in charge of managing entities on the map, spawning and assigning them to agents
 * TODO: Look into registering the spawned entities for ease of access/manipulation
 */

public enum MapPopulationError
{
	OK,
	CantAccessConfigData
}


public class TextureBankComponent
{
	private Dictionary<string, List<ImageTexture>> TextureDictionary = new();

	public List<ImageTexture> GetTexturesOfFolder(string folderPath)
	{
		if (TextureDictionary.ContainsKey(folderPath))
		{
			return TextureDictionary[folderPath];
		}

		List<ImageTexture> createdTextures = CreateTexturesFromFolder(folderPath);
		TextureDictionary.Add(folderPath, createdTextures);
		return createdTextures;
	}

	private List<ImageTexture> CreateTexturesFromFolder(string folderPath)
	{
		var createdTextures = new List<ImageTexture>();
		DirAccess dirAccess = DirAccess.Open(folderPath);
		if (dirAccess == null)
		{
			return createdTextures;
		}

		string[] files = dirAccess.GetFiles();
		foreach (string file in files)
		{
			if (!(file.EndsWith(".jpg") || file.EndsWith(".png")))
			{
				continue;
			}
			Image image = Image.LoadFromFile(folderPath +"/"+ file);
			if (image == null)
			{
				continue;
			}

			ImageTexture newTexture = ImageTexture.CreateFromImage(image);
			if (newTexture == null)
			{
				continue;
			}

			createdTextures.Add(newTexture);
		}

		return createdTextures;
	}
};

public partial class EntityManager : Node, IMessageReceiver
{
	[Signal]
	public delegate void OnMapPopulationFinishedEventHandler(int populationResult);

	private MapConfiguration MapConfigData = null;
	private MapInfo MapLayout;
	private float StartPopulationTime = 0;
	private TextureBankComponent TextureBankComponent;
	[Export] private NavigationRegion3D NavMesh;
	[Export] private Node3D Ground;

	[Export] private Godot.Collections.Dictionary<string, string> BasePrefabs;

	[Export] private Godot.Collections.Dictionary<string, string> SpawnablePrefabs;

	private static EntityManager instance = null;

	public static EntityManager GetInstance()
	{
		return instance;
	}

	public override void _Ready()
	{
		base._Ready();

		if (instance == null)
		{
			instance = this;
		}
		else
		{
			GD.PushWarning("[EntityManager::OnReady] Found an existing instance of EntityManager");
			QueueFree();
		}

		TextureBankComponent = new TextureBankComponent();
	}

	public void StartMapPopulation()
	{
		//StartPopulationTime = Time.GetTicksMsec();
		MapConfigData = Utilities.ConfigData.GetMapConfigurationData();
		MapLayout = Utilities.ConfigData.GetMapInfo();
		if (MapConfigData == null || MapLayout.GetMapSize() == Vector2I.Zero)
		{
			GD.PushError("[EntityManager::StartMapPopulation] Map config is empty or the map size is 0");
			EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.CantAccessConfigData);
			return;
		}

		char[,] mapEntities = MapLayout.GetMapSymbolMatrix();
		int numberOfSpawners = 0;

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

				Vector3 entityLocation = CalculateEntityLocation(x, y);
				Node3D newEntity = SpawnEntityFromChar(entityChar);
				if (newEntity == null)
				{
					continue;
				}

				//We need to name spawners so agents can use them later to spawn entities
				if (entityChar.ToString().Equals("A"))
				{
					numberOfSpawners++;
					newEntity.Name = "Spawner " + numberOfSpawners.ToString();
				}

				Ground.AddChild(newEntity);
				newEntity.GlobalPosition = entityLocation;
			}
		}
		//TODO: Add objects from map_config.json
		if (NavMesh != null)
		{
			NavMesh.BakeNavigationMesh();
		}

		EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.OK);
		//GD.Print($" Population Time : {Time.GetTicksMsec() - StartPopulationTime} msec");
		//Start listening to commands
		XMPPCommunicationComponent.GetInstance().RegisterNewMessageReceiver("EntityManager", this);
	}

	private Vector3 CalculateEntityLocation(int x, int y)
	{
		Vector2 offset = new Vector2(y, x) * MapConfigData.distance;
		//TODO: Ground elevation is hardcoded. Find a way to calculate it before populating
		Vector3 entityLocation = MapConfigData.origin + new Vector3(offset.X, 0.5f, -offset.Y);
		return entityLocation;
	}
	
	private bool GetEntityNameAndPath(char symbolChar, out string prefabName, out string dataFolderPath)
	{
		if (MapConfigData.SymbolToPrefabMapping.TryGetValue(symbolChar.ToString(), out SymbolPrefabPair prefabInfo))
		{
			prefabName = prefabInfo.PrefabName;
			dataFolderPath = prefabInfo.DataFolder;
			return true;
		}

		prefabName = dataFolderPath = "";
		return false;
	}

	private Node3D SpawnEntityFromChar(char entityChar)
	{
		if (GetEntityNameAndPath(entityChar, out string prefabName, out string dataFolderPath))
		{
			if (BasePrefabs.TryGetValue(prefabName, out string prefabPath))
			{
				Node3D newEntity = SpawnNewEntity(prefabPath);
				
				if (newEntity == null)
				{
					GD.PushError(
						$"[EntityManager::SpawnEntityFromChar] Couldn't find prefab {prefabName} associated to symbol {entityChar}"
					);
					return null;
				}

				if (dataFolderPath!=null)
				{
					List<ImageTexture> TexturesToApply = TextureBankComponent.GetTexturesOfFolder(dataFolderPath);
					TextureChangerComponent entityAsTextureChanger = newEntity as TextureChangerComponent;
					if (entityAsTextureChanger != null)
					{
						entityAsTextureChanger.ApplyRandomTextureToMeshes(TexturesToApply);
					}

				}

				return newEntity;
			}
			

			return null;
		}
		
		else 
		{
			GD.PushWarning(
				$"[EntityManager::SpawnEntityFromChar] Found symbol '{entityChar}' without an associated prefab. Skipping"
			);
			return null;
		}
		
	}

	private Node3D SpawnNewEntity(string entityPath, Vector3 entityLocation)
	{
		Debug.Assert(ResourceLoader.Exists(entityPath));
		var newInstance = ResourceLoader.Load<PackedScene>(entityPath).Instantiate() as Node3D;
		if (newInstance == null)
		{
			GD.PrintErr($"[EntityManager::SpawnNewEntity] Could not create instance of entity with path {entityPath}");
			return null;
		}

		Ground.AddChild(newInstance);
		newInstance.GlobalPosition = entityLocation;
		return newInstance;
	}

	private Node3D SpawnNewAgent(string agentType, Vector3 starterPosition)
	{
		if (!SpawnablePrefabs.TryGetValue(agentType, out string pathToEntity))
		{
			GD.PushWarning($"[EntityManager::SpawnNewAgent] Could not get prefab path for agent type {agentType}");
			return null;
		}

		Node3D newEntity = SpawnNewEntity(pathToEntity, starterPosition);
		return newEntity;
	}

	public void ReceiveMessage(CommandInfo CommandData, string SenderID)
	{
		string agentName = CommandData.data[0];
		string agentType = CommandData.data[1];
		string starterPositionString = CommandData.data[2];

		Vector3 starterPosition = GetSpawnLocation(starterPositionString);
		if (starterPosition == Vector3.Inf)
		{
			GD.PushWarning(
				$"[EntityManager::ReceiveMessage] Couldn't get a starter position from data {starterPositionString}"
			);
			return;
		}

		Node3D newEntity = SpawnNewAgent(agentType, starterPosition);
		if (newEntity == null)
		{
			return;
		}

		var controllableAgent = newEntity as ControllableAgent;
		if (controllableAgent == null)
		{
			return;
		}
		controllableAgent.Init(SenderID, agentName);
	}

	private Vector3 GetSpawnLocation(string startingPositionInfo)
	{
		// The starter position can be either a spawner or a position
		//If it starts with {, it's a vector
		if (startingPositionInfo.StartsWith("{"))
		{
			Vector3 parsedStartPosition = Utilities.Messages.ParseVector3FromMessage(
				ref startingPositionInfo,
				out bool succeed
			);
			return parsedStartPosition;
		}
		else
		{
			string spawnerName = startingPositionInfo;
			var selectedSpawner = Ground.GetNode(spawnerName) as Node3D;
			if (selectedSpawner == null)
			{
				GD.PushWarning(
					$"[EntityManager::ReceiveMessage] Could not find spawner with name {spawnerName}. Aborting entity spawn"
				);
				return Vector3.Inf;
			}
			else
			{
				return selectedSpawner.Position;
			}
		}
	}
	
	private static Node3D SpawnNewEntity(string entityPath)
	{
		var newInstance = ResourceLoader.Load<PackedScene>(entityPath).Instantiate() as Node3D;
		return newInstance;
	}
}