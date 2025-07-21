using System.Text;

namespace CyclingTrainer.SessionAnalyzer.Console
{
    internal class CsvWriter : IDisposable
    {
        private string _filePath;
        private StreamWriter _writer;
        private bool _headersWritten;
        private int _dataCounter;

        internal CsvWriter(string filePath)
        {
            _filePath = filePath;
            _writer = new StreamWriter(_filePath, false, Encoding.UTF8);
            _headersWritten = false;
            _dataCounter = 0;
        }

        internal void WriteHeaders()
        {
            if (!_headersWritten)
            {
                string[] headers = { "Time", "Power[W]" };
                _writer.WriteLine(string.Join(",", headers));
                _headersWritten = true;
            }
        }

        internal void WriteData(string time, string power)
        {
            if (!_headersWritten)
            {
                WriteHeaders();
            }

            string[] row = {
                time,
                power,
            };
            _writer.WriteLine(string.Join(",", row));
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}