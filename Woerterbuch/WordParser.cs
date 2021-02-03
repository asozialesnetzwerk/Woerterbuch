using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Threading;

namespace Woerterbuch
{
    public class WordParser : IArticle
    {
        public delegate void ProgressEventHandler(int index, int count);

        public event ProgressEventHandler ProgressEvent;

        private int _mNumThreads;
        private Zim _mZim;
        private int _mNumArticlesParsed;
        private object _mLock = new object();
        Dictionary <string, WordInfo> _wordDictionary = new Dictionary<string, WordInfo>();

        public List <WordInfo> WordList
        {
            get
            {
                List <WordInfo> wordList = new List<WordInfo>();

                foreach (WordInfo val in _wordDictionary.Values)
                    wordList.Add(val);

                return wordList;
            }
        }

        public WordParser(int numThreads)
        {
            _mNumThreads = numThreads;
        }

        public void Clear()
        {
            _wordDictionary.Clear();
        }

        public void Parse(string zimFileName)
        {
            _mNumArticlesParsed = 0;

            List <WordParserThread> threadList = new List<WordParserThread>();
            int idx;

            using (_mZim = new Zim(zimFileName))
            {
                _mZim.Init();
                SendProgressEvent();

                for (idx = 0; idx < _mNumThreads; idx++)
                {
                    WordParserThread wpt = new WordParserThread(this);
                    wpt.ArticleParsedEvent += ArticleParsedEventHandler;

                    wpt.Start();
                    threadList.Add(wpt);
                }

                foreach (WordParserThread wpt in threadList)
                    wpt.Join();

                SendProgressEvent();
            }
            
            foreach (WordParserThread wpt in threadList)
            {
                foreach (var keyValPair in wpt.WordDict)
                {
                    if (_wordDictionary.ContainsKey(keyValPair.Key))
                    {
                        _wordDictionary[keyValPair.Key].Merge(keyValPair.Value);
                    }
                    else
                    {
                        _wordDictionary[keyValPair.Key] = keyValPair.Value;
                    }
                }
            }
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

        public string GetNextArticle()
        {
            lock (_mZim)
            {
                while (!_mZim.IsEnd() && (_mZim.GetArticleSize() == 0))
                    _mZim.Next();

                if (_mZim.IsEnd())
                    return null;

                string article = _mZim.GetArticle();
                _mZim.Next();

                return article;
            }
        }
    }
}

