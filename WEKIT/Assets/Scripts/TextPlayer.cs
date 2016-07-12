using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

public class TextPlayer : WekitPlayer_Base
{
    [Serializable]
    public class TextElement
    {
        public float StartTime;
        public float EndTime;
        public string Text;
    }

    [Serializable]
    public class TextData
    {
        public float TotalLength;
        public TextElement[] TextElements;
    }

    public override int FrameCount
    {
        get
        {
            return Data != null? (int)(Data.TotalLength) : 0;
        }
    }

    public TextData Data;
    private int _currentTextIndex=-1;
    private IEnumerator _coroutine;
    private float _starttime;


    public void Reset()
    {
        SavePath = Application.streamingAssetsPath + @"/Replays/" + CustomDirectory + "/";
        CustomDirectory = "Text";
        PlayerName = "Text";
    }

    void Start()
    {
        ReplayFps = 0.1f;
    }

    void Update()
    {
        if (Replaying&&Playing)
        {
            SetIndex(Index+Time.time-_starttime,false);
            _starttime = Time.time;
        }
    }

    public override void Replay()
    {
        base.Replay();
        if (Replaying)
        {
            _starttime = Time.time;
        }
    }

    public override void Pause()
    {
        base.Pause();
        if (Playing)
        {
            _starttime = Time.time;
        }
    }

    public override bool Load(bool zip, string fileName, string entryName)
    {
        if (!UseZip)
        {
            string path = SavePath + "/" + CustomDirectory + "/" + LoadFileName + ".txt";
            if (File.Exists(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(WekitGui.XMLData));
                StreamReader reader = new StreamReader(path);
                Data = (TextData)serializer.Deserialize(reader);
                reader.Close();
                return true;
            }
            return false;
        }
        else
        {
            Data = Compression.GetItemFromCompoundArchive<TextData>(SavePath + "/" + CustomDirectory + "/" + (UseCompoundArchive ? CompoundZipName : LoadFileName) + ".zip", LoadFileName + ".txt", new XmlSerializer(typeof(TextData)));
            return Data!=null&&Data.TextElements.Length>0;
        }
    }

    public override void Save()
    {
        if (!UseZip)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TextData));
            FileStream file = File.Open(SavePath + "/" + CustomDirectory + "/" + FileName + ".txt", FileMode.OpenOrCreate);
            serializer.Serialize(file, Data);
            file.Close();
        }
        else
        {
            string filestring = SavePath + "/" + CustomDirectory + "/" + (UseCompoundArchive ? CompoundZipName : FileName) + ".zip";
            Compression.AddItemToCompoundArchive(filestring, FileName + ".txt", Data, new XmlSerializer(typeof(TextData)));
        }
    }

    public override void SetIndex(float index, bool relative)
    {
        Index = relative ? FrameCount * index : index;
        if (Data != null && Data.TextElements.Length>0)
        {
            for (int i = 0; i < Data.TextElements.Length; i++)
            {
                if (Data.TextElements[i].StartTime <= Index && Data.TextElements[i].EndTime > Index)
                {
                    _currentTextIndex = i;
                    return;
                }
            }
        }
        _currentTextIndex = -1;
    }

    public override void Record()
    {
        Replaying = false;
        Recording = !Recording;
        if (!Recording)
        {
            Data.TotalLength = Time.time - _starttime;
        }
    }

    public override void InitiateRecording()
    {
        _starttime = Time.time;
        Recording = true;
    }

    public override void SetUpRecording()
    {
        //Just don't Debug.Log
    }

    private void OnGUI()
    {
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(Screen.width/2f-50, Screen.height*0.75f-10, 100, 20),_currentTextIndex==-1||Data==null||Data.TextElements.Length<=_currentTextIndex?"":Data.TextElements[_currentTextIndex].Text, centeredStyle);
    }
}