
using Fiddler;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace ClientPCMonitoring
{
    public partial class Form1 : Form
    {
        public VideoCapture vc = new VideoCapture();
        public List<Session> oAllSessions = new List<Session>();
        System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
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
            StartFiddlerCapturing();
        }

        private void StartFiddlerCapturing()
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
            FinishFiddlerCapturing();
        }

        private void FinishFiddlerCapturing()
        {
            DoPrep();
            if (oAllSessions.Count > 0)
            {
                SaveSessionsToDesktop(oAllSessions, textBox1.Text.Trim());
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

            //MessageBox.Show(s);
        }

        private static void SaveSessionsToDesktop(List<Session> oAllSessions,string filePath)
        {
            bool bSuccess = false;
            string sFilename =FolderFileUtil.GetFullFilePath(filePath) + ".saz";
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
            CheckPsPingProgram();


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


            string path =FolderFileUtil.GetFullFilePath(filePath)+"-Pspinglog.txt";
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

        private void CheckPsPingProgram()
        {

            string pspingPath = Path.Combine(textBox1.Text.Trim(), "psping.exe");
            if (!File.Exists(pspingPath))
            {
                File.Copy(@".\psping.exe", pspingPath);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DoPrep();
            RunPsPing(textBox1.Text.Trim());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //strat to tick every 30 minutes.


            _timer.Interval = 30 * 60 * 1000;
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Enabled = true;
            
            //first time run
            vc.StartRecording(textBox1.Text.Trim());
            StartFiddlerCapturing();

            RunPsPing(textBox1.Text.Trim());


        }

        void timer_Tick(object sender, EventArgs e)
        {
            //Check if any Capture is working
            //End the existing

            DoPrep();
           

            if (vc.GetRecorderState().Equals(Screna.RecorderState.Recording))
            {
                vc.StopRecording();
            }

            if (FiddlerApplication.IsStarted())
            {
                FinishFiddlerCapturing();
            }
            
           
            //Start all the capture function.

            vc.StartRecording(textBox1.Text.Trim());
            StartFiddlerCapturing();

            RunPsPing(textBox1.Text.Trim());



        }

        private void button7_Click(object sender, EventArgs e)
        {
            //End all the capture function
            vc.StopRecording();
            FinishFiddlerCapturing();

            _timer.Stop();
            _timer.Tick -= timer_Tick;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer.Stop();
            _timer.Tick -= timer_Tick;
        }

        void InstalledApplication()
        {
            string path = FolderFileUtil.GetFullFilePath(textBox1.Text.Trim()) + "-InstalledApplication.txt";
            if (!File.Exists(path))
            {
                FileInfo txtFile = new FileInfo(path);
                FileStream fs = txtFile.Create();
                fs.Close();
            }

            StreamWriter sw = File.AppendText(path);
            sw.WriteLine(DateTime.Now.ToString());

            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
            {
                foreach (string subkey_name in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        string displayName = subkey.GetValue("DisplayName")?.ToString()??"";
                        string displayVersion = subkey.GetValue("DisplayVersion")?.ToString()??"";
                        string installDate = subkey.GetValue("InstallDate")?.ToString() ?? "";
                        sw.WriteLine(string.Format("{0},{1},{2}", displayName,displayVersion,installDate));
                        

                        
                    }
                }
            }
            sw.Flush();
            sw.Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            InstalledApplication();
        }
    }
}
