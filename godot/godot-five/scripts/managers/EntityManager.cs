using System;
using System.Data;
using System.Diagnostics;
using Godot;
using Godot.Collections;
using godotfive.scripts.interfaces;
using Newtonsoft.Json;

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

public partial class EntityManager : Node, IMessageReceiver
{
	[Signal]
	public delegate void OnMapPopulationFinishedEventHandler(int populationResult);

	private MapConfiguration MapConfigData = null;
	private MapInfo MapLayout;

	[Export] private NavigationRegion3D NavMesh;
	[Export] private Node3D Ground;

	//TODO: Check about turning this into a resource
	[Export] private Dictionary<string, string> BasePrefabs;

	[Export] private Dictionary<string, string> SpawnablePrefabs;

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
	}

	public void StartMapPopulation()
	{
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
				Node3D newEntity = SpawnEntityFromChar(entityChar, entityLocation);
				if (newEntity == null)
				{
					continue;
				}

				//We need to name spawners so agents can use them later to spawn entities
				if (entityChar.ToString().Equals("A"))
				{
					//TODO: Check if spawners naming is 0-based
					newEntity.Name = "Spawner " + numberOfSpawners.ToString();
					numberOfSpawners++;
				}

				Ground.AddChild(newEntity);
				newEntity.GlobalPosition = entityLocation;
			}
		}

		if (NavMesh != null)
		{
			NavMesh.BakeNavigationMesh();
		}

		EmitSignal(SignalName.OnMapPopulationFinished, (int)MapPopulationError.OK);

		//Start listening to commands
		XMPPCommunicationComponent.GetInstance().RegisterNewMessageReceiver("EntityManager", this);
	}

	private Vector3 CalculateEntityLocation(int x, int y)
	{
		Vector2 offset = new Vector2(x, y) * MapConfigData.distance;
		//TODO: Ground elevation is hardcoded. Find a way to calculate it before populating
		Vector3 entityLocation = MapConfigData.origin + new Vector3(offset.X, 0.5f, -offset.Y);
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

	private Node3D SpawnEntityFromChar(char entityChar, Vector3 entityLocation)
	{
		string entityPath = GetEntityPath(entityChar, out bool pathFound);
		if (!pathFound || entityPath == null)
		{
			GD.PushWarning(
				$"[EntityManager::SpawnEntityFromChar] Found symbol '{entityChar}' without an associated prefab. Skipping"
			);
			return null;
		}

		Node3D newEntity = Utilities.Entities.SpawnNewEntity(entityPath);
		if (newEntity == null)
		{
			GD.PushError(
				$"[EntityManager::SpawnEntityFromChar] Couldn't instantiate scene in path {entityPath} associated to symbol {entityChar}"
			);
		}

		return newEntity;
	}

	private Node3D SpawnNewEntity(string entityPath, Vector3 entityLocation)
	{
		Debug.Assert(ResourceLoader.Exists(entityPath));
		var instance = ResourceLoader.Load<PackedScene>(entityPath).Instantiate() as Node3D;
		if (instance == null)
		{
			GD.PrintErr($"[EntityManager::SpawnNewEntity] Could not create instance of entity with path {entityPath}");
			return null;
		}

		Ground.AddChild(instance);
		instance.GlobalPosition = entityLocation;
		return instance;
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
		//TODO: Move spawning logic to smaller functions

		Vector3 starterPosition = GetSpawnLocation(starterPositionString);
		if (starterPosition == Vector3.Inf)
		{
			GD.PushWarning(
				$"[EntityManager::ReceiveMessage] Couldn't get a starter position from data{starterPositionString}"
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
		Vector3 starterPosition = Vector3.Inf;
		//If it starts with {, it's a vector
		if (startingPositionInfo.StartsWith("{"))
		{
			float[] parsedArray = Utilities.Messages.ParseArrayFromMessage(
				ref startingPositionInfo,
				out bool succeed,
				3
			);
			if (!succeed)
			{
				return starterPosition;
			}

			starterPosition = new Vector3(parsedArray[0], parsedArray[1], parsedArray[2]);
			starterPosition = Utilities.Math.OrientVector3(starterPosition);
			return starterPosition;
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
				return starterPosition;
			}
			else
			{
				return selectedSpawner.Position;
			}
		}
	}
}