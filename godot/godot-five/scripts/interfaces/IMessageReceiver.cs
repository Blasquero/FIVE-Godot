namespace godotfive.scripts.interfaces;

public interface IMessageReceiver
{
	public void ReceiveMessage(CommandInfo CommandData, string SenderID);
}