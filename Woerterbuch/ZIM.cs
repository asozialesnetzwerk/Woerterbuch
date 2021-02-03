using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Woerterbuch
{
    public class ZIM : IDisposable
    {
        private byte[] m_titleData = new byte[1024];
        private byte[] m_articleData = new byte[1024];

        private int m_handle = -1;
        private int m_articleCount = 0;

        private readonly object m_lock = new object();

        public int Count { get { return m_articleCount; } }

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

                m_articleCount = CountItems();
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

        public uint GetTitleSize()
        {
            lock (m_lock)
            {
                AssertValidHandle();

                uint size;

                if (zim_get_title_size(m_handle, out size) == -1)
                    throw new Exception("ZIM: title size");

                return size;
            }
        }

        public uint GetArticleSize()
        {
            lock (m_lock)
            {
                AssertValidHandle();

                uint size;

                if (zim_get_data_size(m_handle, out size) == -1)
                    throw new Exception("ZIM: data size");

                return size;
            }
        }

        public string GetTitle()
        {
            lock (m_lock)
            {
                AssertValidHandle();

                uint size = GetTitleSize();

                if (size > m_titleData.Length)
                    m_titleData = new byte[size * 2];

                if (zim_get_title(m_handle, m_titleData) == -1)
                    throw new Exception("ZIM: title");

                return Encoding.UTF8.GetString(m_titleData, 0, (int)size);
            }
        }

        public string GetArticle()
        {
            lock (m_lock)
            {
                AssertValidHandle();

                uint size = GetArticleSize();

                if (size > m_articleData.Length)
                    m_articleData = new byte[size * 2];

                if (zim_get_data(m_handle, m_articleData) == -1)
                    throw new Exception("ZIM: data");

                return Encoding.UTF8.GetString(m_articleData, 0, (int)size);
            }
        }

        private int CountItems()
        {
            int numItems = 0;

            Init();
            while (!IsEnd())
            {
                numItems++;
                Next();
            }

            return numItems;
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
    }
}

