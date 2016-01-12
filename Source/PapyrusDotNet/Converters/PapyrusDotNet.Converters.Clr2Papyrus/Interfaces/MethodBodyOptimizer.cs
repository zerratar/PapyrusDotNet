//     This file is part of PapyrusDotNet.
// 
//     PapyrusDotNet is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     PapyrusDotNet is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
//  
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public class MethodBodyOptimizer : IMethodBodyOptimizer
    {
        public IMethodBodyOptimizerResult Optimize(PapyrusMethodDefinition method)
        {
            var methodBody = method.Body;
            var variables = method.Body.Variables;
            var success = false;
            var optimizationRatio = 0d;

            // 1. find all assignment on these variables
            // 2. find all usage on the variables
            // - If a variable is never assigned or never used, it is safe to be removed
            // - If a variable is assigned but never used, it could still be needed.
            //     (Consider this: You have a property getter method that does some logic more than just returning the value) 
            //      -- So, depending on what kind of assigning; it should find out if its safe to be removed or not.


            return new MethodBodyOptimizerResult(method, success, optimizationRatio);
        }
    }
}