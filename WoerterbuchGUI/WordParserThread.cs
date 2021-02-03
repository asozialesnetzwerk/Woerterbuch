using System;
using System.Threading;
using System.Collections.Generic;
using Utils;
using Majestic12;
using Gtk;

namespace WoerterbuchGUI
{
    public class WordParserThread
    {
        private IArticle m_iArticle;
        private Thread m_thread;
        private Dictionary <string, int> m_dictionary = new Dictionary<string, int>();

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
                    ParseArticle(article);

                    article = m_iArticle.GetNextArticle();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR: " + exc.Message);
            }
        }

        public void ParseArticle(string article)
        {
            if (article.Contains("<html>"))
            {
                using (HTMLparser parser = new HTMLparser(article))
                {
                    HTMLchunk chunk = parser.ParseNext();

                    while (chunk != null)
                    {
                        if (chunk.oType == HTMLchunkType.Text)
                        {
                            ParseText(chunk.oHTML);
                        }

                        chunk = parser.ParseNext();
                    }
                }
            }
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
                        if (Letters.IsGermanLetter(c))
                        {
                            startPos = pos;
                            state = EParseState.WORD;
                        }

                        if (c == '&')
                            state = EParseState.ESCAPE;

                        break;

                    case EParseState.WORD:
                        if (!Letters.IsGermanLetter(c))
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

