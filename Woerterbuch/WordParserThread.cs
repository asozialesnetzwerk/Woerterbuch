using System;
using System.Threading;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace Woerterbuch
{
    public class WordParserThread
    {
        private readonly IArticle _mIArticle;
        private Thread _mThread;
        private readonly Dictionary <string, WordInfo> _wordDictionary = new Dictionary<string, WordInfo>();
        public delegate void ArticleParsedEventHandler();

        public event ArticleParsedEventHandler ArticleParsedEvent;

        public Dictionary <string, WordInfo> WordDict => _wordDictionary;

        public WordParserThread(IArticle iArticle)
        {
            _mIArticle = iArticle;
        }

        public void Start()
        {
            _mThread = new Thread(Run);
            _mThread.Start();
        }

        public void Join()
        {
            _mThread.Join();
        }

        private void Run()
        {
            try
            {
                string article = _mIArticle.GetNextArticle();

                while (article != null)
                {
                    if (article.Contains("<html>"))
                    {
                        ParseHtml(article);
                    }

                    if (ArticleParsedEvent != null)
                        ArticleParsedEvent();

                    article = _mIArticle.GetNextArticle();
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

        private enum EParseState { None, Word, Escape }

        private void ParseText(string txt)
        {
            int startPos = 0;
            int pos;
            EParseState state = EParseState.None;
            string lastWord = "";

            for (pos = 0; pos < txt.Length; pos++)
            {
                char c = txt[pos];

                switch (state)
                {
                    case EParseState.None:
                        if (Utils.IsGermanLetter(c))
                        {
                            startPos = pos;
                            state = EParseState.Word;
                        }

                        if (c == '&')
                            state = EParseState.Escape;

                        break;

                    case EParseState.Word:
                        if (!Utils.IsGermanLetter(c))
                        {
                            string word = txt.Substring(startPos, pos - startPos);
                            WordFound(word, lastWord);
                            
                            lastWord = word;
                            
                            if (c == '&')
                                state = EParseState.Escape;
                            else
                                state = EParseState.None;
                        }
                        break;

                    case EParseState.Escape:

                        if (c == ';')
                            state = EParseState.None;

                        break;
                }
            }

            if (state == EParseState.Word)
            {
                string word = txt.Substring(startPos, pos - startPos);
                WordFound(word, lastWord);
            }
        }

        private void WordFound(string word, string lastWord)
        {
            if ((word.Length <= 1) || (word.Length > 64)) return;
            if (_wordDictionary.ContainsKey(word))
            {
                _wordDictionary[word].IncreaseCount();
            }
            else
            {
                _wordDictionary[word] = new WordInfo(word);
            }

            if ((lastWord.Length <= 1) || (lastWord.Length > 64)) return;
            if (!_wordDictionary.ContainsKey(lastWord))
            {
                _wordDictionary[lastWord] = new WordInfo(lastWord);
            }
            _wordDictionary[lastWord].AddNextWord(word);
        }
    }
}

