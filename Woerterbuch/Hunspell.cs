using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SpellCheck
{
    public class Hunspell : IDisposable
    {
        private IntPtr _mPHunspell = IntPtr.Zero;

        public Hunspell(string affpath, string dpath)
        {
            _mPHunspell = Hunspell_create(affpath, dpath);
            if (_mPHunspell == IntPtr.Zero)
                throw new Exception("Can not open libhunspell");
        }

        public void Dispose()
        {
            if (_mPHunspell != IntPtr.Zero)
            {
                Hunspell_destroy(_mPHunspell);
                _mPHunspell = IntPtr.Zero;
            }
        }

        private Encoding _mEncoding = Encoding.GetEncoding("ISO-8859-1");
        private byte[] _mByteArr = new byte[256];

        public bool SpellCheck(string word)
        {
            _mEncoding.GetBytes(word, 0, word.Length, _mByteArr, 0);
            _mByteArr[word.Length] = 0;

            return Hunspell_spell(_mPHunspell, _mByteArr) != 0;
        }

        [DllImport("libhunspell.so")]
        private static extern IntPtr Hunspell_create([MarshalAs(UnmanagedType.LPStr)] string affpath, [MarshalAs(UnmanagedType.LPStr)] string dpath);

        [DllImport("libhunspell.so")]
        private static extern int Hunspell_spell(IntPtr pHunspell, byte[] word);

        [DllImport("libhunspell.so")]
        private static extern void Hunspell_destroy(IntPtr pHunspell);
    }
}

