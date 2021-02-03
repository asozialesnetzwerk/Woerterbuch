using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SpellCheck
{
    public class Hunspell : IDisposable
    {
        private IntPtr m_pHunspell = IntPtr.Zero;

        public Hunspell(string affpath, string dpath)
        {
            m_pHunspell = Hunspell_create(affpath, dpath);
            if (m_pHunspell == IntPtr.Zero)
                throw new Exception("Can not open libhunspell");
        }

        public void Dispose()
        {
            if (m_pHunspell != IntPtr.Zero)
            {
                Hunspell_destroy(m_pHunspell);
                m_pHunspell = IntPtr.Zero;
            }
        }

        private Encoding m_encoding = Encoding.GetEncoding("ISO-8859-1");
        private byte[] m_byteArr = new byte[256];

        public bool SpellCheck(string word)
        {
            m_encoding.GetBytes(word, 0, word.Length, m_byteArr, 0);
            m_byteArr[word.Length] = 0;

            return Hunspell_spell(m_pHunspell, m_byteArr) != 0;
        }

        [DllImport("libhunspell.so")]
        private static extern IntPtr Hunspell_create([MarshalAs(UnmanagedType.LPStr)] string affpath, [MarshalAs(UnmanagedType.LPStr)] string dpath);

        [DllImport("libhunspell.so")]
        private static extern int Hunspell_spell(IntPtr pHunspell, byte[] word);

        [DllImport("libhunspell.so")]
        private static extern void Hunspell_destroy(IntPtr pHunspell);
    }
}

