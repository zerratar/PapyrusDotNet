using PapyrusDotNet.Core;

namespace Fallout4Example
{
    public class MyObjectReference : ObjectReference
    {
        public string HelloWorld;

        public bool HelloThere { get; set; }

        public override void OnInit()
        {
            Debug.MessageBox("Hello");

            base.OnInit();
        }
    }
}
