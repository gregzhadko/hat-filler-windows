using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HatDesktop
{
    public class AttachedProperties
    {
        public static string GetTag(DependencyObject obj)
        {
            return (string)obj.GetValue(TagProperty);
        }

        public static void SetTag(DependencyObject obj, string value)
        {
            obj.SetValue(TagProperty, value);
        }

        // Using a DependencyProperty as the backing store for Tag.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagProperty =
            DependencyProperty.RegisterAttached("Tag", typeof(string), typeof(AttachedProperties), new PropertyMetadata(null));
    }
}
