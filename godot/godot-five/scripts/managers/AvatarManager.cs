#nullable enable
using Godot;
using System.Collections.Generic;
using Artalk.Xmpp;

//Class in charge of managing the avatars on the level, spawning and assigning them to agents
/*
 * TODO: Rename to entity manager and have it fill the map using MapManager::MapInfo?
 * Possible upgrades:
 *      -Add pooling functionality
 */
public partial class AvatarManager : Node
{
    private static readonly AvatarManager instance = new AvatarManager();
    public static AvatarManager GetAvatarManager() => instance;
    
    //Upgrade: Update this to a Dictionary<AgentType, Queue<Node> so we can have different types of avatars
    private Queue<Node> AvailableAvatars = new Queue<Node>();
    private Dictionary<Jid, Node> AssignedAvatars = new Dictionary<Jid, Node>();


    private bool InternalHandleAgentRequest(Jid requesterJid)
    {
        if (AvailableAvatars.Count == 0)
        {
            GD.PushWarning("AgentManager::InternalRequest: An agent was requested but all agents are assigned");
            return false;
        }

        if (AssignedAvatars.ContainsKey(requesterJid))
        {
            GD.Print($"AgentManager::InternalRequest: Agent {requesterJid} requested an avatar, but it already has one assigned");
            return false;
        }
        
        Node agentToAssign = AvailableAvatars.Dequeue();
        AssignedAvatars.Add(requesterJid,agentToAssign);
        return true;
    }

    public bool RegisterAvatar(Node avatarToRegister)
    {
        if (instance.AvailableAvatars.Contains(avatarToRegister))
        {
            GD.PushWarning($"AgentManager::RegisterAvatar: Avatar {avatarToRegister} already registered");
            return false;
        }
        
        AvailableAvatars.Enqueue(avatarToRegister);
        return true;
    }
    
    #region Getters

    public Node? GetAvatarAssignedToJid(Jid jid) => AssignedAvatars.GetValueOrDefault(jid);
    public int GetNumberOfAvailableAvatars() => AvailableAvatars.Count;

    #endregion
}
