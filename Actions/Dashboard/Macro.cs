namespace Actions.Dashboard
{
    internal class Macro
    {
        public virtual string Name { get; set; } = null!;
        public virtual string Content { get; set; } = null!;
        public virtual bool IsCommand { get; set; }

        public Macro() { }

        public Macro(string name, string content, bool isCommand)
        {
            Name = name;
            Content = content;
            IsCommand = isCommand;
        }
    }
}