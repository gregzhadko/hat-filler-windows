using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HatNewUI.UtilsObject
{
    public class DialogResult
    {
        readonly object[] _values;
        public MessageBoxResult Result { get; set; }

        public DialogResult(params object[] values)
        {
            _values = values;
            Result = MessageBoxResult.None;
        }

        public DialogResult(MessageBoxResult result, params object[] values)
        {
            _values = values;
            Result = result;
        }

        public T GetFirst<T>()
        {
            return _values.OfType<T>().First();
        }

        public T GetFirstOrDefault<T>()
        {
            return (_values == null || _values.Length == 0) ? default(T) :
                _values.OfType<T>().FirstOrDefault();
        }

        public IEnumerable<T> GetAll<T>()
        {
            return _values == null ?
                Enumerable.Empty<T>() :
                _values.OfType<T>();
        }
    }
}
