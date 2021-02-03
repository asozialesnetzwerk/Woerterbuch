using System;
using System.Collections.Generic;
using System.Threading;

namespace WoerterbuchGUI
{
    public class WordParser : IArticle
    {
        public delegate void ProgressEventHandler(int index, int count);

        public event ProgressEventHandler ProgressEvent;

        private int m_numThreads;
        private volatile int m_numArticlesParsed;
        private volatile int m_numArticles;
        private ZIM m_zim;

        private object m_lock = new object();

        Dictionary <string, int> m_dictionary = new Dictionary<string, int>();
        private volatile bool m_requestStop = false;

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

        public void Stop()
        {
            m_requestStop = true;
        }

        public void Parse(string zimFileName)
        {
            m_requestStop = false;
            m_numArticlesParsed = 0;

            List <WordParserThread> threadList = new List<WordParserThread>();

            using (m_zim = new ZIM(zimFileName))
            {
                m_numArticles = m_zim.GetArticleCount();

                SendProgressEvent();

                int idx;
                for (idx = 0; idx < m_numThreads; idx++)
                {
                    WordParserThread wpt = new WordParserThread(this);
                    wpt.Start();
                    threadList.Add(wpt);
                }

                foreach (WordParserThread wpt in threadList)
                    wpt.Join();
            }

            SendProgressEvent();

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
                ProgressEvent(m_numArticlesParsed, m_numArticles);
        }

        public string GetNextArticle()
        {
            lock (m_lock)
            {
                if (m_requestStop)
                    return null;
                
                string article = null;

                if (m_numArticlesParsed < m_numArticles)
                {
                    m_zim.GoToArticle(m_numArticlesParsed);
                    article = m_zim.GetArticle();

                    m_numArticlesParsed++;
                }

                SendProgressEvent();
                return article;
            }
        }
    }
}

