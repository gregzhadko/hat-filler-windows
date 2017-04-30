using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using HatNewUI.Handlers;

namespace HatNewUI.Helpers
{
    public static class Extensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> col)
        {
            return new ObservableCollection<T>(col);
        }

        public static T DeepCopy<T>(this T oSource)
        {
            T oClone;

            var dcs = new DataContractSerializer(typeof(T));

            using (var ms = new MemoryStream())
            {
                dcs.WriteObject(ms, oSource);
                ms.Position = 0;
                oClone = (T)dcs.ReadObject(ms);
            }
            return oClone;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> col)
        {
            return col == null || col.Count == 0;
        }

        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> function)
        {
            var toRemove = collection.Where(function).ToArray();
            foreach (var i in toRemove) collection.Remove(i);
        }

        public static bool ImplementsGenericType<T>(this T obj, Type type) where T : class
        {
            if (obj == null) return false;

            return obj.GetType().GetInterfaces()
               .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == type);
        }

        public static Application RegisterApp(this Application app)
        {
            if (app == null) return app;

            //TODO: Decide wether this will be kept here or the RegisterApp Method will be called programmatically
            //((dynamic)app).RegisterAppMessages();
            MessengerHelper.RegisterAppMessages((dynamic)app);
            //TODO: END Decide wether this will be kept here or the RegisterApp Method will be called programmatically

            app.BuildUnhandledExceptionHandler();

            return app;
        }

        internal static void BuildUnhandledExceptionHandler(this Application app)
        {
            app.DispatcherUnhandledException += (sender, e) =>
            {
                e.Handled = true;
                e.Exception.ShowError(showStackTrace: true);

                if (!(e.Exception is ReflectionTypeLoadException)) return;
                var rf = e.Exception as ReflectionTypeLoadException;

                foreach (var le in rf.LoaderExceptions)
                {
                    le.ShowError(showStackTrace: true);
                }
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                e.SetObserved();
                e.Exception.ShowError("An error occured on a parallel process", showStackTrace: true);
            };
        }
    }
}
