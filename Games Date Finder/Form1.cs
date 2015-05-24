using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Games_Date_Finder
{
    public partial class Form1 : Form
    {
        string m_Machine;
        string m_GameName;
        int m_Renombrats = 0;

        List<String> m_FilePaths = new List<string>();


        int m_ContWeb = 0;
        List<String> m_GameNamesWeb = new List<string>();
        string m_FilePathWeb;
        List<String> m_FilePathsWeb = new List<string>();



        //Constructor
        public Form1()
        {
            InitializeComponent();
        }

        #region  --> Form Events
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == String.Empty || textBox1.Text == " ")
                comboBox1.Enabled = true;
            else
                comboBox1.Enabled = false;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var hl = webBrowser1.Document;
            textBox2.Text = hl.Body.InnerText;




            String date = SearchOnWeb(hl.Body.InnerText);

            if (String.IsNullOrEmpty(date) == false)
            {
                string extension = Path.GetExtension(m_FilePathWeb);
                String newfilepath = Path.GetDirectoryName(m_FilePathWeb) + "\\" + date + " - " + m_GameName + extension;
                File.Move(m_FilePathWeb, newfilepath);
                m_Renombrats++;
            }

            m_ContWeb++;

            if (m_ContWeb < m_GameNamesWeb.Count)
            {
                m_GameName = m_GameNamesWeb[m_ContWeb];
                m_FilePathWeb = m_FilePathsWeb[m_ContWeb];
                String url = String.Format("http://en.wikipedia.org/w/index.php?search={0}+{1}+release+date&fulltext=Search", m_GameName, m_Machine);
                webBrowser1.Navigate(url); //anar al event navigated
            }

            progressBar1.Value = m_ContWeb;

        }
        private void Form1_Load(object sender, EventArgs e)
        {

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);

            comboBox1.SelectedIndex = 0;
            //webBrowser1.Navigate("http://www.google.es/search?hl=es&sa=N&tab=lw&q=space%20invaders%20atari2600%20release%20date");




        }
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy; // Okay
            else
                e.Effect = DragDropEffects.None; // Unknown data, ignore it
        }
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {

            string[] tempFileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            //Escollim maquina
            if (textBox1.Text == "" || textBox1.Text == " ")
                m_Machine = comboBox1.Text;
            else
                m_Machine = textBox1.Text;



            if (tempFileList.Length < 1) return;


            //Ens pategem totes les rutes de el que ens an arrastrat
            foreach (string path in tempFileList)
            {
                //SI es un directori
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    m_FilePaths.AddRange(files);
                }

                //Si es un ficher
                if (File.Exists(path))
                    m_FilePaths.Add(path);
            }


            progressBar1.Maximum = m_FilePaths.Count;
            progressBar1.Value = 0;

            //Renombrem fichers
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += (o, c) =>
            {
                foreach (String fileptah in m_FilePaths)
                {
                    TryRename(fileptah);
                    bw.ReportProgress(0);
                }
            };
            bw.ProgressChanged += (o, c) =>
            {
                progressBar1.Value++;
            };
            bw.RunWorkerCompleted += (o, c) =>
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = m_GameNamesWeb.Count;
                m_GameName = m_GameNamesWeb[0];
                m_FilePathWeb = m_FilePathsWeb[0];
                String url = String.Format("http://en.wikipedia.org/w/index.php?search={0}+{1}+release+date&fulltext=Search", m_GameName, m_Machine);
                webBrowser1.Navigate(url); //anar al event navigated

            };

            bw.RunWorkerAsync();

        }
        #endregion


        #region --> Private Functions
        private bool TryRename(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            m_GameName = Path.GetFileNameWithoutExtension(filePath);
            m_GameName = m_GameName.ToLower();
            //Purificacio
            m_GameName = m_GameName.Replace("(ue)", "");
            m_GameName = m_GameName.Replace("(jue)", "");
            m_GameName = m_GameName.Replace("(jp)", "");
            m_GameName = m_GameName.Replace("(usa)", "");
            m_GameName = m_GameName.Replace("(us)", "");
            m_GameName = m_GameName.Replace("(ue)", "");
            m_GameName = m_GameName.Replace("(u)", "");
            m_GameName = m_GameName.Replace("(j)", "");
            m_GameName = m_GameName.Replace("(e)", "");
            m_GameName = m_GameName.Replace("(ue)", "");
            m_GameName = m_GameName.Replace("[jue]", "");
            m_GameName = m_GameName.Replace("[jp]", "");
            m_GameName = m_GameName.Replace("[usa]", "");
            m_GameName = m_GameName.Replace("[us]", "");
            m_GameName = m_GameName.Replace("[ue]", "");
            m_GameName = m_GameName.Replace("[u]", "");
            m_GameName = m_GameName.Replace("[j]", "");
            m_GameName = m_GameName.Replace("[e]", "");
            m_GameName = m_GameName.Replace(".", "");
            m_GameName = m_GameName.Replace("_", " ");
            m_GameName = m_GameName.Replace("[", "");
            m_GameName = m_GameName.Replace("]", "");
            m_GameName = m_GameName.Replace("(", "");
            m_GameName = m_GameName.Replace(")", "");
            m_GameName = m_GameName.Replace("!", "");
            m_GameName = m_GameName.Replace("jp", "");
            m_GameName = m_GameName.Replace(" usa ", "");
            m_GameName = m_GameName.Replace(" eu ", " ");

            m_GameName = m_GameName.Replace(" us ", " ");


            m_GameName = m_GameName.Replace(" j ", "");
            m_GameName = m_GameName.Replace(" gc ", "");
            m_GameName = m_GameName.Replace(" e ", "");
           
            //Comprobem si ja comença per la data
            if (m_GameName.StartsWith("19"))
            {
                if (Char.IsDigit(m_GameName[2]) && Char.IsDigit(m_GameName[3]))
                    return false;
            }

            //Si conte la data pero no esta al proncipi ho reordenm
            if (m_GameName.Contains("197") || m_GameName.Contains("198"))
            {
                for (int i = 0; i < m_GameName.Length; i++)
                {
                    char c = m_GameName[i];

                    if (i == m_GameName.Length - 3)
                    {
                        m_GameNamesWeb.Add(m_GameName);
                        m_FilePathsWeb.Add(filePath);
                        return false;
                    }
                    if (Char.IsDigit(c))
                    {
                        char[] str = new char[4];

                        str[0] = m_GameName[i];
                        str[1] = m_GameName[i + 1];
                        str[2] = m_GameName[i + 2];
                        str[3] = m_GameName[i + 3];

                        if (str[0] != '1') continue;
                        if (str[1] != '9') continue;
                        if (Char.IsDigit(str[2]) == false) continue;
                        if (Char.IsDigit(str[3]) == false) continue;


                        if (i < 0)
                            if (Char.IsDigit(m_GameName[i - 1]) == true) continue;

                        if (i + 4 < m_GameName.Length)
                            if (Char.IsDigit(m_GameName[i + 4]) == true) continue;


                        String any = new String(str);
                        String newName = Path.GetDirectoryName(filePath) + "\\" + any + " - " + m_GameName + extension;
                        File.Move(filePath, newName);
                        m_Renombrats++;
                        return true;
                    }
                }
            }

            m_GameNamesWeb.Add(m_GameName);
            m_FilePathsWeb.Add(filePath);
            return false;

        }

        #endregion


        public string SearchOnWeb(string webContent)
        {
            try
            {
                string str = webContent;
                var dictionary = new Dictionary<string, int>();


                //Busquem la data
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];

                    if (i == str.Length - 3) break;
                    if (Char.IsDigit(c))
                    {
                        char[] strArray = new char[4];

                        strArray[0] = str[i];
                        strArray[1] = str[i + 1];
                        strArray[2] = str[i + 2];
                        strArray[3] = str[i + 3];

                        if (strArray[0] != '1') continue;
                        if (strArray[1] != '9') continue;
                        if (strArray[2] != '7' && strArray[2] != '8' && strArray[2] != '9') continue;
                        if (Char.IsDigit(strArray[3]) == false) continue;


                        if (i < 0)
                            if (Char.IsDigit(str[i - 1]) == true) continue;

                        if (i + 4 < strArray.Length)
                            if (Char.IsDigit(str[i + 4]) == true) continue;

                        //Afegim el resultat

                        String data = new String(strArray);
                        if (dictionary.ContainsKey(data) == false)
                        {
                            dictionary.Add(data, 1);
                        }
                        else
                        {
                            dictionary[data]++;
                        }
                    }
                }

                string newData = String.Empty;
                int maxCount = 0;
                //Contem qui te mes
                foreach (var kv in dictionary)
                {
                    if (kv.Value > maxCount)
                    {
                        maxCount = kv.Value;
                        newData = kv.Key;
                    }

                }

                return newData;
            }
            catch
            {
                return null;
            }
        }
    }
}
