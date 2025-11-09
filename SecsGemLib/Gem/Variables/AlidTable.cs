using System.Collections.Generic;
using SecsGemLib.Enums;

namespace SecsGemLib.Gem.Variables
{
    public class AlidTable
    {
        private readonly Dictionary<int, (string desc, AlarmState state)> _alarms = new();

        public void Add(int id, string desc)
            => _alarms[id] = (desc, AlarmState.Clear);

        public void Set(int id) => Update(id, AlarmState.Set);
        public void Clear(int id) => Update(id, AlarmState.Clear);

        private void Update(int id, AlarmState newState)
        {
            if (_alarms.ContainsKey(id))
                _alarms[id] = (_alarms[id].desc, newState);
        }

        public AlarmState GetState(int id)
            => _alarms.ContainsKey(id) ? _alarms[id].state : AlarmState.Clear;

        public IEnumerable<KeyValuePair<int, (string desc, AlarmState state)>> GetAll() => _alarms;
    }
}
