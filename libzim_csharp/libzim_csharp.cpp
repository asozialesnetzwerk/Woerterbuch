#include <string.h>

#include <zim/file.h>
#include <zim/fileiterator.h>



//-----------------------------------------------------------------------------

#define MAX_FILES     128

//-----------------------------------------------------------------------------

using namespace std;

//-----------------------------------------------------------------------------

class ZimFileHandle {
public:
    void Open(const std::string &fileName) {
        Close();
        m_zimFile.reset(new zim::File(fileName));
    }

    void Close() {
        m_zimFile.reset();
    }

    bool IsOpen() {
        return m_zimFile != 0;
    }

    void Init() {
        AssertIsOpen();
        m_zimIterator = m_zimFile->begin();
        GetEntry();
    }

    void Next() {
        AssertIsOpen();
        ++m_zimIterator;
        GetEntry();
    }

    bool IsEnd() {
        AssertIsOpen();
        return (m_zimIterator == m_zimFile->end());
    }

    unsigned int GetTitleSize() {
        AssertIsOpen();
        return m_title.size();
    }

    unsigned int GetDataSize() {
        AssertIsOpen();
        return m_data.size();
    }

    void GetTitle(void *buffer) {
        memcpy(buffer, m_title.data(), m_title.size());
    }

    void GetData(void *buffer) {
        memcpy(buffer, m_data.data(), m_data.size());
    }

    unsigned int GetArticleCount() {
        AssertIsOpen();
        return (unsigned int) m_zimFile->getCountArticles();
    }

    void GetArticle(unsigned int idx) {
        AssertIsOpen();

        zim::Article article = m_zimFile->getArticle(idx);
        m_title = article.getTitle();
        m_data = article.getData();
    }

private:
    shared_ptr <zim::File> m_zimFile; 
    zim::File::const_iterator m_zimIterator;

    std::string m_title;
    zim::Blob m_data;

    void AssertIsOpen() {
        if (!m_zimFile)
            throw runtime_error("not open");
    }

    void GetEntry() {
        if (m_zimIterator != m_zimFile->end()) {
            m_title = m_zimIterator->getTitle();
            m_data = m_zimIterator->getData();
        }
    }
};

//-----------------------------------------------------------------------------

static std::vector <ZimFileHandle> g_zimFileList(MAX_FILES);

//-----------------------------------------------------------------------------

extern "C" int zim_open(const char *zimFileName, int *handle) {
    *handle = -1;

    try {
        int h;

        for (h = 0; h < MAX_FILES; h++) {
            if (!g_zimFileList.at(h).IsOpen()) {
                g_zimFileList.at(h).Open(zimFileName);
                *handle = h;
                return 0;
            }
        }
    }
    catch (exception &exc) {
        return -1;
    }

    return -1;
}

//-----------------------------------------------------------------------------

extern "C" int zim_close(int handle) {
    try {
        g_zimFileList.at(handle).Close();
    }
    catch (exception &exc) {
        return -1;
    }

    return 0;
}

//-----------------------------------------------------------------------------

extern "C" int zim_init(int handle) {
    try {
        g_zimFileList.at(handle).Init();
    }
    catch (exception &exc) {
        return -1;
    }

    return 0;
}

//-----------------------------------------------------------------------------

extern "C" int zim_next(int handle) {
    try {
        g_zimFileList.at(handle).Next();
    }
    catch (exception &exc) {
        return -1;
    }

    return 0;
}

//-----------------------------------------------------------------------------

extern "C" int zim_is_end(int handle, unsigned int *is_end) {
    try {
        *is_end = g_zimFileList.at(handle).IsEnd() ? 1 : 0;
    }
    catch (exception &exc) {
        return -1;
    }

    return 0;
}

//-----------------------------------------------------------------------------

extern "C" int zim_get_title_size(int handle, unsigned int *size) {
    try {
        *size = g_zimFileList.at(handle).GetTitleSize();
    }
    catch (exception &exc) {
        return -1;
    }

    return 0;
}

//-----------------------------------------------------------------------------

extern "C" int zim_get_data_size(int handle, unsigned int *size) {
    try {
        *size = g_zimFileList.at(handle).GetDataSize();
    }
    catch (exception &exc) {
        return -1;
    }

    return 0;
}

//-----------------------------------------------------------------------------

extern "C" int zim_get_title(int handle, void *buffer) {
    try {
        g_zimFileList.at(handle).GetTitle(buffer);
    }
    catch (exception &exc) {
        return -1;
    }

    return 0;
}

//-----------------------------------------------------------------------------

extern "C" int zim_get_data(int handle, void *buffer) {
    try {
        g_zimFileList.at(handle).GetData(buffer);
    }
    catch (exception &exc) {
        return -1;
    }

    return 0;
}

//-----------------------------------------------------------------------------

extern "C" int zim_get_article_count(int handle, unsigned int *count) {
    try {
        *count = g_zimFileList.at(handle).GetArticleCount();
    }
    catch (exception &exc) {
        return -1;
    }

    return 0;
}

//-----------------------------------------------------------------------------

extern "C" int zim_get_article(int handle, unsigned int idx) {
    try {
        g_zimFileList.at(handle).GetArticle(idx);
    }
    catch (exception &exc) {
        return -1;
    }

    return 0;
}

//-----------------------------------------------------------------------------
