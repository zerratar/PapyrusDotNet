using PapyrusDotNet.Core;

namespace Example1
{
    [Conditional, Hidden]
    public class AttributeExample : Actor
    {
        [InitialValue("Hello world!")] private string dummy;

        [Property, Auto] public string MyPropertyString;

        [Property, Auto] public Actor PlayerRef;

        [InitialValue(0)] private int totalHoursElapsed;

        [Property, Auto] public Weapon WeaponRef;

        public override void OnInit()
        {
            RegisterForSingleUpdateGameTime(1);
        }

        public override void OnUpdateGameTime()
        {
            totalHoursElapsed++;

            Debug.MessageBox(totalHoursElapsed + " hours spent ingame! And my name is " + PlayerRef.GetName());

            RegisterForSingleUpdateGameTime(1);
        }
    }
}