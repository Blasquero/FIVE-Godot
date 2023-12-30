using Godot;
using Artalk.Xmpp;
using Artalk.Xmpp.Im;

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
    }

    public static class Math
    {
        //Godot is left-handed
        public static void OrientVector3(ref Vector3 vectorToOrient)
        {
            vectorToOrient.Z *= -1;
        }
    }
}