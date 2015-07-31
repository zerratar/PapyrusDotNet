namespace Example1
{
    using PapyrusDotNet.Core;
    using PapyrusDotNet.System;

    public class FacialExpressions : Actor
    {
        [Property, Auto]
        public Face playerExpression;

        public int activeExpression;

        public int expressionCount = 8;

        public double lastUpdate = DateTime.Now;

        public override void OnInit()
        {
            RegisterForSingleUpdateGameTime(1);
        }

        public override void OnUpdateGameTime()
        {
            if (DateTime.Now - lastUpdate > 500)
            {
                if (activeExpression < expressionCount)
                    activeExpression++;
                else
                    activeExpression = 0;

                playerExpression = (Face)activeExpression;
                lastUpdate = DateTime.Now;
            }
            RegisterForSingleUpdateGameTime(1);

            Debug.Trace("New expression: " + activeExpression, 0);

            if (playerExpression == Face.Frown)
            {
                Debug.Trace("Aww! Don't be frowning!", 0);
            }
        }
    }

    public enum Face
    {
        Normal,
        Frown,
        Grin,
        Angry,
        Raged,
        Happy,
        Sad,
        Scared
    }
}
