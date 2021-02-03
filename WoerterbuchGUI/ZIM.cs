using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WoerterbuchGUI
{
    public class ZIM : IDisposable
    {
        private int m_handle = -1;
        private readonly object m_lock = new object();

        public ZIM()
        {
        }

        public ZIM(string fileName)
        {
            Open(fileName);
        }

        public void Open(string fileName)
        {
            Close();

            lock (m_lock)
            {
                if (zim_open(fileName, out m_handle) == -1)
                    throw new Exception("ZIM: can not open file: " + fileName);
            }
        }

        public void Close()
        {
            lock (m_lock)
            {
                if (m_handle != -1)
                {
                    zim_close(m_handle);
                    m_handle = -1;
                }
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void Init()
        {
            lock (m_lock)
            {
                AssertValidHandle();

                if (zim_init(m_handle) == -1)
                    throw new Exception("ZIM: init");
            }
        }

        public void Next()
        {
            lock (m_lock)
            {
                AssertValidHandle();

                if (zim_next(m_handle) == -1)
                    throw new Exception("ZIM: next");
            }
        }

        public bool IsEnd()
        {
            lock (m_lock)
            {
                AssertValidHandle();

                uint isEnd;

                if (zim_is_end(m_handle, out isEnd) == -1)
                    throw new Exception("ZIM: is end");

                return isEnd != 0;
            }
        }

        public int GetArticleCount()
        {
            lock (m_lock)
            {
                AssertValidHandle();

                uint count;

                if (zim_get_article_count(m_handle, out count) == -1)
                    throw new Exception("ZIM: article count");

                return (int)count;
            }
        }

        public void GoToArticle(int idx)
        {
            lock (m_lock)
            {
                if (zim_get_article(m_handle, (uint)idx) == -1)
                    throw new Exception("ZIM: article");
            }
        }

        public string GetTitle()
        {
            byte[] titleBuf;

            lock (m_lock)
            {
                AssertValidHandle();

                titleBuf = new byte[GetTitleSize()];

                if (titleBuf.Length > 0)
                {
                    if (zim_get_title(m_handle, titleBuf) == -1)
                        throw new Exception("ZIM: title");
                }
            }

            return Encoding.UTF8.GetString(titleBuf);
        }

        public string GetArticle()
        {
            byte[] articleBuf;

            lock (m_lock)
            {
                AssertValidHandle();

                articleBuf = new byte[GetArticleSize()];

                if (articleBuf.Length > 0)
                {
                    if (zim_get_data(m_handle, articleBuf) == -1)
                        throw new Exception("ZIM: data");
                }
            }

            return Encoding.UTF8.GetString(articleBuf);
        }

        private uint GetTitleSize()
        {
            uint size;

            if (zim_get_title_size(m_handle, out size) == -1)
                throw new Exception("ZIM: title size");

            return size;
        }

        private uint GetArticleSize()
        {
            uint size;

            if (zim_get_data_size(m_handle, out size) == -1)
                throw new Exception("ZIM: data size");

            return size;
        }

        private void AssertValidHandle()
        {
            if (m_handle == -1)
                throw new Exception("invalid handle");
        }

        // extern "C" int zim_open(const char *zimFileName, int *handle)
        [DllImport("libzim_csharp.so.1.0")]
        private static extern int zim_open([MarshalAs(UnmanagedType.LPStr)] string zimFileName, out int handle);

        // extern "C" int zim_close(int handle)
        [DllImport("libzim_csharp.so.1.0")]
        private static extern int zim_close(int handle);

        // extern "C" int zim_init(int handle)
        [DllImport("libzim_csharp.so.1.0")]
        private static extern int zim_init(int handle);

        // extern "C" int zim_next(int handle)
        [DllImport("libzim_csharp.so.1.0")]
        private static extern int zim_next(int handle);

        // extern "C" int zim_is_end(int handle, unsigned int *is_end)
        [DllImport("libzim_csharp.so.1.0")]
        private static extern int zim_is_end(int handle, out uint is_end);

        // extern "C" int zim_get_title_size(int handle, unsigned int *size)
        [DllImport("libzim_csharp.so.1.0")]
        private static extern int zim_get_title_size(int handle, out uint size);

        // extern "C" int zim_get_data_size(int handle, unsigned int *size)
        [DllImport("libzim_csharp.so.1.0")]
        private static extern int zim_get_data_size(int handle, out uint size);

        // extern "C" int zim_get_title(int handle, void *buffer)
        [DllImport("libzim_csharp.so.1.0")]
        public static extern int zim_get_title(int handle, byte[] title);

        // extern "C" int zim_get_data(int handle, void *buffer)
        [DllImport("libzim_csharp.so.1.0")]
        public static extern int zim_get_data(int handle, byte[] data);

        // extern "C" int zim_get_article_count(int handle, unsigned int *count)
        [DllImport("libzim_csharp.so.1.0")]
        public static extern int zim_get_article_count(int handle, out uint count);

        // extern "C" int zim_get_article(int handle, unsigned int idx)
        [DllImport("libzim_csharp.so.1.0")]
        public static extern int zim_get_article(int handle, uint idx);

    }
}

