using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AutoTapGUI
{
    public class TextBoxTraceListener : TraceListener
    {
        private TextBox Target;
        private StringSendDelegate InvokeWrite;
        private string OutputFilePath;
        public bool IsOutput { get; set; }

        public TextBoxTraceListener(TextBox target)
        {
            Target = target;
            InvokeWrite = new StringSendDelegate(SendString);
            IsOutput = false;
        }
        public TextBoxTraceListener(TextBox target, string outputFilePath) : this(target)
        {
            OutputFilePath = outputFilePath;
            IsOutput = true;
        }
        public override async void Write(string message)
        {
            Target.Dispatcher.Invoke(InvokeWrite, new object[] { message });
            if (IsOutput)
            {
                using (StreamWriter sw = new StreamWriter(OutputFilePath, true, Encoding.Unicode))
                {
                    await sw.WriteAsync(string.Format("{0} - {1}", DateTime.Now, message));
                    sw.Close();
                }
            }
        }

        public override async void WriteLine(string message)
        {
            Target.Dispatcher.Invoke(InvokeWrite, new object[] { message + Environment.NewLine });
            if (IsOutput)
            {
                using (StreamWriter sw = new StreamWriter(OutputFilePath, true, Encoding.Unicode))
                {
                    await sw.WriteLineAsync(string.Format("{0} - {1}", DateTime.Now, message));
                    sw.Close();
                }
            }
        }

        private delegate void StringSendDelegate(string message);

        private void SendString(string message)
        {
            // No need to lock text box as this function will only
            // ever be executed from the UI thread
            Target.Text += string.Format("{0} - {1}",DateTime.Now, message);
        }
    }
}