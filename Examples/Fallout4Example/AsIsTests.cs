using PapyrusDotNet.Core;

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

        //public bool Is2_Test()
        //{
        //    //// Whenever the object is guaranteed to be of the same type, 
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