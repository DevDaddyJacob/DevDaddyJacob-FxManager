namespace DevDaddyJacob.FxManager.Shared.ActionResults
{
    [Flags]
    internal enum ActionResultCodeScope : byte
    {
        All = FxHub | FxNode,
        FxHub = 0b_00000001,
        FxNode = 0b_00000010,
        _Unk_1 = 0b_00000100,
        _Unk_2 = 0b_00001000,
        _Unk_3 = 0b_00010000,
        _Unk_4 = 0b_00100000,
        _Unk_5 = 0b_01000000,
        _Unk_6 = 0b_10000000,
    }
}