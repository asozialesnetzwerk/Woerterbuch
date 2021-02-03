using System;
using Gtk;
using System.Threading;
using Utils;
using System.Collections.Generic;
using WoerterbuchGUI;
using System.Globalization;
using SpellCheck;
using System.IO;

public partial class MainWindow: Gtk.Window
{
	public enum EState { Idle, Running }

	private EState m_state = EState.Idle;
	private Thread m_guiThread;
	private volatile Thread m_runThread;
    private volatile bool m_requestStop = false;
    private ListStore m_treeViewFileList;

    private List<string> m_zimFileList = new List<string>();
    private string m_affFile;
    private string m_dicFile;
    private string m_fileWordList;
    private string m_fileWordListSpellChecked;
    private string m_fileWordListUppercaseSpellChecked;

    private DateTime m_startTime;

    private WordParser m_wordParser;

    public MainWindow () : base (Gtk.WindowType.Toplevel)
    {
        Build();

		m_guiThread = Thread.CurrentThread;

        TreeViewColumn columnFiles = new TreeViewColumn();
        CellRendererText cellFiles = new CellRendererText();

        columnFiles.Title = "Files";
        columnFiles.PackStart(cellFiles, true);
        columnFiles.AddAttribute(cellFiles, "text", 0);

        treeViewZimFiles.AppendColumn(columnFiles);

        m_treeViewFileList = new ListStore(typeof(string));
        treeViewZimFiles.Model = m_treeViewFileList;
            
		UpdateWindowState();

        progressbar1.Adjustment.Lower = 0;
        progressbar1.Adjustment.Upper = 1000;

        m_treeViewFileList.AppendValues("/home/torsten/Source/Woerterbuch/wikipedia_de_all_nopic_2015-11.zim");
        m_zimFileList.Add("/home/torsten/Source/Woerterbuch/wikipedia_de_all_nopic_2015-11.zim");
    }

    protected void OnDeleteEvent (object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    protected void ClearZimFileList (object sender, EventArgs e)
    {
        m_zimFileList.Clear();
        m_treeViewFileList.Clear();
    }

    protected void AddZimFiles (object sender, EventArgs e)
    {
        FileChooserDialog fileChooserDlg = new FileChooserDialog ("Open ZIM file", null, FileChooserAction.Open);

        // add buttons you wish to see in the dialog
        fileChooserDlg.AddButton(Stock.Cancel, ResponseType.Cancel);
        fileChooserDlg.AddButton(Stock.Open, ResponseType.Ok);

        // then create a filter for files
        fileChooserDlg.Filter = new FileFilter();
        fileChooserDlg.Filter.AddPattern("*.zim");

        // if you wish to select multiple files set the do the following:
        fileChooserDlg.SelectMultiple = true;

        // run the dialog
        ResponseType retVal = (ResponseType)fileChooserDlg.Run();

        // handle the dialog's exit value
        if (retVal == ResponseType.Ok)
        {
            foreach (string file in fileChooserDlg.Filenames)
            {
                m_treeViewFileList.AppendValues(file);
                m_zimFileList.Add(file);
            }
        }

        // destroy the dialog
        fileChooserDlg.Destroy();
    }

	protected void Start(object sender, EventArgs e)
	{
        if (m_runThread != null)
        {
            m_requestStop = true;

            if (m_wordParser != null)
                m_wordParser.Stop();
        }
        else
        {
            progressbar1.Adjustment.Value = 0;
            m_requestStop = false;

            m_affFile = entryAffFile.Text;
            m_dicFile = entryDicFile.Text;
            m_fileWordList = checkbuttonAllWords.Active ? entryAllWords.Text : null;
            m_fileWordListSpellChecked = checkbuttonSpellChecked.Active ? entrySpellChecked.Text : null;
            m_fileWordListUppercaseSpellChecked = checkbuttonUppercase.Active ? entryUppercase.Text : null;

            m_runThread = new Thread(Run);
            m_runThread.Start();
        }
	}

	private void SetNewState(EState newState)
	{
		m_state = newState;
		UpdateWindowState();
	}

	private void UpdateWindowState()
	{
		if (Thread.CurrentThread != m_guiThread)
		{
			Gtk.Application.Invoke(delegate
				{
					UpdateWindowState();
				});
			
			return;
		}

		switch (m_state)
		{
            case EState.Idle:
                buttonStart.Label = "Start";
				buttonAddZimFile.Sensitive = true;
				buttonClearZimFiles.Sensitive = true;
				buttonAffFile.Sensitive = true;
				buttonDicFile.Sensitive = true;
				treeViewZimFiles.Sensitive = true;
				entryAffFile.Sensitive = true;
				entryDicFile.Sensitive = true;
				entryAllWords.Sensitive = true;
				entrySpellChecked.Sensitive = true;
				entryUppercase.Sensitive = true;
				checkbuttonAllWords.Sensitive = true;
				checkbuttonSpellChecked.Sensitive = true;
				checkbuttonUppercase.Sensitive = true;
				break;

			case EState.Running:
                buttonStart.Label = "Stop";
				buttonAddZimFile.Sensitive = false;
				buttonClearZimFiles.Sensitive = false;
				buttonAffFile.Sensitive = false;
				buttonDicFile.Sensitive = false;
				treeViewZimFiles.Sensitive = false;
				entryAffFile.Sensitive = false;
				entryDicFile.Sensitive = false;
				entryAllWords.Sensitive = false;
				entrySpellChecked.Sensitive = false;
				entryUppercase.Sensitive = false;
				checkbuttonAllWords.Sensitive = false;
				checkbuttonSpellChecked.Sensitive = false;
				checkbuttonUppercase.Sensitive = false;
				break;
		}
	}

	private void Run()
	{
		SetNewState(EState.Running);

		try
		{
            List <string> wordList = ParseWords(m_zimFileList);
            wordList.Sort();

            CreateWordList(wordList);
            CreateWordListSpellChecked(wordList);
            CreateWordListUppercaseSpellChecked(wordList);
		}
		catch (Exception exc)
		{
			Gtk.Application.Invoke(delegate
				{
                    MessageBox.Show(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, exc.Message, "ERROR");
				});
		}

		SetNewState(EState.Idle);
		m_runThread = null;
	}

    private List <string> ParseWords(List <string> zimFiles)
    {
        m_wordParser = new WordParser(Environment.ProcessorCount);
        m_wordParser.ProgressEvent += ProgressEventHandler;

        foreach (string zimFile in zimFiles)
        {
            m_promille = -1;
            m_startTime = DateTime.Now;
            m_wordParser.Parse(zimFile);

            if (m_requestStop)
                break;
        }

        m_wordParser.ProgressEvent -= ProgressEventHandler;

        return m_wordParser.WordList;
    }

    private double m_promille;

    private void ProgressEventHandler(int index, int count)
    {
        double promille = Math.Ceiling(1000.0f * (double)index / (double)count);
        if ((promille != m_promille) || (index >= count - 1))
        {
            m_promille = promille;

            string str;

            if (promille != 0)
            {
                double s = (DateTime.Now - m_startTime).TotalSeconds;
                double remaining = s / promille * (1000 - promille);
                TimeSpan t = TimeSpan.FromSeconds(remaining);

                str = String.Format("{0}/{1} remaining: {2:00}:{3:00}:{4:00}", index, count, t.Hours, t.Minutes, t.Seconds);
            }
            else
            {
                str = String.Format("{0}/{1}", index, count);
            }

            Gtk.Application.Invoke(delegate
                {
                    progressbar1.Text = str;
                    progressbar1.Adjustment.Value = m_promille;
                });
        }
    }

    public void CreateWordList(List <string> wordList)
    {
        if (m_fileWordList != null)
        {
            SaveWordList(wordList, m_fileWordList);
        }
    }

    private void CreateWordListSpellChecked(List <string> wordList)
    {
        if (m_fileWordListSpellChecked != null)
        {
            List <string> wordListSpellChecked = new List<string>();

            using (Hunspell hunspell = new Hunspell(m_affFile, m_dicFile))
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

            SaveWordList(wordListSpellChecked, m_fileWordListSpellChecked);
        }
    }

    private void CreateWordListUppercaseSpellChecked(List <string> wordList)
    {
        if (m_fileWordListUppercaseSpellChecked != null)
        {
            Dictionary <string, int> dictionary = new Dictionary<string, int>();
            CultureInfo ci = new CultureInfo("de-DE", false);

            foreach (string word in wordList)
            {
                dictionary[word.ToUpper(ci)] = 1;
            }

            List <string> wordListUppercaseSpellChecked = new List<string>();

            using (Hunspell hunspell = new Hunspell(m_affFile, m_dicFile))
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

            SaveWordList(wordListUppercaseSpellChecked, m_fileWordListUppercaseSpellChecked);
        }
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

    private void Test()
    {
        List <string> articleList1 = new List<string>();

        DateTime t;
        int idx;

        t = DateTime.Now;
        using (ZIM zim = new ZIM(m_zimFileList[0]))
        {
            for (idx = 0; idx < 5000; idx++)
            {
                zim.GoToArticle(idx);
                articleList1.Add(zim.GetArticle());
            }
        }
        Console.WriteLine("time 1 = " + (DateTime.Now - t).TotalMilliseconds + " ms");

        WordParserThread wpt = new WordParserThread(null);

        t = DateTime.Now;
        for (idx = 0; idx < articleList1.Count; idx++)
        {
            wpt.ParseArticle(articleList1[idx]);
        }
        Console.WriteLine("time 2 = " + (DateTime.Now - t).TotalMilliseconds + " ms");

        List <string> articleList2 = new List<string>();
        List <string> articleList3 = new List<string>();
        List <string> articleList4 = new List<string>();

        foreach (string s in articleList1)
        {
            articleList2.Add((string)s.Clone());
            articleList3.Add((string)s.Clone());
            articleList4.Add((string)s.Clone());
        }

        TestWorker w1 = new TestWorker(articleList1);
        TestWorker w2 = new TestWorker(articleList2);
        TestWorker w3 = new TestWorker(articleList3);
        TestWorker w4 = new TestWorker(articleList4);
        Thread t1 = new Thread(w1.Run);
        Thread t2 = new Thread(w2.Run);
        Thread t3 = new Thread(w3.Run);
        Thread t4 = new Thread(w4.Run);

        t = DateTime.Now;
        t1.Start();
        t2.Start();
        t3.Start();
        t4.Start();

        t1.Join();
        t2.Join();
        t3.Join();
        t4.Join();
        Console.WriteLine("time 3 = " + (DateTime.Now - t).TotalMilliseconds + " ms");
    }

    private class TestWorker
    {
        private List <string> m_articleList;
        private WordParserThread m_wpt = new WordParserThread(null);

        public TestWorker(List <string> articleList)
        {
            m_articleList = articleList;
        }

        public void Run()
        {
            int idx;
            for (idx = 0; idx < m_articleList.Count; idx++)
            {
                m_wpt.ParseArticle(m_articleList[idx]);
            }
        }
    }
}

