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

        private int m_numThreads;
        private ZIM m_zim;
        private int m_numArticlesParsed;
        private object m_lock = new object();
        Dictionary <string, int> m_dictionary = new Dictionary<string, int>();

        public List <string> WordList
        {
            get
            {
                List <string> wordList = new List<string>();

                foreach (string key in m_dictionary.Keys)
                    wordList.Add(key);

                return wordList;
            }
        }

        public WordParser(int numThreads)
        {
            m_numThreads = numThreads;
        }

        public void Clear()
        {
            m_dictionary.Clear();
        }

        public void Parse(string zimFileName)
        {
            m_numArticlesParsed = 0;

            List <WordParserThread> threadList = new List<WordParserThread>();
            int idx;

            using (m_zim = new ZIM(zimFileName))
            {
                m_zim.Init();
                SendProgressEvent();

                for (idx = 0; idx < m_numThreads; idx++)
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
                foreach (var item in wpt.Dictionary)
                {
                    if (!m_dictionary.ContainsKey(item.Key))
                        m_dictionary[item.Key] = 1;
                }
            }
        }

        private void SendProgressEvent()
        {
            if (ProgressEvent != null)
                ProgressEvent(m_numArticlesParsed, m_zim.Count);
        }

        private void ArticleParsedEventHandler()
        {
            lock (m_lock)
            {
                m_numArticlesParsed++;
                SendProgressEvent();
            }
        }

        public string GetNextArticle()
        {
            lock (m_zim)
            {
                while (!m_zim.IsEnd() && (m_zim.GetArticleSize() == 0))
                    m_zim.Next();

                if (m_zim.IsEnd())
                    return null;

                string article = m_zim.GetArticle();
                m_zim.Next();

                return article;
            }
        }
    }
}

