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
    private float _starttime;


    public void Reset()
    {
        CustomDirectory = "Text";
        PlayerName = "Text";
        SavePath = Application.streamingAssetsPath + @"/Replays/" + CustomDirectory + "/";
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
            string path = SavePath + "/" + fileName + ".txt";
            if (File.Exists(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TextData));
                StreamReader reader = new StreamReader(path);
                Data = (TextData)serializer.Deserialize(reader);
                reader.Close();
                return true;
            }
            return false;
        }
        else
        {
            Data = Compression.GetItemFromCompoundArchive<TextData>(SavePath + "/" + fileName + ".zip", entryName + ".txt", new XmlSerializer(typeof(TextData)));
            return Data!=null&&Data.TextElements.Length>0;
        }
    }

    public override void Save()
    {
        Directory.CreateDirectory(SavePath);

        if (!UseZip)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TextData));
            FileStream file = File.Open(SavePath + "/" + FileName + ".txt", FileMode.OpenOrCreate);
            serializer.Serialize(file, Data);
            file.Close();
        }
        else
        {
            string filestring = SavePath + "/" + (UseCompoundArchive ? CompoundZipName : FileName) + ".zip";
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

    public override object GetListAsObject()
    {
        return Data;
    }

    public override void MakeDataContainerFromObject(object source)
    {
        Data = (TextData) source;
    }

    public override void OnGUI()
    {
        base.OnGUI();
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(Screen.width/2f-50, Screen.height*0.75f-10, 100, 20),_currentTextIndex==-1||Data==null||Data.TextElements.Length<=_currentTextIndex?"":Data.TextElements[_currentTextIndex].Text, centeredStyle);
    }

    public override void CustomGUI()
    {
        if (Data != null && Data.TotalLength <= 0)
        {
            GUI.Label(new Rect(Screen.width/2f, Screen.height/2f, 50, 20), "Record or load a replay");
        }
        else
        {
            
        }
    }
}