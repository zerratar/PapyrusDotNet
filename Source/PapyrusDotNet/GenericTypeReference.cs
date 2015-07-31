namespace PapyrusDotNet
{
    public class GenericTypeReference
    {
        public string SourceClass { get; set; }
        public string Type { get; set; }
        public GenericTypeReference(string t, string c = null)
        {
            Type = t;
            SourceClass = c;
        }
    }
}