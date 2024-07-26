#nullable enable

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
		private static readonly string VectorStart = "{";
		private static readonly string VectorEnd = "}";

		public static void SendMessage(Jid to, string messageBody)
		{
			var message = new Message(to: to, body: messageBody, type: MessageType.Chat);
			var x = message.Data.OwnerDocument.CreateElement("x", "jabber:x:data");
			x.SetAttribute("type", "form");
			var t = x.OwnerDocument.CreateElement("title");
			t.InnerText = "spade:x:metadata";
			var f = x.OwnerDocument.CreateElement("field");
			f.SetAttribute("var", "five");
			f.SetAttribute("type", "text-single");
			var v = f.OwnerDocument.CreateElement("value");
			v.InnerText = "command";

			f.AppendChild(v);
			x.AppendChild(f);
			x.AppendChild(t);
			message.Data.AppendChild(x);
			XMPPCommunicationComponent.SendMessage(message);
		}

		public static bool SendImage(string agentName, Image imageToSend)
		{
			return TCPCommunicationComponent.SendImageToAgent(agentName, imageToSend);
		}

		public static string CreateMessageFromVector3(ref Vector3 vectorToParse)
		{
			string parsedArray = vectorToParse.ToString();
			string fixedString = parsedArray.Substring(1, parsedArray.Length - 2);
			return VectorStart + fixedString + VectorEnd;
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