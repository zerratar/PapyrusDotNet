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

namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public interface IPapyrusUsageFinder : IPapyrusItemFinder
    {
        /// <summary>
        ///     Finds the method usage.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        IFindResult FindMethodUsage(string methodName);

        /// <summary>
        ///     Finds the property usage.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFindResult FindPropertyUsage(string propertyName);

        /// <summary>
        ///     Finds the field usage.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        IFindResult FindFieldUsage(string fieldName);

        ///// </summary>
        ///// Finds the type usage.

        ///// <summary>
        ///// <param name="typeName">Name of the type.</param>
        ///// <returns></returns>
        //IFindUsageResult FindTypeUsage(string typeName);
    }
}