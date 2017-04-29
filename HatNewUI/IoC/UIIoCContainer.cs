using System;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using HatNewUI.Helpers;
using HatNewUI.UtilsObject;

namespace HatNewUI.IoC
{
    public class UIIoCContainer
    {
        static UIIoCContainer()
        {
            if (ViewModelBase.IsInDesignModeStatic) return;
            var app = Application.Current;
            if (!app.ImplementsGenericType(typeof(IMVVMApp<>)))
                throw new Exception("The Current Application does not implement the MVVM interface.");
            app.RegisterApp();
        }

        #region Registering methods

        /// <summary>
        /// Method to register an interface and a class type with the IoC container abstracting
        /// away the used container. This IoC record will only be available in real mode (not design)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        public static void RegisterDesign<T, T2>(bool createInstanceImmediately = false)
            where T : class
            where T2 : class, T
        {
            if (ViewModelBase.IsInDesignModeStatic) Register<T, T2>(createInstanceImmediately);
        }

        /// <summary>
        /// Method to register a class with the IoC container. This IoC record will only be available
        /// in real mode (not design).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterDesign<T>(Func<T> factory = null, string token = null, bool createInstanceImmediately = false) where T : class
        {
            if (ViewModelBase.IsInDesignModeStatic) Register(factory, token, createInstanceImmediately);
        }

        /// <summary>
        /// Method to register an interface and a class type with the IoC container abstracting
        /// away the used container. This IoC record will only be available in real mode (not design)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        public static void RegisterReal<T, T2>(bool createInstanceImmediately = false)
            where T : class
            where T2 : class, T
        {
            if (!ViewModelBase.IsInDesignModeStatic) Register<T, T2>(createInstanceImmediately);
        }


        /// <summary>
        /// Method to register a class with the IoC container. This IoC record will only be available
        /// in real mode (not design).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterReal<T>(Func<T> factory = null, string token = null, bool createInstanceImmediately = false) where T : class
        {
            if (!ViewModelBase.IsInDesignModeStatic) Register(factory, token, createInstanceImmediately);
        }

        /// <summary>
        /// Method to be used to register a class with the IoC containter when
        /// it doesn't matter wether in design mode or not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>(Func<T> factory = null, string token = null, bool createInstanceImmediately = false) where T : class
        {
            if (factory == null) SimpleIoc.Default.Register<T>(createInstanceImmediately);
            else if (token == null) SimpleIoc.Default.Register(factory, createInstanceImmediately);
            else SimpleIoc.Default.Register(factory, token, createInstanceImmediately);
        }

        /// <summary>
        /// Private method to register an interface and a class type with the IoC container
        /// abstracting away the used container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        static void Register<T, T2>(bool createInstanceImmediately = false)
            where T : class
            where T2 : class, T
        {
            SimpleIoc.Default.Register<T, T2>(createInstanceImmediately);
        }
        #endregion


        #region Instantiating methods
        public static bool IsRegistered<T>(string key = null)
        {
            return string.IsNullOrWhiteSpace(key) ?
                SimpleIoc.Default.IsRegistered<T>() :
                SimpleIoc.Default.IsRegistered<T>(key);
        }

        public static T GetInstance<T>(string key = null)
        {
            return SimpleIoc.Default.GetInstance<T>(key);
        }

        public static object GetInstance(Type type, string key = null)
        {
            return SimpleIoc.Default.GetInstance(type, key);
        }
        #endregion
    }
}
