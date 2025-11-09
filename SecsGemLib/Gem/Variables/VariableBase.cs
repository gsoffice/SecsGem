namespace SecsGemLib.Gem.Variables
{
    public class VariableBase
    {
        public int Id { get; }
        public string Name { get; }
        public object Value { get; set; }
        public string Description { get; }

        public VariableBase(int id, string name, object value = null, string desc = "")
        {
            Id = id;
            Name = name;
            Value = value;
            Description = desc;
        }
    }
}
