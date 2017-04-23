using GalaSoft.MvvmLight.Ioc;
using HatDesktop.Views;
using Microsoft.Practices.ServiceLocation;
using System.Diagnostics.CodeAnalysis;
using Model;

namespace HatDesktop.ViewModels
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IPackService, PackService>();
            SimpleIoc.Default.Register<RenamePackViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SpellCheckerViewModel>();

            SimpleIoc.Default.Register<IModalWindow>(() => new RenamePackView(), Constants.RenamePackView);
        }

        /// <summary>
        ///     Gets the Main property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();


        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public SpellCheckerViewModel SpellCheckerViewModel => ServiceLocator.Current.GetInstance<SpellCheckerViewModel>();

        /// <summary>
        ///     Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            SimpleIoc.Default.Unregister<MainViewModel>();
            SimpleIoc.Default.Unregister<PackService>();
            SimpleIoc.Default.Unregister<RenamePackViewModel>();
            SimpleIoc.Default.Unregister<SpellCheckerViewModel>();
        }
    }
}