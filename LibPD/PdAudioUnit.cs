using System;
using MonoTouch.AudioUnit;
using MonoTouch.AudioToolbox;

namespace LibPDBinding
{
    public class PdAudioUnit : IDisposable
    {
        const double SampleRate = 44100;
        AudioUnit _audioUnit;
        bool _inputEnabled;
        bool _active;
        bool _disposed;

        static AudioStreamBasicDescription StreamDescription {
            get {
                const int kFloatSize = 4;
                const int kBitSize = 8;

                var asd = new AudioStreamBasicDescription ();
                asd.SampleRate = SampleRate;
                asd.Format = AudioFormatType.LinearPCM;
                asd.FormatFlags = AudioFormatFlags.IsFloat | AudioFormatFlags.IsPacked;
                asd.BytesPerPacket = kFloatSize * 2;
                asd.FramesPerPacket = 1;
                asd.BytesPerFrame = kFloatSize * 2;
                asd.ChannelsPerFrame = 2;
                asd.BitsPerChannel = kFloatSize * kBitSize;

                return asd;
            }
        }

        public bool Active {
            get { return _active; }
            set {
                if (_active == value)
                    return;

                if (value)
                    _audioUnit.Start ();
                else
                    _audioUnit.Stop ();

                _active = value;
            }
        }

        public PdAudioUnit ()
        {
            _audioUnit = InitializeAudioUnit ();
            _audioUnit.SetRenderCallback (AudioRenderCallback, AudioUnitScopeType.Input, 0);
            _audioUnit.Initialize ();

            LibPD.OpenAudio (0, 2, (int)SampleRate);
            LibPD.ComputeAudio (true);
        }

        AudioUnit InitializeAudioUnit()
        {
            
            var acd = AudioComponentDescription.CreateOutput (AudioTypeOutput.Remote);
            var audioComponent = AudioComponent.FindComponent (acd);

            var audioUnit = new AudioUnit (audioComponent);
            var asd = StreamDescription;

            audioUnit.SetAudioFormat (asd, AudioUnitScopeType.Input, 0);
            return audioUnit;   
        }

        AudioUnitStatus AudioRenderCallbackTest(AudioUnitRenderActionFlags actionFlags, AudioTimeStamp timeStamp, uint busNumber, uint numberFrames, AudioBuffers data)
        {
            var deltaT = 1f / 44100;
            var f = 440f;

            IntPtr outL = data [0].Data;

//            timeStamp.
            unsafe {
                var outLPtr = (float*)outL.ToPointer ();

                for (int i = 0; i < numberFrames; i++) {
                    
                    var val = Math.Sin (2 * Math.PI * (timeStamp.SampleTime + i) * deltaT * f);
                    *outLPtr++ = (float)val;
                    *outLPtr++ = (float)val;
                }
            }

            return AudioUnitStatus.OK;
        }

        AudioUnitStatus AudioRenderCallback(AudioUnitRenderActionFlags actionFlags, AudioTimeStamp timeStamp, 
                                            uint busNumber, uint numberFrames, AudioBuffers data)
        {
            if (_inputEnabled) {
                _audioUnit.Render (ref actionFlags, timeStamp, busNumber, numberFrames, data);
            }

            IntPtr dataPtr = data [0].Data;
            var ticks = numberFrames / 64;

            unsafe {
                var buf = (float*)dataPtr.ToPointer ();
                LibPD.Process ((int)ticks, buf, buf);                
            }

            return AudioUnitStatus.NoError;
        }

        float[] OutBuf = new float[128];
        float[] InBuf = new float[128];

        AudioUnitStatus AudioRenderCallbackRaw(AudioUnitRenderActionFlags actionFlags, AudioTimeStamp timeStamp, 
                                               uint busNumber, uint numberFrames, AudioBuffers data)
        {
            uint ticks = numberFrames / 64;
            IntPtr outData = data [0].Data;   

            unsafe {
                var outPtr = (float*)outData.ToPointer ();
                for (int i = 0; i < ticks; i++) {
                    LibPD.ProcessRaw (InBuf, OutBuf);
                    for (int j = 0; j < 64; j++) {
                        *outPtr++ = OutBuf [j];
                        *outPtr++ = OutBuf [j + 64];
                    }
                }    
            }           

            return AudioUnitStatus.NoError;
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
                _audioUnit.Dispose (disposing);
            }

            _disposed = true;
        }

        #endregion
    }
}

