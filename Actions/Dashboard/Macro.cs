namespace Actions.Dashboard
{
    internal class Macro
    {
        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
        public bool IsCommand { get; set; }

        public Macro() { }

        public Macro(string name, string content, bool isCommand)
        {
            Name = name;
            Content = content;
            IsCommand = isCommand;
        }
    }
}