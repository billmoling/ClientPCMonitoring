using Screna;
using Screna.Avi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Timers.Timer;



namespace ClientPCMonitoring
{
    public class VideoCapture
    {
        MouseCursor _cursor;
        string _currentFileName=string.Empty;
        
        IRecorder _recorder;
        public Screen captureScreen { get; set; }
        public void StartRecording(string filePath)
        {
            _cursor = new MouseCursor(true);
            _currentFileName = Path.Combine(filePath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".avi");
            var imgProvider = GetImageProvider();
            var videoEncoder = GetVideoFileWriter(imgProvider);
            
            _recorder = new Recorder(videoEncoder, imgProvider, 5, null);


            _recorder.RecordingStopped += (s, E) =>
              {
                  OnStopped();
                  if (E?.Error == null)
                      return;
              };

            _recorder.Start(0);
        }


        void OnStopped()
        {
            _recorder = null;
        }

        public void StopRecording()
        {
            _recorder.Stop();
            OnStopped();
        }
        IImageProvider GetImageProvider()
        {
            var mouseKeyHook = new MouseKeyHook(true,true);

            captureScreen = Screen.AllScreens[0];

            return new ScreenProvider(captureScreen, _cursor, mouseKeyHook);
        }

        IVideoFileWriter GetVideoFileWriter(object imgProvider)
        {
             
            var encoder = AviCodec.MotionJpeg;
            encoder.Quality = 20;
            IVideoFileWriter videoEncoder = new AviWriter(_currentFileName, encoder);
            return videoEncoder;
        }
    }
}
