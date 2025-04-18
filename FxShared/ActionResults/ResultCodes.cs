using DevDaddyJacob.ActionResults;

namespace DevDaddyJacob.FxManager.Shared.ActionResults
{
    internal static class ResultCodes
    {
        public static class General
        {
            public static ActionResultCode UnknownError
                => new(false, (byte)ActionResultCodeScope.All, (uint)ActionResultCodeFamily.General, 0);
        }

        public static class FxRunnerPowerAction
        {
            public static ActionResultCode CannotStartTheServerWhileShuttingDown
                => new(false, (byte)ActionResultCodeScope.FxHub, (uint)ActionResultCodeFamily.FxRunnerPowerAction, 0);

            public static ActionResultCode ServerAlreadyStarted
                => new(false, (byte)ActionResultCodeScope.FxHub, (uint)ActionResultCodeFamily.FxRunnerPowerAction, 1);

            public static ActionResultCode FailedToSetupSpawnVars
                => new(false, (byte)ActionResultCodeScope.FxHub, (uint)ActionResultCodeFamily.FxRunnerPowerAction, 2);

            public static ActionResultCode MisingConfiguration
                => new(false, (byte)ActionResultCodeScope.FxHub, (uint)ActionResultCodeFamily.FxRunnerPowerAction, 3);

            public static ActionResultCode Started
                => new(true, (byte)ActionResultCodeScope.FxHub, (uint)ActionResultCodeFamily.FxRunnerPowerAction, 4);

            public static ActionResultCode RestartAlreadyInProgress
                => new(false, (byte)ActionResultCodeScope.FxHub, (uint)ActionResultCodeFamily.FxRunnerPowerAction, 5);
        }
    }
}