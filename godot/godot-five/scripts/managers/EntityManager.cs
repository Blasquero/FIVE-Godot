using System;
using System.Data;
using System.Diagnostics;
using Godot;
using Godot.Collections;
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

public partial class EntityManager : Node
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
	
	public void StartMapPopulation()
	{
		MapConfigData = Utilities.ConfigData.GetMapConfigurationData();
		MapLayout = Utilities.ConfigData.GetMapInfo();
		if (MapConfigData == null || MapLayout.GetMapSize() == Vector2I.Zero)
		{
			GD.PushError("Map config is empty or the map size is 0");
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
		XMPPCommunicationManager.GetInstance().OnMessageReceived += OnMessageReceived;
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
			GD.PushError($"Found symbol '{entityChar}' without an associated prefab. Skipping");
			return null;
		}

		Node3D newEntity = Utilities.Entities.SpawnNewEntity(entityPath);
		if (newEntity == null)
		{
			GD.PushError($"Couldn't instantiate scene in path {entityPath} associated to symbol {entityChar}");
		}
		return newEntity;
	}
	
	private Node3D SpawnNewEntity(string entityPath, Vector3 entityLocation)
	{
		Debug.Assert(ResourceLoader.Exists(entityPath));
		var instance = ResourceLoader.Load<PackedScene>(entityPath).Instantiate() as Node3D;
		if (instance == null)
		{
			//TODO: Log error
			return null;
		}
		Ground.AddChild(instance);
		instance.GlobalPosition = entityLocation;
		return instance;
	}

	private void OnMessageReceived(string senderID, string commandType, string[] commandData)
	{
		//TODO: Que responder si hay errores aquí
		if (commandType != "command_create")
		{
			return;
		}

		string agentOwner = commandData[0];
		string agentType = commandData[1];

		// The starter position can be either a spawner or a position
		Vector3 starterPosition = Vector3.Zero;
		//If it starts with {, it's a vector
		if (commandData[2].StartsWith("{"))
		{
			starterPosition = Utilities.Files.ParseJson<Vector3>(commandData[2]);
		}
		else
		{
			var selectedSpawner = Ground.GetNode(commandData[2]) as Node3D;
			if (selectedSpawner == null)
			{
				//TODO: Log error
			}
			else
			{
				starterPosition = selectedSpawner.Position;
			}
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
		controllableAgent.SetOwnerJID(senderID);
		Utilities.Messages.SendMessage(senderID, JsonConvert.SerializeObject(starterPosition));

	}

	private Node3D SpawnNewAgent(string agentType, Vector3 starterPosition)
	{
		if (!SpawnablePrefabs.TryGetValue(agentType, out string pathToEntity))
		{
			//TODO: Log error
			return null;
		}

		Node3D newEntity = SpawnNewEntity(pathToEntity, starterPosition);
		return newEntity;
	}
}
