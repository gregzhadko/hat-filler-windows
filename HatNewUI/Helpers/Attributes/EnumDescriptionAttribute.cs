using System;

namespace HatNewUI.Helpers
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class EnumDescriptionAttribute : Attribute
    {
        public string Description { get; private set; }

        public EnumDescriptionAttribute(string description)
        {
            Description = description;
        }
        public override string ToString()
        {
            return Description;
        }
    }


    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class EnumDescriptionsAttribute : Attribute
    {
        public string[] Descriptions { get; private set; }

        public EnumDescriptionsAttribute(params string[] description)
        {
            Descriptions = description;
        }
    }
}
