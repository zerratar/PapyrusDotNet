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

using PapyrusDotNet.Core;

#endregion

namespace Fallout4Example
{
    public class AsIsTests
    {
        private Form form;

        private ObjectReference objRef;

        // Works
        public bool Is_Test()
        {
            var isit = form is ObjectReference;
            return isit;
        }

        //    //// Whenever the object is guaranteed to be of the same type, 
        //{

        //public bool Is2_Test()
        //    //// it doesnt work (currently)
        //    //var isit = objRef is Form;

        //    // Works
        //    var isit = objRef != null;
        //    return isit;
        //}

        //// Does not work
        //public Form As_Test()
        //{
        //    var isit = objRef as Form;
        //    return isit;
        //}
    }
}