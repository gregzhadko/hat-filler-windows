using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HatNewUI.Helpers.Attributes;

namespace HatNewUI.Helpers
{
    public static class Util
    {
        public static T? TryParseObjectToEnumNullable<T>(object value) where T : struct
        {
            T retval;
            var parsingCompleted = Enum.TryParse((value ?? "").ToString(), out retval);

            return parsingCompleted ? retval : (T?)null;
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return String.Empty;

            Object value = null;
            var type = obj.GetType();

            if (propertyName.Contains("."))
            {
                var innerPropertyName = propertyName.Split('.')[0];

                var info = type.GetProperty(innerPropertyName);

                if (info != null)
                {
                    value = GetPropertyValue(info.GetValue(obj, null),
                        propertyName.Replace(innerPropertyName + ".", String.Empty));
                }
            }
            else
            {
                var info = type.GetProperty(propertyName);

                if (info != null)
                {
                    var result = info.GetValue(obj, null);

                    if (result != null)
                    {
                        value = result;
                    }
                }
            }

            return value ?? String.Empty;
        }
        public static string GetExceptionsMessages<T>(this T exception) where T : Exception
        {
            Exception ex = exception;
            var sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine(ex.Message);
                ex = ex.InnerException;
            }
            return sb.ToString();
        }
        public static void AsyncAction(Action action,
            Action<Task> callback = null, bool callbackOnUIThread = true)
        {
            var task = new Task(action);
            if (callback != null)
            {
                if (callbackOnUIThread) task.ContinueWith(x => Application.Current.Dispatcher.Invoke(() => callback(x)));
                else task.ContinueWith(callback);
            }
            task.Start();
        }

        public static void AsyncAction<T>(Func<T> action,
            Action<Task<T>> callback = null, bool callbackOnUIThread = true)
        {
            var task = new Task<T>(action);
            if (callback != null)
            {
                if (callbackOnUIThread) task.ContinueWith(x => Application.Current.Dispatcher.Invoke(() => callback(x)));
                else task.ContinueWith(callback);
            }
            task.Start();
        }


        public static string Md5Hash(string text, string salt = "")
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text
            md5.ComputeHash(Encoding.ASCII.GetBytes(text + salt));

            //get hash result after compute it
            byte[] result = md5.Hash;

            var strBuilder = new StringBuilder();
            //change it into 2 hexadecimal digits
            //for each byte
            foreach (var b in result)
                strBuilder.Append(b.ToString("x2"));

            return strBuilder.ToString();
        }

        public static string ConvertToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static string GetEnumDescription<T>(this T value) where T : struct
        {
            return GetEnumDescriptionText(typeof(T), value);
        }

        public static string GetEnumDescriptionText(Type type, object value)
        {
            if (value == null)
            {
                return String.Empty;
            }

            var field = type.GetField(value.ToString());
            var enumDescriptionAttribute = field.GetCustomAttributes(typeof(EnumDescriptionAttribute), false).FirstOrDefault() as EnumDescriptionAttribute;

            return enumDescriptionAttribute != null ? enumDescriptionAttribute.Description : String.Empty;
        }

        public static int GetEnumIntOrder(Type type, object value)
        {
            if (value == null)
            {
                return -1;
            }

            var field = type.GetField(value.ToString());
            var intOrderAttribute = field.GetCustomAttributes(typeof(IntOrderAttribute), false).FirstOrDefault() as IntOrderAttribute;

            return intOrderAttribute != null ? intOrderAttribute.Order : -1;
        }
    }
}
