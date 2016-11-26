
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientPCMonitoring
{
    public partial class Form1 : Form
    {
        public VideoCapture vc = new VideoCapture();
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
    }
}
