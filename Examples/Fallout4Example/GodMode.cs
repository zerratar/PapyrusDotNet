using PapyrusDotNet.Core;

namespace Fallout4Example
{
    public class GodMode : ScriptObject
    {
        public override void OnInit()
        {
            Debug.SetGodMode(true);
            Debug.MessageBox("God Mode Activated!!");

            var player = Game.GetPlayer();
            var position = "x: " + player.X + ", y: " + player.Y + ", z: " + player.Z;

            Debug.MessageBox("Current player position: " + position);
        }
    }
}
