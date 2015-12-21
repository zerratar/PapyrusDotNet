using System;

namespace PapyrusDotNet.Common
{
    public class Hotkeys
    {
        public readonly string Text;
        public readonly ConsoleKey Key;
        public readonly Action Action;
        public string UsageKey => $"{Key}) - {Text}";

        public Hotkeys(string text, ConsoleKey key, Action action)
        {
            this.Text = text;
            this.Key = key;
            this.Action = action;
        }
    }
}