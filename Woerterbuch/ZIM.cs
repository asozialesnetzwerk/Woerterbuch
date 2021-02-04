using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Woerterbuch
{
    public class Zim : IDisposable
    {
        private readonly object _mLock = new object();
        private byte[] _mArticleData = new byte[1024];

        private int _mHandle = -1;
        private byte[] _mTitleData = new byte[1024];

        public Zim()
        {
        }

        public Zim(string fileName)
        {
            Open(fileName);
        }

        public int Count { get; private set; }

        public void Dispose()
        {
            Close();
        }

        public void Open(string fileName)
        {
            Close();

            lock (_mLock)
            {
                if (zim_open(fileName, out _mHandle) == -1)
                    throw new Exception("ZIM: can not open file: " + fileName);

                Count = CountItems();
            }
        }

        public void Close()
        {
            lock (_mLock)
            {
                if (_mHandle != -1)
                {
                    zim_close(_mHandle);
                    _mHandle = -1;
                }
            }
        }

        public void Init()
        {
            lock (_mLock)
            {
                AssertValidHandle();
                if (zim_init(_mHandle) == -1)
                    throw new Exception("ZIM: init");
            }
        }

        public void Next()
        {
            lock (_mLock)
            {
                AssertValidHandle();
                if (zim_next(_mHandle) == -1)
                    throw new Exception("ZIM: next");
            }
        }

        public bool IsEnd()
        {
            lock (_mLock)
            {
                AssertValidHandle();

                uint isEnd;

                if (zim_is_end(_mHandle, out isEnd) == -1)
                    throw new Exception("ZIM: is end");

                return isEnd != 0;
            }
        }

        public uint GetTitleSize()
        {
            lock (_mLock)
            {
                AssertValidHandle();

                uint size;

                if (zim_get_title_size(_mHandle, out size) == -1)
                    throw new Exception("ZIM: title size");

                return size;
            }
        }

        public uint GetArticleSize()
        {
            lock (_mLock)
            {
                AssertValidHandle();

                uint size;

                if (zim_get_data_size(_mHandle, out size) == -1)
                    throw new Exception("ZIM: data size");

                return size;
            }
        }

        public string GetTitle()
        {
            lock (_mLock)
            {
                AssertValidHandle();

                var size = GetTitleSize();

                if (size > _mTitleData.Length)
                    _mTitleData = new byte[size * 2];

                if (zim_get_title(_mHandle, _mTitleData) == -1)
                    throw new Exception("ZIM: title");

                return Encoding.UTF8.GetString(_mTitleData, 0, (int) size);
            }
        }

        public string GetArticle()
        {
            lock (_mLock)
            {
                AssertValidHandle();

                var size = GetArticleSize();

                if (size > _mArticleData.Length)
                    _mArticleData = new byte[size * 2];

                if (zim_get_data(_mHandle, _mArticleData) == -1)
                    throw new Exception("ZIM: data");

                return Encoding.UTF8.GetString(_mArticleData, 0, (int) size);
            }
        }

        private int CountItems()
        {
            var numItems = 0;

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
            if (_mHandle == -1)
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
        private static extern int zim_is_end(int handle, out uint isEnd);

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