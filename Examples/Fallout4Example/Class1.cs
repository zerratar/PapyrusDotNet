using PapyrusDotNet.Core;

namespace Fallout4Example
{
    public class MyObjectReference : ObjectReference
    {
        public override void OnInit()
        {
            Debug.MessageBox("Hello");

            base.OnInit();
        }
    }
}
