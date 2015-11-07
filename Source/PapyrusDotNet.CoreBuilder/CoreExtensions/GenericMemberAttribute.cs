using System;

namespace PapyrusDotNet.CoreBuilder.CoreExtensions
{
    public class GenericMemberAttribute : Attribute
    {
        /// <summary>
        ///     Specify to PapyrusDotNet that a copy will be made
        ///     of this property to match the target/used type.
        ///     The name will automatically be [typename]_[membername]
        ///     This will automatically be resolved for your code.
        /// </summary>
        public GenericMemberAttribute()
        {
            // if none defined, an explicit copy of the member
            // to all used types.
        }

        public GenericMemberAttribute(string targetType, string memberName)
        {
            // [GenericMember("Int", "Add")]
            // 
        }
    }
}