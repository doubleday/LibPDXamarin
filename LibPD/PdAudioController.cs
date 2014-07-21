using System;
using MonoTouch.AVFoundation;
using MonoTouch.Foundation;
using MonoTouch.AudioToolbox;

namespace LibPDBinding
{
    public class PdAudioController : IDisposable
    {
        PdAudioUnit _audioUnit;

        bool _disposed;
        bool _active;

        double _sampleRate;
        bool _inputEnabled;
        bool _mixingEnabled;

        public PdAudioController ()
        {
            _audioUnit = new PdAudioUnit ();

            // AudioSession
            var globalSession = AVAudioSession.SharedInstance ();
            globalSession.BeginInterruption += AVSession_BeginInterruption;
            globalSession.EndInterruption += AVSession_EndInterruption;
        }

        public bool Active {
            get { return _active; }
            set { 
                _audioUnit.Active = value;
                _active = _audioUnit.Active;
            }
        }

        void AVSession_BeginInterruption(object sender, EventArgs e)
        {
            _audioUnit.Active = false;
        }

        void AVSession_EndInterruption(object sender, EventArgs e)
        {
            Active = _active;
        }

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return; 

            if (disposing) {
                var globalSession = AVAudioSession.SharedInstance ();
                globalSession.BeginInterruption -= AVSession_BeginInterruption;
                globalSession.EndInterruption -= AVSession_EndInterruption;

                _audioUnit.Dispose ();
                _audioUnit = null;
            }

            _disposed = true;
        }

        #endregion

    }
}

