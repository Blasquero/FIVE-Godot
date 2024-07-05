#nullable enable
using System;
using Godot;
using Artalk.Xmpp;
using Artalk.Xmpp.Im;
using Newtonsoft.Json;

/*
 * Utility class with static functions regarding communication and messages
 */
namespace Utilities
{
	public static class Messages
	{
		public static void SendMessage(Jid to, string messageBody)
		{
			var message = new Message(to: to, body: messageBody, type: MessageType.Chat);
			XMPPCommunicationManager.SendMessage(message);
		}

		public static string CreateMessageFromVector3(ref Vector3 vectorToParse, out bool messageCreated)
		{
			messageCreated = false;
			return string.Empty;
		}
		
		public static float[] ParseArrayFromMessage(ref string stringToParse)
		{
			float[] resultArray = Array.Empty<float>();
			
			string trimmedString = stringToParse.Trim();

			if (!trimmedString.StartsWith("{"))
			{
				return resultArray ;
			}

			trimmedString = trimmedString.Replace("{", "");
			trimmedString = trimmedString.Replace("}", "");
			trimmedString = trimmedString.Replace(" ", "");
			string[] splittedFloats = trimmedString.Split(",");

			resultArray = new float[splittedFloats.Length];
			for (int i = 0; i < splittedFloats.Length; i++)
			{
				resultArray[i] = splittedFloats[i].ToFloat();
			}

			return resultArray;
		}
	}

	public static class Entities
	{
		public static Node3D? SpawnNewEntity(string entityPath)
		{
			var instance = ResourceLoader.Load<PackedScene>(entityPath).Instantiate() as Node3D;
			return instance;
		}
	}
	public static class Files
	{
		public static Error GetFileContent(string pathToFile, out string fileContent)
		{
			fileContent = "";
			if (!FileAccess.FileExists(pathToFile))
			{
				GD.PushError($"Error: File {pathToFile} doesn't exist");
				return Error.FileBadPath;
			}

			using FileAccess mapFileAccess = FileAccess.Open(pathToFile, FileAccess.ModeFlags.Read);
			Error openingError = mapFileAccess.GetError();

			if (openingError != Error.Ok)
			{
				return openingError;
			}

			fileContent = mapFileAccess.GetAsText();
			return Error.Ok;
		}

		public static T? ParseJson<T>(string jsonString)
		{
			return JsonConvert.DeserializeObject<T>(jsonString);
		}

		public static T? ParseJsonFile<T>(string filePath, out Error outError)
		{
			outError = GetFileContent(filePath, out string fileContent);
			return outError != Error.Ok ? default : ParseJson<T>(fileContent);
		}
	}

	public static class ConfigData
	{
		public static ref MapConfiguration GetMapConfigurationData()
		{
			return ref SimulationManager.GetMapConfigurationData();
		}

		public static ref MapInfo GetMapInfo()
		{
			return ref MapManager.GetMapInfo();
		}

		public static ref InfoCollection GetMapConfigInfo()
		{
			return ref MapManager.GetMapConfigInfo();
		}

		public static ref UnityToGodotFolder GetFoldersConfig()
		{
			return ref SimulationManager.GetFoldersConfig();
		}
	}

	public static class Math
	{
		//Godot is right-handed. Assume that Unity is king and all the vectors we receive are left-handed
		public static void OrientVector3(ref Vector3 vectorToOrient)
		{
			vectorToOrient.Z *= -1;
		}
	}
}