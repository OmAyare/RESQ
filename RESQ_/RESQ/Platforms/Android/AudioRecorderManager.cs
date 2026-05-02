using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Media;
using Application = Android.App.Application;
using Environment = Android.OS.Environment;

namespace RESQ.Platforms.Android
{
    public class AudioRecorderManager
    {
        private MediaRecorder _recorder;
        private string _filePath;

        public void Start()
        {
            var RecDir = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryRecordings);
            //var musicDir = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryMusic);

            var resqDir = new Java.IO.File(RecDir, "RESQ");
            if (!resqDir.Exists())
                resqDir.Mkdirs();

            var dir = Application.Context.GetExternalFilesDir(null);

            var fileName =
                $"resq_audio_{DateTime.Now:yyyyMMdd_HHmmss}.m4a";

            _filePath = System.IO.Path.Combine(resqDir.AbsolutePath, fileName);

            _recorder = new MediaRecorder();
            _recorder.SetAudioSource(AudioSource.Mic);
            _recorder.SetOutputFormat(OutputFormat.Mpeg4);
            _recorder.SetAudioEncoder(AudioEncoder.Aac);
            _recorder.SetAudioEncodingBitRate(128000);
            _recorder.SetAudioSamplingRate(44100);
            _recorder.SetOutputFile(_filePath);

            _recorder.Prepare();
            _recorder.Start();
        }

        public void Stop()
        {
            try { _recorder?.Stop(); } catch { }
            _recorder?.Release();
            _recorder = null;
        }

        public string FilePath => _filePath;
    }
}

