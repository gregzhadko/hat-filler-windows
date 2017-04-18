using System;

namespace HatDesktop
{
    public interface IModalWindow
    {
        bool? DialogResult { get; set; }
        object DataContext { get; set; }
        event EventHandler Closed;
        void Show();
        void Close();
    }
}