using Godot;
using Artalk.Xmpp;
using Artalk.Xmpp.Im;

/*
 * Utility class with static functions regarding communication and messages
 */

public partial class CommUtils : GodotObject
{
    public static void SendMessage(Jid to, string messageBody)
    {
        var message = new Message(to: to, body: messageBody, type: MessageType.Chat);
        XMPPCommunicationManager.SendMessage(message);
    }
}
