using Screna;
using Screna.Avi;
using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;



namespace ClientPCMonitoring
{
    public class VideoCapture
    {
        MouseCursor _cursor;
        string _currentFileName=string.Empty;
        
        IRecorder _recorder;
        public Screen captureScreen { get; set; }

        public RecorderState GetRecorderState()
        {
            if (_recorder!=null)
            {
                return _recorder.State;
            }

            return RecorderState.Ready;
        }

        public void StartRecording(string filePath)
        {
            _cursor = new MouseCursor(true);
            _currentFileName = FolderFileUtil.GetFullFilePath(filePath) + ".avi";
            var imgProvider = GetImageProvider();
            var videoEncoder = GetVideoFileWriter(imgProvider);
            
            _recorder = new Recorder(videoEncoder, imgProvider, int.Parse(ConfigurationManager.AppSettings["VideoFrameRate"]), null);


            

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
            encoder.Quality =int.Parse(ConfigurationManager.AppSettings["VideoQuality"]);
            IVideoFileWriter videoEncoder = new AviWriter(_currentFileName, encoder);
            return videoEncoder;
        }

    }
}
