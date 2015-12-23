namespace PapyrusDotNet.Converters.Papyrus2CSharp
{
    public struct TextRange
    {
        public int Row;
        public int Column;
        public int Length;
        public string Text;
        public object Key;

        public TextRange(object key, string text, int row, int column, int length)
            : this()
        {
            Key = key;
            Row = row;
            Column = column;
            Length = length;
            Text = text;
        }
    }
}