using System;
using System.Collections.Generic;
using System.IO;
using SpellCheck;
using System.Globalization;

namespace Woerterbuch
{
    public class MainClass
    {
        private DateTime m_startTime;

        public List <string> ParseWords(string[] zimFiles)
        {
            WordParser wp = new WordParser(4);
            wp.ProgressEvent += ProgressEventHandler;

            foreach (string zimFile in zimFiles)
            {
                m_promille = -1;
                m_startTime = DateTime.Now;
                wp.Parse(zimFile);
            }

            wp.ProgressEvent -= ProgressEventHandler;

            return wp.WordList;
        }

        public void CreateWordList(string[] zimFiles)
        {
            List <string> wordList = ParseWords(zimFiles);
            Console.WriteLine("num words: " + wordList.Count);

            Console.WriteLine("sort word list ...");
            wordList.Sort();
            SaveWordList(wordList, "word_list_all.txt");

            CreateSpellCheckedWordList(wordList);
            CreateUppercaseSpellCheckedWordList(wordList);

            Console.WriteLine("done");
        }

        private void CreateSpellCheckedWordList(List <string> wordList)
        {
            List <string> wordListSpellChecked = new List<string>();

            Console.WriteLine("create spell checked word list ...");

            using (Hunspell hunspell = new Hunspell("/usr/share/hunspell/de_DE.aff", "/usr/share/hunspell/de_DE.dic"))
            {
                foreach (string word in wordList)
                {
                    if (word.Length < 64)
                    {
                        if (hunspell.SpellCheck(word))
                            wordListSpellChecked.Add(word);
                    }
                }
            }

            Console.WriteLine("num words spell checked: " + wordListSpellChecked.Count);

            SaveWordList(wordListSpellChecked, "word_list_spell_checked.txt");
        }

        private void CreateUppercaseSpellCheckedWordList(List <string> wordList)
        {
            Dictionary <string, int> dictionary = new Dictionary<string, int>();
            CultureInfo ci = new CultureInfo("de-DE", false);

            Console.WriteLine("create uppercase spell checked word list ...");

            foreach (string word in wordList)
            {
                dictionary[word.ToUpper(ci)] = 1;
            }

            List <string> wordListUppercaseSpellChecked = new List<string>();

            using (Hunspell hunspell = new Hunspell("/usr/share/hunspell/de_DE.aff", "/usr/share/hunspell/de_DE.dic"))
            {
                foreach (string word in dictionary.Keys)
                {
                    if (word.Length < 64)
                    {
                        if (hunspell.SpellCheck(word))
                            wordListUppercaseSpellChecked.Add(word);
                    }
                }
            }

            Console.WriteLine("num words uppercase spell checked: " + wordListUppercaseSpellChecked.Count);

            SaveWordList(wordListUppercaseSpellChecked, "word_list_uppercase_spell_checked.txt");
        }

        private void SaveWordList(List <string> list, string fileName)
        {
            using (StreamWriter w = new StreamWriter(fileName))
            {
                foreach (string word in list)
                {
                    w.WriteLine(word);
                }
            }
        }

        private double m_promille;

        public void ProgressEventHandler(int index, int count)
        {
            double promille = Math.Ceiling(1000.0f * (double)index / (double)count);
            if (promille != m_promille)
            {
                m_promille = promille;

                if (promille != 0)
                {
                    double s = (DateTime.Now - m_startTime).TotalSeconds;
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
