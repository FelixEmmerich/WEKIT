﻿using System;
using UnityEngine;
using System.Collections;
using System.IO;

public class AudioPlayer : WekitPlayer<bool,bool>
{
    [Serializable]
    public class AudioData
    {
        public int Channels;
        public float[] Data;
        public int Frequency;

        public AudioData(int channels, float[] data, int frequency)
        {
            Channels = channels;
            Data = data;
            Frequency = frequency;
        }

        public static implicit operator AudioClip(AudioData data)
        {
            AudioClip clip= AudioClip.Create("", data.Data.Length, data.Channels, data.Frequency, false);
            clip.SetData(data.Data, 0);
            return clip;
        }
    }
    //The maximum and minimum available recording frequencies
    private int _minFreq;
    private int _maxFreq;

    public override float Index
    {
        get { return AudioSource != null? AudioSource.time : 0; }
    }

    public override int FrameCount
    {
        get
        {
            return AudioSource != null && AudioSource.clip != null ? (int)(AudioSource.clip.length*10) : 0;
        }
    }

    //A handle to the attached AudioSource
    public AudioSource AudioSource;

    private AudioClip _clip;

    private const int HeaderSize = 44;

    //Standard values
    public override void Reset()
    {
        base.Reset();
        UncompressedFileExtension = "wav";
        CustomDirectory = "Audio";
        PlayerName = "Audio";
    }

    //Use this for initialization
    public override void Start()
    {
        ReplayFps = 0.1f;
        base.Start();
        //Check if there is at least one microphone connected
        if (Microphone.devices.Length <= 0)
        {
            //Throw a warning message at the console if there isn't
            Debug.LogWarning("Microphone not connected!");
        }
        else //At least one microphone is present
        {

            //Get the default microphone recording capabilities
            Microphone.GetDeviceCaps(null, out _minFreq, out _maxFreq);

            //According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...
            if (_minFreq == 0 && _maxFreq == 0)
            {
                //...meaning 44100 Hz can be used as the recording sampling rate
                _maxFreq = 44100;
            }

            //Get the attached AudioSource component
            if (AudioSource == null)
            {
                AudioSource = GetComponent<AudioSource>();
            }
        }
    }

    public override void Update()
    {
        
    }

    public override void Record()
    {
        base.Record();
        if (!Recording&&Microphone.IsRecording(null))
        {
            int lastSample = Microphone.GetPosition(null);
            Microphone.End(null); //Stop the audio recording
            AudioSource.clip = TrimClip(_clip, lastSample);
        }
    }

    public AudioClip TrimClip(AudioClip clip, int lastSample)
    {
        float[] samples = new float[lastSample * clip.channels];
        clip.GetData(samples, 0);
        int start=0;
        AudioClip newClip;
        //Remove silence at the start of the recording
        for (int i = 0; i < samples.Length; i++)
        {
            //3.051758E-05 is an empirically determinded threshold for noise at the start of a recording
            if (Math.Abs(samples[i]) > 3.051758E-05)
            {
                start = i;
                float[] samplesCut = new float[samples.Length - start];
                Array.Copy(samples, start, samplesCut, 0, samples.Length - start);
                newClip = AudioClip.Create(clip.name, lastSample - start, clip.channels, clip.frequency, false);
                newClip.SetData(samplesCut, 0);
                return newClip;
            }
        }
        newClip = AudioClip.Create(clip.name, lastSample-start, clip.channels, clip.frequency, false);
        newClip.SetData(samples, start);
        return newClip;
    }

    /*public override IEnumerator RecordAfterTime(float time)
    {
        if (Recording) yield break;
        Recording = true;
        yield return new WaitForSeconds(time);
        //After countdown, only begin the recording process if it wasn't cancelled
        if (!Recording) yield break;
        Debug.Log("Start recording " + PlayerName);
        Playing = true;
        //Currently the max audio recording time is 30 seconds
        _clip = Microphone.Start(null, true, 30, _maxFreq);

        //Halt EVERYTHING until the microphone is ready. Otherwise the recording may have a noticeable delay
        while (!(Microphone.GetPosition(null) > 0))
        {
        }
    }*/

    public override void SetUpRecording()
    {
        Recording = true;
    }

    public override void InitiateRecording()
    {
        Debug.Log("Start recording " + PlayerName);
        Playing = true;
        //Currently the max audio recording time is 30 seconds
        _clip = Microphone.Start(null, true, 30, _maxFreq);

        //Halt EVERYTHING until the microphone is ready. Otherwise the recording may have a noticeable delay
        while (!(Microphone.GetPosition(null) > 0))
        {
        }
    }

    private IEnumerator LoadWav(string path)
    {
        WWW wav = new WWW("file://" + path);
        while (!wav.isDone) { }
        if (wav.error == null)
        {
            Debug.Log("Loaded " + path);
            AudioSource.clip = wav.audioClip;
            yield return true;
        }
        else
        {
            Debug.Log(wav.error);
            yield return false;
        }
    }

    public override void Replay()
    {
        base.Replay();
        if (Replaying)
        {
            AudioSource.Play();
        }
        else
        {
            AudioSource.Stop();
        }
    }

    public override void Pause()
    {
        base.Pause();
        if (Playing)
        {
            AudioSource.UnPause();
        }
        else
        {
            AudioSource.Pause();
        }
    }

    public override bool Load(bool useZip, string fileName, string entryName)
    {
        if (!useZip)
        {
            Enabled(true);
            IEnumerator routine = LoadWav(SavePath + fileName + "." + UncompressedFileExtension);
            StartCoroutine(routine);
            //Since coroutines are enumerators, we need to get the current value to check for success
            return (bool)routine.Current;
        }
        else
        {
            string filestring = SavePath + fileName + ".zip";
            if (File.Exists(filestring))
            {
                byte[] wavBytes = Compression.GetByteArrayFromCompoundArchive(filestring, fileName + "." + UncompressedFileExtension);
                if (wavBytes != null)
                {
                    AudioSource.clip = GetAudioClipFromWav(wavBytes);
                    return true;
                } 
            }
            return false;
        }
    }

    #region Saving
    public override void Save()
    {
        if (AudioSource!=null&&AudioSource.clip!=null)
        {
            string filepath = SavePath;
            if (!UseZip)
            {
                filepath += FileName + "." + UncompressedFileExtension;

                // Make sure directory exists if user is saving to sub dir.
                Directory.CreateDirectory(SavePath);

                using (var stream = CreateEmpty(filepath))
                {
                    ConvertAndWrite(stream, AudioSource.clip);
                    WriteHeader(stream, AudioSource.clip);
                }
            }
            else
            {
                filepath += (UseCompoundArchive ? CompoundZipName : FileName) + ".zip";
                Directory.CreateDirectory(SavePath);
                using (var stream = new MemoryStream())
                {
                    byte emptyByte = new byte();

                    for (int i = 0; i < HeaderSize; i++) //preparing the header
                    {
                        stream.WriteByte(emptyByte);
                    }

                    ConvertAndWrite(stream, AudioSource.clip);
                    WriteHeader(stream, AudioSource.clip);
                    stream.Position = 0;
                    Compression.AddItemToCompoundArchive(stream,filepath,FileName+"."+UncompressedFileExtension);
                }
            }
        }
    }

    public AudioClip GetAudioClipFromWav(byte[] wav)
    {
        //Get data from wav header
        int channels = BitConverter.ToInt16(wav, 22);
        int frequency = BitConverter.ToInt32(wav, 24);
        float[]samples=new float[(wav.Length-44)/2];

        float rescaleFactor = 32767;
        for (int i = 0; i < (wav.Length-HeaderSize)/2; i++)
        {
            samples[i] = BitConverter.ToInt16(wav, HeaderSize + (i*2))/rescaleFactor;
        }
        return new AudioData(channels,samples,frequency);
    }

    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < HeaderSize; i++) //preparing the header
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }

    //Wave file header
    static void WriteHeader(Stream fileStream, AudioClip clip)
    {
        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        //fileStream.Seek(0, SeekOrigin.Begin);
        fileStream.Position = 0;

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);

        //		fileStream.Close();
    }

    static void ConvertAndWrite(Stream fileStream, AudioClip clip)
    {

        var samples = new float[clip.samples*clip.channels];

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

        Byte[] bytesData = new Byte[samples.Length * 2];
        //bytesData array is twice the size of
        //dataSource array because a float converted in Int16 is 2 bytes.

        float rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }
    #endregion

    public override void SetIndex(float index, bool relative)
    {
        if (AudioSource != null && AudioSource.clip != null)
        {
            AudioSource.time = Mathf.Clamp(relative?AudioSource.clip.length*index:index, 0, AudioSource.clip.length);
        }
    }

    public override object GetListAsObject()
    {
        float[] container=new float[AudioSource.clip.samples*AudioSource.clip.channels];
        AudioSource.clip.GetData(container, 0);
        return new AudioData(AudioSource.clip.channels,container,AudioSource.clip.frequency);
    }

    public override void MakeDataContainerFromObject(object source)
    {
        AudioSource.clip = (AudioData) source;
    }

    public override void ClearFrameList()
    {
    }
}