using System;
using System.Text;
using System.Windows;

namespace HatNewUI.Handlers
{
    public static class ErrorHandler
    {
        private static readonly string DefaultCaption;
        private static readonly string DefaultErrorMessage;

        static ErrorHandler()
        {
            //TODO: Get this from resources.
            DefaultCaption = "Error";
            DefaultErrorMessage = "An exception was caught";
        }

        /// <summary>
        /// Shows an error message
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        /// <param name="caption">The dialog's caption</param>
        /// <param name="secondaryMessage">The secondary message to be displayed</param>
        public static void ShowError(string message, string caption = null, string secondaryMessage = null)
        {
            if (caption == null) caption = DefaultCaption;
            ShowErrorUI(message, caption, secondaryMessage);
        }

        /// <summary>
        /// Shows an error message taking the error info from the exception.
        /// Alternatively, it allows the stacktrace to be shown.
        /// </summary>
        /// <param name="ex">The exception that was raised from the error</param>
        /// <param name="message">The message to be shown before the exception</param>
        /// <param name="showStackTrace">It indicates wether the stacktrace will be shown or not</param>
        public static void ShowError(this Exception ex, string message = "", bool showStackTrace = false)
        {
            var sb = new StringBuilder(message ?? DefaultErrorMessage);
            var traceSb = new StringBuilder();

            PrepareExceptionInfo(ex, sb, traceSb, showStackTrace);

            ShowErrorUI(sb.ToString(), DefaultCaption, traceSb.ToString());
        }


        public static string GetStackTrace(Exception e)
        {
            if (e == null) return null;
            var strackTrace = new System.Diagnostics.StackTrace(e);

            var frames = strackTrace.GetFrames();

            if (frames == null) return null;
            var sb = new StringBuilder();
            foreach (var frame in frames)
            {
                sb.AppendFormat("{0}({1}): {2}()\n", frame.GetFileName(),
                    frame.GetFileLineNumber(), frame.GetMethod().Name);
            }

            return sb.ToString();
        }


        public static void PrepareExceptionInfo(Exception ex, StringBuilder sb,
            StringBuilder traceSb, bool addStackTrace)
        {
            if (ex == null) return;
            if (addStackTrace)
            {
                if (traceSb.Length > 0) traceSb.AppendLine();
                traceSb.Append(GetStackTrace(ex) ?? string.Empty);
            }

            sb.AppendFormat(": {0}", ex.Message);

            if (ex.InnerException != null)
                PrepareExceptionInfo(ex.InnerException, sb, traceSb, addStackTrace);
        }

        private static void ShowErrorUI(string message, string caption, string stackTrace)
        {
            try
            {
                NotificationHandler.Show(message + "\n\n" + stackTrace, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch
            {
                //Log to
            }
        }
    }
}
