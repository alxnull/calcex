using System;

namespace Bluegrams.Calcex.CalcexConsole
{
    public class Command
    {
        public string Name { get; private set; }
        public string[] Params { get; private set; }
        public Action<string[]> Action { get; private set; }
        public string HelpText { get; private set; }
        public int MinArgs { get; set; }
        public int MaxArgs { get; set; }
        public bool InteractiveOnly { get; private set; }

        public Command(string symbol, string[] parameters, Action<string[]> action, string help, bool interactiveOnly = false)
        {
            this.Name = symbol;
            this.Params = parameters;
            this.Action = action;
            this.HelpText = help;
            this.MinArgs = parameters?.Length ?? 0;
            this.MaxArgs = parameters?.Length ?? 0;
            this.InteractiveOnly = interactiveOnly;
        }

        public override string ToString()
        {
            string s = Name;
            if (Params != null)
            {
                int count = 0;
                foreach (var param in Params)
                {
                    count++;
                    if (count > MinArgs) s += $" (<{param}>)";
                    else s += $" <{param}>";
                }
            }
            return s;
        } 
    }
}