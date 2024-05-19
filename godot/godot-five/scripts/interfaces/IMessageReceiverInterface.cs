public interface IMessageReceiverInterface
{
    public void ProcessReceivedMessage(string senderID, string commandType, string commandData)
    {
        return;
    }

    public bool SubscribeToMessageReceiver()
    {
        var comManager = XMPPCommunicationManager.GetInstance();
        if (comManager == null)
        {
            //Log Error
            return false;
        }

        comManager.OnMessageReceived += ProcessReceivedMessage;
        return true;
    }
}