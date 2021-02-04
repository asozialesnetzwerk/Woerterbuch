using System.Collections.Generic;
using System.Text;

namespace Woerterbuch
{
    public class WordInfo
    {
        private int _count = 1;
        private readonly string _word;

        public WordInfo(string word)
        {
            _word = word;
        }

        public Dictionary<string, int> GetNextWords { get; } = new Dictionary<string, int>();

        public string GetWord()
        {
            return _word;
        }

        public int GetCount()
        {
            return _count;
        }

        public void AddNextWord(string nextWord)
        {
            AddNextWord(nextWord, 1);
        }

        private void AddNextWord(string nextWord, int count)
        {
            if (GetNextWords.ContainsKey(nextWord))
                GetNextWords[nextWord] += count;
            else
                GetNextWords[nextWord] = count;
        }

        public void IncreaseCount()
        {
            _count += 1;
        }

        public bool Merge(WordInfo wordInfo)
        {
            if (wordInfo._word != GetWord()) return false;

            _count += wordInfo.GetCount();

            foreach (var keyValPair in wordInfo.GetNextWords) AddNextWord(keyValPair.Key, keyValPair.Value);
            return true;
        }

        public WordInfo CopyUpper()
        {
            var wordInfoUpper = new WordInfo(GetWord().ToUpper());
            wordInfoUpper._count = _count;
            foreach (var keyValPair in GetNextWords) wordInfoUpper.AddNextWord(keyValPair.Key, keyValPair.Value);

            return wordInfoUpper;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(_word).Append("; ").Append(_count).Append("; [");
            foreach (var keyValPair in GetNextWords)
                stringBuilder.Append("{")
                    .Append(keyValPair.Key)
                    .Append(":")
                    .Append(keyValPair.Value)
                    .Append("}, ");

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }
    }
}