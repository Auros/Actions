namespace Actions.Dashboard
{
    internal class Macro
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public bool IsCommand { get; set; }

        public Macro(string name, string content, bool isCommand)
        {
            Name = name;
            Content = content;
            IsCommand = IsCommand;
        }
    }
}