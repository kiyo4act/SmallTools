using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace AutoTapGUI
{
    public class ListBoxTraceListener : TraceListener
    {
        private ListBox Target;
        private StringSendDelegate InvokeWrite;

        public ListBoxTraceListener(ListBox target)
        {
            Target = target;
            InvokeWrite = new StringSendDelegate(SendString);
        }

        public override void Write(string message)
        {
            Target.Dispatcher.Invoke(InvokeWrite, new object[] { message });
        }

        public override void WriteLine(string message)
        {
            Target.Dispatcher.Invoke(InvokeWrite, new object[] { message });
        }

        private delegate void StringSendDelegate(string message);

        private void SendString(string message)
        {
            // No need to lock text box as this function will only
            // ever be executed from the UI thread
            Target.Items.Add(string.Format("{0} - {1}", DateTime.Now, message));
        }
    }
}