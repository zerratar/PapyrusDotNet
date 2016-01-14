using System;

namespace PapyrusDotNet.PapyrusAssembly.Extensions
{
    internal class For
    {
        public static void Do(int count, Action action)
        {
            for (var i = 0; i < count; i++) action();
        }
    }
}