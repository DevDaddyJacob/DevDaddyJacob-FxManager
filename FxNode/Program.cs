using DevDaddyJacob.FxManager.Socket.Payloads.General;

namespace DevDaddyJacob.FxManager.Node
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            FxNode.Run(args);
#if DEBUG
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                await FxNode.Socket.SendMessage(new HeartbeatFrame());
            });
#endif
        }
    }
}