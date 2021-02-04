using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;

namespace Woerterbuch
{
    public class WordParserThread
    {
        public delegate void ArticleParsedEventHandler();

        private readonly IArticle _mIArticle;
        private Thread _mThread;

        public WordParserThread(IArticle iArticle)
        {
            _mIArticle = iArticle;
        }

        public Dictionary<string, WordInfo> WordDict { get; } = new Dictionary<string, WordInfo>();

        public event ArticleParsedEventHandler ArticleParsedEvent;

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
                var article = _mIArticle.GetNextArticle();

                while (article != null)
                {
                    if (article.Contains("<html>")) ParseHtml(article);

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
            var html = new HtmlDocument();
            html.LoadHtml(article);

            ParseHtmlNode(html.DocumentNode);
        }

        private void ParseHtmlNode(HtmlNode node)
        {
            if (!string.IsNullOrWhiteSpace(node.InnerText)) ParseText(node.InnerText);

            foreach (var childNode in node.ChildNodes)
                ParseHtmlNode(childNode);
        }

        private void ParseText(string txt)
        {
            var startPos = 0;
            int pos;
            var state = EParseState.None;
            var lastWord = "";

            for (pos = 0; pos < txt.Length; pos++)
            {
                var c = txt[pos];

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
                            var word = txt.Substring(startPos, pos - startPos);
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
                var word = txt.Substring(startPos, pos - startPos);
                WordFound(word, lastWord);
            }
        }

        private void WordFound(string word, string lastWord)
        {
            if (word.Length <= 1 || word.Length > 64) return;
            if (WordDict.ContainsKey(word))
                WordDict[word].IncreaseCount();
            else
                WordDict[word] = new WordInfo(word);

            if (lastWord.Length <= 1 || lastWord.Length > 64) return;
            if (!WordDict.ContainsKey(lastWord)) WordDict[lastWord] = new WordInfo(lastWord);
            WordDict[lastWord].AddNextWord(word);
        }

        private enum EParseState
        {
            None,
            Word,
            Escape
        }
    }
}