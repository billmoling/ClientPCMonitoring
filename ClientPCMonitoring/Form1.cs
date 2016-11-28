
using Fiddler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientPCMonitoring
{
    public partial class Form1 : Form
    {
        public VideoCapture vc = new VideoCapture();
        public List<Session> oAllSessions = new List<Session>();
        public Form1()
        {
            InitializeComponent();

        }

        public void DoPrep()
        {
            string folderPath = textBox1.Text.Trim();
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            DriveInfo drive = new DriveInfo("C");
            if (drive.IsReady)
            {
                long expectSpace = (long)10 * 1024 * 1024 * 1024;
                long freeSpace=drive.AvailableFreeSpace;
                if (freeSpace < expectSpace)
                {
                    MessageBox.Show("Disk is not enough");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DoPrep();
            label1.Text = "Start Recording";
            vc.StartRecording(textBox1.Text.Trim());
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DoPrep();
            label1.Text = "Stop Recording";
            vc.StopRecording();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DoPrep();
            label4.Text = "Monitoring the traffic, Click 'Stop' to write down the file";
            oAllSessions = new List<Session>();
            string sSAZInfo = "NoSAZ";
            sSAZInfo = Assembly.GetAssembly(typeof(Ionic.Zip.ZipFile)).FullName;
            FiddlerApplication.oSAZProvider = new DNZSAZProvider();

            FiddlerApplication.BeforeRequest += delegate (Session oS)
            {
                oS.bBufferResponse = false;
                Monitor.Enter(oAllSessions);
                //Filter the localhost access request
                if (oS.host.Contains("localhost"))
                {
                    Monitor.Exit(oAllSessions);
                    return;
                }

                oAllSessions.Add(oS);
                Monitor.Exit(oAllSessions);
            };



            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;

            FiddlerApplication.Startup(80, FiddlerCoreStartupFlags.Default);
        }

        private void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            
        }

        private static string Ellipsize(string s, int iLen)
        {
            if (s.Length <= iLen) return s;
            return s.Substring(0, iLen - 3) + "...";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DoPrep();
            if (oAllSessions.Count > 0)
            {
                SaveSessionsToDesktop(oAllSessions,textBox1.Text.Trim());
            }
            else
            {
                WriteCommandResponse("No sessions have been captured");
            }

            Monitor.Enter(oAllSessions);
            oAllSessions.Clear();
            Monitor.Exit(oAllSessions);
            FiddlerApplication.Shutdown();
        }

        public static void WriteCommandResponse(string s)
        {
            MessageBox.Show(s);
        }

        private static void SaveSessionsToDesktop(List<Session> oAllSessions,string filePath)
        {
            bool bSuccess = false;
            string sFilename = Path.Combine(filePath, DateTime.Now.ToString("hh-mm-ss") + ".saz");
            try
            {
                try
                {
                    Monitor.Enter(oAllSessions);

                    bSuccess = Utilities.WriteSessionArchive(sFilename, oAllSessions.ToArray(), null, false);
                }
                finally
                {
                    Monitor.Exit(oAllSessions);
                }

                WriteCommandResponse(bSuccess ? ("Wrote: " + sFilename) : ("Failed to save: " + sFilename));
            }
            catch (Exception eX)
            {
                MessageBox.Show("Save failed: " + eX.Message);
                
            }
        }

        private void RunPsPing(string filePath)
        {
            //Create process
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();

            //strCommand is path and file name of command to run
            pProcess.StartInfo.FileName =Path.Combine(filePath, "psping.exe");

            //strCommandParameters are parameters to pass to program
            pProcess.StartInfo.Arguments = "-b -l 1500 -n 10 www.sina.com.cn -nobanner";

            pProcess.StartInfo.UseShellExecute = false;

            //Set output of program to be written to process output stream
            pProcess.StartInfo.RedirectStandardOutput = true;

            //Optional
            pProcess.StartInfo.WorkingDirectory = filePath;

            //Start the process
            pProcess.Start();

            //Get program output
            string strOutput = pProcess.StandardOutput.ReadToEnd();



            //Wait for process to finish
            pProcess.WaitForExit();


            string path = Path.Combine(filePath, "Pspinglog.txt");
            if (!File.Exists(path))
            {
                FileInfo txtFile=new FileInfo(path);
                FileStream fs = txtFile.Create();
                fs.Close();
            }

            StreamWriter sw = File.AppendText(path);
            sw.WriteLine(DateTime.Now.ToString());
            sw.WriteLine(strOutput);
            sw.Flush();
            sw.Close();


           

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DoPrep();
            RunPsPing(textBox1.Text.Trim());
        }
    }
}
