using SecsGemLib.Enums;

namespace SecsGemLib.Gem.Data
{
    public class GemAlarmData
    {
        public int ALID { get; }
        public string Description { get; }
        public AlarmState State { get; }

        public GemAlarmData(int id, string desc, AlarmState state)
        {
            ALID = id;
            Description = desc;
            State = state;
        }
    }
}
