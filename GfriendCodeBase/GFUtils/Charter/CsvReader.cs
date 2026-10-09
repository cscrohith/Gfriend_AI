using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HP.GFriend.Utils.Charter
{
    internal class CsvReader : IEnumerable<List<string>>
    {
        private readonly string _filePath;
        private List<string> _lines;
        private List<List<string>> _tokens;
        private List<string> _headers;


        public string FilePath
        {
            get { return _filePath; }
        }
        public IList<string> Header
        {
            get { return _headers.AsReadOnly(); }
        }
        public IList<string> AllRows
        {
            get { return _lines.AsReadOnly(); }
        }
        public IList<List<string>> DataRows
        {
            get { return _tokens.AsReadOnly(); }
        }


        private CsvReader(string filePath)
        {
            this._filePath = filePath;
        }


        private void Read()
        {
            _lines = File.ReadAllLines(_filePath).ToList();
            _headers = _lines
                      .Take(1)
                      .Single()
                      .Split(new char[] { ',' }, StringSplitOptions.None)
                      .ToList();
            _tokens = _lines
                     .Skip(1)
                     .Select(l => l.Split(new char[] { ',' }, StringSplitOptions.None)
                                   .ToList())
                     .ToList();
        }


        public IEnumerator GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }


        IEnumerator<List<string>> IEnumerable<List<string>>.GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }


        public static class Builder
        {
            public static CsvReader CreateInstance(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    throw new ArgumentException();
                }
                CsvReader reader = new CsvReader(filePath);
                reader.Read();
                return reader;
            }
        }
    }
}