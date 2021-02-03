using System;

namespace Utils
{
    public static class Letters
    {
        public static bool IsGermanLetter(char c)
        {
            if ((c >= 'A') && (c <= 'Z'))
                return true;

            if ((c >= 'a') && (c <= 'z'))
                return true;

            switch (c)
            {
                case 'ä':
                case 'ö':
                case 'ü':
                case 'Ä':
                case 'Ö':
                case 'Ü':
                case 'ß':
                    return true;
            }

            return false;
        }

        public static bool IsEnglishLetter(char c)
        {
            if ((c >= 'A') && (c <= 'Z'))
                return true;

            if ((c >= 'a') && (c <= 'z'))
                return true;

            return false;
        }
    }
}

