using System.Collections.Generic;
using System.Text;

namespace Woerterbuch
{
    public class WordInfo
    {
        private string _word;
        private Dictionary<string, int> _nextWords = new Dictionary<string, int>();
        private int _count = 1;

        public WordInfo(string word)
        {
            _word = word;
        }

        public string GetWord() => _word;
        public int GetCount() => _count;
        public Dictionary<string, int> GetNextWords => _nextWords;

        public void AddNextWord(string nextWord)
        {
            AddNextWord(nextWord, 1);
        }

        private void AddNextWord(string nextWord, int count)
        {
            if (_nextWords.ContainsKey(nextWord))
            {
                _nextWords[nextWord] += count;
            }
            else
            {
                _nextWords[nextWord] = count;
            }
        }

        public void IncreaseCount()
        {
            _count += 1;
        }

        public bool Merge(WordInfo wordInfo)
        {
            if (wordInfo._word != GetWord()) return false;
            
            _count += wordInfo.GetCount();
            
            foreach (var keyValPair in wordInfo._nextWords)
            {
                AddNextWord(keyValPair.Key, keyValPair.Value);
            }
            return true;
        }

        public WordInfo CopyUpper()
        {
            WordInfo wordInfoUpper = new WordInfo(GetWord().ToUpper());
            wordInfoUpper._count = _count;
            foreach (var keyValPair in _nextWords)
            {
                wordInfoUpper.AddNextWord(keyValPair.Key, keyValPair.Value);
            }

            return wordInfoUpper;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(_word).Append("; ").Append(_count).Append("; [");
            foreach (var keyValPair in _nextWords)
            {
                stringBuilder.Append("{")
                    .Append(keyValPair.Key)
                    .Append(":")
                    .Append(keyValPair.Value)
                    .Append("}, ");
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }
    }
}