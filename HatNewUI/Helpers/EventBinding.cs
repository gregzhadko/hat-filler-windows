using System.Windows;
using GalaSoft.MvvmLight.Command;

namespace HatNewUI.Helpers
{
    public class EventBinding : EventToCommand
    {



        public DependencyObject TriggerVisual
        {
            get => (DependencyObject)GetValue(TriggerVisualProperty);
            set => SetValue(TriggerVisualProperty, value);
        }

        // Using a DependencyProperty as the backing store for TriggerVisual.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggerVisualProperty =
            DependencyProperty.Register("TriggerVisual", typeof(DependencyObject), typeof(EventBinding), new PropertyMetadata(null));



    }
}
