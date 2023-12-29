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

    public static class Avatars
    {
        public static bool RegisterAvatarAsAvailable(Node avatarToRegister)
        {
            return AvatarManager.GetAvatarManager().RegisterAvatar(avatarToRegister);
        }
    }
}