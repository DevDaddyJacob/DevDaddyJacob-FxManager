namespace DevDaddyJacob.FxManager.Node.Runner.Models
{
    internal class ChildStateProps
    {
        public required string Mutex { get; set; }
        public required string NetEndpoint { get; set; }
        public required Action OnStatusUpdate { get; set; }
    }
}
