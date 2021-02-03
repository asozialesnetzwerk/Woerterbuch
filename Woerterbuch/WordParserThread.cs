using System;
using System.Threading;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace Woerterbuch
{
    public class WordParserThread
    {
        private IArticle m_iArticle;
        private Thread m_thread;
        private Dictionary <string, int> m_dictionary = new Dictionary<string, int>();

        public delegate void ArticleParsedEventHandler();

        public event ArticleParsedEventHandler ArticleParsedEvent;

        public Dictionary <string, int> Dictionary
        {
            get { return m_dictionary; }
        }

        public WordParserThread(IArticle iArticle)
        {
            m_iArticle = iArticle;
        }

        public void Start()
        {
            m_thread = new Thread(Run);
            m_thread.Start();
        }

        public void Join()
        {
            m_thread.Join();
        }

        private void Run()
        {
            try
            {
                string article = m_iArticle.GetNextArticle();

                while (article != null)
                {
                    if (article.Contains("<html>"))
                    {
                        ParseHtml(article);
                    }

                    if (ArticleParsedEvent != null)
                        ArticleParsedEvent();

                    article = m_iArticle.GetNextArticle();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR: " + exc.Message);
            }
        }

        private void ParseHtml(string article)
        {
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(article);

            ParseHtmlNode(html.DocumentNode);
        }

        private void ParseHtmlNode(HtmlNode node)
        {
            if (!String.IsNullOrWhiteSpace(node.InnerText))
            {
                ParseText(node.InnerText);
            }

            foreach (var childNode in node.ChildNodes)
                ParseHtmlNode(childNode);
        }

        private enum EParseState { NONE, WORD, ESCAPE }

        private void ParseText(string txt)
        {
            int startPos = 0;
            int pos;
            EParseState state = EParseState.NONE;

            for (pos = 0; pos < txt.Length; pos++)
            {
                char c = txt[pos];

                switch (state)
                {
                    case EParseState.NONE:
                        if (Utils.IsGermanLetter(c))
                        {
                            startPos = pos;
                            state = EParseState.WORD;
                        }

                        if (c == '&')
                            state = EParseState.ESCAPE;

                        break;

                    case EParseState.WORD:
                        if (!Utils.IsGermanLetter(c))
                        {
                            string word = txt.Substring(startPos, pos - startPos);
                            WordFound(word);

                            if (c == '&')
                                state = EParseState.ESCAPE;
                            else
                                state = EParseState.NONE;
                        }
                        break;

                    case EParseState.ESCAPE:

                        if (c == ';')
                            state = EParseState.NONE;

                        break;
                }
            }

            if (state == EParseState.WORD)
            {
                string word = txt.Substring(startPos, pos - startPos);
                WordFound(word);
            }
        }

        private void WordFound(string word)
        {
            if ((word.Length > 1) && (word.Length <= 64))
            {
                m_dictionary[word] = 1;
            }
        }
    }
}

