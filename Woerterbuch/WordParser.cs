using System.Collections.Generic;

namespace Woerterbuch
{
    public class WordParser : IArticle
    {
        public delegate void ProgressEventHandler(int index, int count);

        private readonly object _mLock = new object();
        private int _mNumArticlesParsed;

        private readonly int _mNumThreads;
        private Zim _mZim;
        private readonly Dictionary<string, WordInfo> _wordDictionary = new Dictionary<string, WordInfo>();

        public WordParser(int numThreads)
        {
            _mNumThreads = numThreads;
        }

        public List<WordInfo> WordList
        {
            get
            {
                var wordList = new List<WordInfo>();

                foreach (var val in _wordDictionary.Values)
                    wordList.Add(val);

                return wordList;
            }
        }

        public string GetNextArticle()
        {
            lock (_mZim)
            {
                while (!_mZim.IsEnd() && _mZim.GetArticleSize() == 0)
                    _mZim.Next();

                if (_mZim.IsEnd())
                    return null;

                var article = _mZim.GetArticle();
                _mZim.Next();

                return article;
            }
        }

        public event ProgressEventHandler ProgressEvent;

        public void Clear()
        {
            _wordDictionary.Clear();
        }

        public void Parse(string zimFileName)
        {
            _mNumArticlesParsed = 0;

            var threadList = new List<WordParserThread>();
            int idx;

            using (_mZim = new Zim(zimFileName))
            {
                _mZim.Init();
                SendProgressEvent();

                for (idx = 0; idx < _mNumThreads; idx++)
                {
                    var wpt = new WordParserThread(this);
                    wpt.ArticleParsedEvent += ArticleParsedEventHandler;

                    wpt.Start();
                    threadList.Add(wpt);
                }

                foreach (var wpt in threadList)
                    wpt.Join();

                SendProgressEvent();
            }

            foreach (var wpt in threadList)
            foreach (var keyValPair in wpt.WordDict)
                if (_wordDictionary.ContainsKey(keyValPair.Key))
                    _wordDictionary[keyValPair.Key].Merge(keyValPair.Value);
                else
                    _wordDictionary[keyValPair.Key] = keyValPair.Value;
        }

        private void SendProgressEvent()
        {
            if (ProgressEvent != null)
                ProgressEvent(_mNumArticlesParsed, _mZim.Count);
        }

        private void ArticleParsedEventHandler()
        {
            lock (_mLock)
            {
                _mNumArticlesParsed++;
                SendProgressEvent();
            }
        }
    }
}