using System;
using System.Collections.Generic;
using System.IO;
using SpellCheck;
using System.Globalization;

namespace Woerterbuch
{
    public class MainClass
    {
        private DateTime _mStartTime;

        public List <WordInfo> ParseWords(string[] zimFiles)
        {
            WordParser wp = new WordParser(4);
            wp.ProgressEvent += ProgressEventHandler;

            foreach (string zimFile in zimFiles)
            {
                _mPromille = -1;
                _mStartTime = DateTime.Now;
                wp.Parse(zimFile);
            }

            wp.ProgressEvent -= ProgressEventHandler;

            return wp.WordList;
        }

        public void CreateWordList(string[] zimFiles)
        {
            List <WordInfo> wordList = ParseWords(zimFiles);
            Console.WriteLine("num words: " + wordList.Count);

            Console.WriteLine("sort word list ...");
            wordList.Sort();
            SaveWordList(wordList, "word_list_all.txt");

            CreateSpellCheckedWordList(wordList);
            CreateUppercaseSpellCheckedWordList(wordList);

            Console.WriteLine("done");
        }

        private void CreateSpellCheckedWordList(List <WordInfo> wordList)
        {
            List <WordInfo> wordListSpellChecked = new List<WordInfo>();

            Console.WriteLine("create spell checked word list ...");

            using (Hunspell hunspell = new Hunspell("/usr/share/hunspell/de_DE.aff", "/usr/share/hunspell/de_DE.dic"))
            {
                foreach (WordInfo wordInfo in wordList)
                {
                    string word = wordInfo.GetWord();
                    if (word.Length < 64)
                    {
                        if (hunspell.SpellCheck(word))
                            wordListSpellChecked.Add(wordInfo);
                    }
                }
            }

            Console.WriteLine("num words spell checked: " + wordListSpellChecked.Count);

            SaveWordList(wordListSpellChecked, "word_list_spell_checked.txt");
        }

        private void CreateUppercaseSpellCheckedWordList(List <WordInfo> wordList)
        {
            Dictionary <string, WordInfo> dictionary = new Dictionary<string, WordInfo>();
            CultureInfo ci = new CultureInfo("de-DE", false);

            Console.WriteLine("create uppercase spell checked word list ...");

            foreach (WordInfo wordInfo in wordList)
            {
                WordInfo wordInfoUpper = wordInfo.CopyUpper();
                string word = wordInfoUpper.GetWord();
                if (dictionary.ContainsKey(word))
                {
                    dictionary[word].Merge(wordInfoUpper);
                }
                else
                {
                    dictionary[word] = wordInfoUpper;
                }
            }

            List <WordInfo> wordListUppercaseSpellChecked = new List<WordInfo>();

            using (Hunspell hunspell = new Hunspell("/usr/share/hunspell/de_DE.aff", "/usr/share/hunspell/de_DE.dic"))
            {
                foreach (WordInfo wordInfo in dictionary.Values)
                {
                    if (wordInfo.GetWord().Length < 64)
                    {
                        if (hunspell.SpellCheck(wordInfo.GetWord()))
                            wordListUppercaseSpellChecked.Add(wordInfo);
                    }
                }
            }

            Console.WriteLine("num words uppercase spell checked: " + wordListUppercaseSpellChecked.Count);

            SaveWordList(wordListUppercaseSpellChecked, "word_list_uppercase_spell_checked.txt");
        }

        private void SaveWordList(List <WordInfo> list, string fileName)
        {
            using (StreamWriter w = new StreamWriter(fileName))
            {
                foreach (WordInfo wordInfo in list)
                {
                    w.WriteLine(wordInfo.ToString());
                }
            }
        }

        private double _mPromille;

        public void ProgressEventHandler(int index, int count)
        {
            double promille = Math.Ceiling(1000.0f * (double)index / (double)count);
            if (promille != _mPromille)
            {
                _mPromille = promille;

                if (promille != 0)
                {
                    double s = (DateTime.Now - _mStartTime).TotalSeconds;
                    double remaining = s / promille * (1000 - promille);
                    TimeSpan t = TimeSpan.FromSeconds(remaining);

                    Console.WriteLine(String.Format("read articles: {0}\u2030\t{1}/{2}\tremaining: {3:00}:{4:00}:{5:00}", promille, index, count, t.Hours, t.Minutes, t.Seconds));
                }
                else
                {
                    Console.WriteLine(String.Format("read articles: {0}\u2030\t{1}/{2}", promille, index, count));
                }
            }
        }

        public static void Main (string[] args)
        {
            try
            {
                MainClass mc = new MainClass();
                mc.CreateWordList(args);       
            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR: " + exc.Message);
            }
        }
    }
}
