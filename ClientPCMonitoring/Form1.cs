
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
        

        private void button2_Click(object sender, EventArgs e)
        {
            
            label1.Text = "Start Recording";
            vc.StartRecording();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label1.Text = "Stop Recording";
            vc.StopRecording();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            oAllSessions = new List<Session>();
            string sSAZInfo = "NoSAZ";
            sSAZInfo = Assembly.GetAssembly(typeof(Ionic.Zip.ZipFile)).FullName;
            FiddlerApplication.oSAZProvider = new DNZSAZProvider();

            FiddlerApplication.BeforeRequest += delegate (Session oS)
            {
                oS.bBufferResponse = false;
                Monitor.Enter(oAllSessions);

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

            if (oAllSessions.Count > 0)
            {
                SaveSessionsToDesktop(oAllSessions);
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

        private static void SaveSessionsToDesktop(List<Session> oAllSessions)
        {
            bool bSuccess = false;
            string sFilename = Path.Combine("C:\\ClientMonitoring", DateTime.Now.ToString("hh-mm-ss") + ".saz");
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
    }
}
