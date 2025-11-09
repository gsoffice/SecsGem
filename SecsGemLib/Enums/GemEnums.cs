namespace SecsGemLib.Enums
{
    public enum GemVariableType
    {
        SVID,
        ALID,
        ECID
    }

    public enum AlarmState
    {
        Set = 1,
        Clear = 2
    }

    public enum EventType
    {
        EquipmentEvent,
        AlarmEvent,
        VariableChange
    }
}
