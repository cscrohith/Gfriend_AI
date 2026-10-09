using FastColoredTextBoxNS;
using System;
using System.IO;
using System.Text;

namespace HP.GFriend.UI
{
    class TextBoxStreamWriter : StringWriter
    {
        private FastColoredTextBox _output = null;
        private StreamWriter _writer;
        private MemoryStream _memStream;

        public event EventHandler<EventArgs> TextBoxDisposed;

        public TextBoxStreamWriter(FastColoredTextBox output)
        {
            _output = output;
            _memStream = new MemoryStream(100000);
            _writer = new StreamWriter(_memStream);            
            _writer.AutoFlush = true;            
        }

        public void ChangeOutputTarget(FastColoredTextBox newOutput)
        {
            _output = newOutput;
        }

        public override void Write(char value)
        {
            try
            {
                if (_output == null || _output.Disposing || _output.IsDisposed)
                {
                    TextBoxDisposed?.Invoke(this, null);
                }
            }
            catch(Exception)
            {
                TextBoxDisposed?.Invoke(this, null);
            }
            string output = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + value;

            base.Write(output);
            try
            {
                _output.AppendText(output);
            }
            catch(ApplicationException)
            {
                TextBoxDisposed?.Invoke(this, null);
            }
            _writer.Write(output);
        }

        public override void Write(string value)
        {
            try
            {
                if (_output == null || _output.Disposing || _output.IsDisposed)
                {
                    TextBoxDisposed?.Invoke(this, null);
                }
            }
            catch (Exception)
            {
                TextBoxDisposed?.Invoke(this, null);
            }
            string output = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + value;

            base.Write(output);
            try
            {
                _output.AppendText(output);
            }
            catch (InvalidOperationException)
            {
                // Consume Exception when new output box is creating
            }
            catch (ApplicationException)
            {
                TextBoxDisposed?.Invoke(this, null);
            }
            _writer.Write(output);

            //_output.AppendText(value.ToString());
        }

        public override void WriteLine(string value)
        {
            try
            {
                if (_output == null || _output.Disposing || _output.IsDisposed)
                {
                    TextBoxDisposed?.Invoke(this, null);
                }
            }
            catch (Exception)
            {
                TextBoxDisposed?.Invoke(this, null);
            }
            string output = value + Environment.NewLine;
            try
            {
                _output.AppendText(output);
            }
            catch(InvalidOperationException)
            {
                // Consume Exception when new output box is creating
            }
            catch (ApplicationException)
            {
                TextBoxDisposed?.Invoke(this, null);
            }
            _writer.Write(output);
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
