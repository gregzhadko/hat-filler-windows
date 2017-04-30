using System;

namespace HatNewUI.Helpers.Attributes
{

    public class IntOrderAttribute : Attribute
    {
        public IntOrderAttribute(int order) { Order = order; }
        public int Order { get; set; }
    }
}
