using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Command;

namespace HatNewUI.Helpers
{
    public class EventBinding : EventToCommand
    {



        public DependencyObject TriggerVisual
        {
            get { return (DependencyObject)GetValue(TriggerVisualProperty); }
            set { SetValue(TriggerVisualProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TriggerVisual.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggerVisualProperty =
            DependencyProperty.Register("TriggerVisual", typeof(DependencyObject), typeof(EventBinding), new PropertyMetadata(null));



    }
}
