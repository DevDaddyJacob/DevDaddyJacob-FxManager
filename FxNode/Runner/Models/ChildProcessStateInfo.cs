namespace DevDaddyJacob.FxManager.Node.Runner.Models
{
    internal class ChildProcessStateInfo
    {
        #region Input

        public required int PID { get; set; }

        public required string Mutex { get; set; }

        public required string NetEndpoint { get; set; }

        #endregion Input

        #region Timings

        public required long TimestampStart { get; set; }
        public required long? TimestampKill { get; set; }
        public required long? TimestampExit { get; set; }
        public required long? TimestampClose { get; set; }

        #endregion Timings

        #region Status

        public required bool IsAlive { get; set; }

        public required ChildProcessState Status { get; set; }

        public required long Uptime { get; set; }

        #endregion Status
    }
}
