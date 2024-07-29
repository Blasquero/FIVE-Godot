#nullable enable

using System;
using Godot;
using Artalk.Xmpp;
using Artalk.Xmpp.Im;
using godotfive.scripts.interfaces;
using Newtonsoft.Json;

#region IntermediateClasses

/*
 * Some classes have different naming conventions in Unity and Godot (e.g: Unity stores coordinates as x,y,z but godot
 * stores them as X,Y,Z). These intermediate classes allow us to be able to convert from and to the unity naming
 * convention and the godot convention using JSON.
 */

public class UnityVector3
{
	public float x = 0;
	public float y = 0;
	public float z = 0;

	public Vector3 GetGodotVector()
	{
		return new Vector3(x, y, -z);
	}

	public UnityVector3()
	{
	}
	public UnityVector3(Vector3 godotVector)
	{
		x = godotVector.X;
		y = godotVector.Y;
		z = -godotVector.Z;
	}
}

public class UnityColor
{
	public float r = 0.0f;
	public float g = 0.0f;
	public float b = 0.0f;
	public float a = 0.0f;

	public UnityColor()
	{
	}

	public Color GetGodotColor()
	{
		return new Color(r, g, b, a);
	}
}

public class ImageData
{
	public int cameraIndex;
	public DateTime dateTimeUTC;
	public string imageBase64;
}

#endregion
/*
 * Utility class with static functions regarding communication and messages
 */
namespace Utilities
{
	public static class Messages
	{
		private static readonly string VectorStart = "{";
		private static readonly string VectorEnd = "}";
		
		public static Vector3 ParseVector3FromMessage(ref string stringToParse, out bool succeed)
		{
			try
			{
				var unityVector3 = JsonConvert.DeserializeObject<UnityVector3>(stringToParse);
				if (unityVector3 == null)
				{
					succeed = false;
					return new Vector3();
				}

				succeed = true;
				return unityVector3.GetGodotVector();
			}
			catch(Exception e)
			{
				GD.PushWarning($"Found exception {e} when parsing {stringToParse} to UnityVector3");
			}

			succeed = false;
			return Vector3.Inf;
		}
		public static Color ParseColorFromMessage(ref string stringToParse, out bool succeed)
		{
			try
			{
				var unityVector3 = JsonConvert.DeserializeObject<UnityColor>(stringToParse);
				if (unityVector3 == null)
				{
					succeed = false;
					return new Color();
				}

				succeed = true;
				return unityVector3.GetGodotColor();
			}
			catch(Exception e)
			{
				GD.PushWarning($"Found exception {e} when parsing {stringToParse} to UnityColor");
			}

			succeed = false;
			return new Color();
		}
		public static float[] ParseArrayFromMessage(ref string stringToParse, out bool succeed,
			int expectedCount = -1)
		{
			string trimmedString = stringToParse.Trim();

			trimmedString = trimmedString.Replace(VectorStart, "");
			trimmedString = trimmedString.Replace(VectorEnd, "");
			trimmedString = trimmedString.Replace(" ", "");
			string[] splittedFloats = trimmedString.Split(",");

			float[] parsedFloats = new float[splittedFloats.Length];
			for (int i = 0; i < splittedFloats.Length; i++)
			{
				parsedFloats[i] = splittedFloats[i].ToFloat();
			}

			int numParsed = parsedFloats.Length;
			succeed = expectedCount <= 0 ? numParsed > 0 : numParsed == expectedCount;
			return parsedFloats;
		}
		
		
		
		public static void SendCommandMessage(Jid to, Vector3 positionVector)
		{
			var unityVector = new UnityVector3(positionVector);
			string positionAsString = JsonConvert.SerializeObject(unityVector);
			XMPPCommunicationComponent.SendMessage(positionAsString, to, "command");
		}
		
		public static void SendImageMessage(Jid to, ImageData imageData)
		{
			
			string messageBody = JsonConvert.SerializeObject(imageData); 
			XMPPCommunicationComponent.SendMessage(messageBody, to, "image");
		}
		public static void RegisterMessageReceiver(string name, IMessageReceiver messageReceiverInterface)
		{
			XMPPCommunicationComponent.GetInstance().RegisterNewMessageReceiver(name, messageReceiverInterface);
		}
		
		public static bool SendImage(string agentName, Image imageToSend)
		{
			return TCPCommunicationComponent.SendImageToAgent(agentName, imageToSend);
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
				GD.PushError($"[Utilities.Files.GetFileContent] File {pathToFile} doesn't exist");
				
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
		public static Vector3 OrientVector3(Vector3 vectorToOrient)
		{
			vectorToOrient.Z *= -1;
			return vectorToOrient;
		}
	}
}