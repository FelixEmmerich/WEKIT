using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

//Class for managing multiple players. Type bool is used to keep the data small.
public class WekitPlayerContainer : WekitPlayer<WekitPlayerContainer.ObjectWithName, bool>
{

    [Serializable]
    public class ObjectWithName
    {
        public object MyObject;
        public string MyName;

        public ObjectWithName(object myObject, string myName)
        {
            MyObject = myObject;
            MyName = myName;
        }

        public ObjectWithName()
        {
            MyObject = null;
            MyName = "";
        }
    }

    [Serializable]
    public class XMLData
    {
        public XMLFileInfo[] Files;

        public XMLData(XMLFileInfo fileInfo)
        {
            Files = new XMLFileInfo[1];
            Files[0] = fileInfo;
        }

        public XMLData(XMLFileInfo[] fileInfo)
        {
            Files = fileInfo;
        }

        public XMLData()
        {
            Files = new XMLFileInfo[0];
        }
    }

    [Serializable]
    public class XMLFileInfo
    {
        public string FileName;
        /// <summary>
        /// Name of the entry if data is saved in a zipfile
        /// </summary>
        public string EntryName;
        public bool Zip;

        public XMLFileInfo(string fileName, string entryName, bool zip)
        {
            FileName = fileName;
            EntryName = entryName;
            Zip = zip;
        }

        public XMLFileInfo()
        {
            FileName = "";
            EntryName = "";
            Zip = false;
        }
    }

    public XMLData XmlData;
    private int _xmlDataIndex;

    [SerializeField]
    private List<WekitPlayer_Base> _wekitPlayers = new List<WekitPlayer_Base>();

    [HideInInspector]
    public List<WekitPlayer_Base> ActiveWekitPlayers;

    public float ButtonWidth = 100;

    public bool SingleSaveFile=true;

    public override void Reset()
    {
        base.Reset();
        UncompressedFileExtension = "WEKITData";
        PlayerName = "Container";
        Provider = true;
    }

    private float _speed;
    public override float Speed
    {
        get { return _speed; }
        set
        {
            if (value == _speed) return;
            _speed = value;
            foreach (WekitPlayer_Base player in ActiveWekitPlayers)
            {
                player.Speed = value;
            }
        }
    }

    public override void Start()
    {
        base.Start();
        ActiveWekitPlayers = new List<WekitPlayer_Base>(_wekitPlayers);
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {

            player.CountDown = CountDown;
        }
    }

    public override void Update()
    {
        base.Update();
        if (Replaying)
        {
            //This ensures the index is updated properly
            GetCurrentFrame();
        }
    }

    public override ObjectWithName AddFrame()
    {
        return new ObjectWithName();
    }

    public override void Replay()
    {
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            player.Replaying = Replaying;
            player.Speed = Speed;
            player.Replay();
        }
        base.Replay();
    }

    public override void Record()
    {
        base.Record();
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            player.CountDown = CountDown;
            //player.Stepsize = Stepsize;
            player.CurrentStep = CurrentStep;
            player.Record();
        }
    }

    public override void Pause()
    {
        base.Pause();
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            player.Pause();
        }
    }

    public override bool Load()
    {
        if (!base.Load()) return false;
        if (!UseZip)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XMLData));
            StreamReader reader = new StreamReader(SavePath + "/" + CustomDirectory + "/" + LoadFileName + ".txt");
            XmlData = (XMLData)serializer.Deserialize(reader);
            reader.Close();
        }
        else
        {
            XmlData = Compression.GetItemFromCompoundArchive<XMLData>(SavePath + "/" + CustomDirectory + "/" + (UseCompoundArchive?CompoundZipName:LoadFileName) + ".zip", LoadFileName + ".txt", new XmlSerializer(typeof(XMLData)));
        }
        _xmlDataIndex = 0;
        return true;
        /*if (!UseZip)
        {
            XmlData =
                JsonUtility.FromJson<XMLData>(
                    File.ReadAllText(SavePath + "/" + CustomDirectory + "/" + LoadFileName + ".txt"));
        }
        else
        {
            XmlData = Compression.GetItemFromCompoundArchive<XMLData>(SavePath + "/" + CustomDirectory + "/"+(UseCompoundArchive?CompoundZipName:LoadFileName)+".zip", LoadFileName+".txt", new XmlSerializer(typeof(XMLData)));
        }
        _jsonDataIndex = 0;
        int localMaxFrames = 0;
        if (SingleSaveFile)
        {
            for (int i = _wekitPlayers.Count - 1; i >= 0; i--)
            {
                WekitPlayer_Base player = _wekitPlayers[i];
                for (int j = FrameList.Count - 1; j >= 0; j--)
                {
                    if (FrameList[j].MyName == player.PlayerName)
                    {
                        player.MakeDataContainerFromObject(FrameList[j].MyObject);
                        if (player.FrameCount > localMaxFrames)
                        {
                            localMaxFrames = player.FrameCount;
                            ReplayFps = player.ReplayFps;
                        }
                        break;
                    }
                }
                if (player.FrameCount == 0)
                {
                    player.Enabled(false);
                    player.ClearFrameList();
                    ActiveWekitPlayers.Remove(player);
                }
            } 
        }
        else
        {
            for (int i = _wekitPlayers.Count - 1; i >= 0; i--)
            {
                WekitPlayer_Base player = _wekitPlayers[i];
                player.UseZip = UseZip;
                player.UseCompoundArchive = UseCompoundArchive;
                player.LoadFileName = LoadFileName;
                if (!player.Load())
                {
                    player.Enabled(false);
                    player.ClearFrameList();
                    ActiveWekitPlayers.Remove(player);
                }
                else
                {
                    if (player.FrameCount > localMaxFrames)
                    {
                        localMaxFrames = player.FrameCount;
                        ReplayFps = player.ReplayFps;
                    }
                }
            } 
        }
        if (localMaxFrames == 0) return false;
        FrameList = new List<ObjectWithName>(new ObjectWithName[localMaxFrames + 1]);
        return true;*/
    }

    public override bool Load(bool useZip, string fileName, string entryName)
    {
        if (!base.Load(useZip,fileName,entryName)) return false;
        int localMaxFrames = 0;
        if (SingleSaveFile)
        {
            for (int i = _wekitPlayers.Count - 1; i >= 0; i--)
            {
                WekitPlayer_Base player = _wekitPlayers[i];
                player.ClearFrameList();

                for (int j = FrameList.Count - 1; j >= 0; j--)
                {
                    if (FrameList[j].MyName == player.PlayerName)
                    {
                        player.MakeDataContainerFromObject(FrameList[j].MyObject);
                        if (player.FrameCount > localMaxFrames)
                        {
                            localMaxFrames = player.FrameCount;
                            ReplayFps = player.ReplayFps;
                        }
                        break;
                    }
                }
                if (player.FrameCount == 0)
                {
                    player.Enabled(false);
                    ActiveWekitPlayers.Remove(player);
                }
                else
                {
                    player.Enabled(true);
                    if (!ActiveWekitPlayers.Contains(player))
                    {
                        ActiveWekitPlayers.Add(player);
                    }
                }
            }
        }
        else
        {
            for (int i = _wekitPlayers.Count - 1; i >= 0; i--)
            {
                WekitPlayer_Base player = _wekitPlayers[i];
                player.ClearFrameList();
                /*player.UseZip = UseZip;
                player.UseCompoundArchive = UseCompoundArchive;
                player.LoadFileName = LoadFileName;*/
                if (!player.Load(useZip,fileName,entryName))
                {
                    player.Enabled(false);
                    ActiveWekitPlayers.Remove(player);
                }
                else
                {
                    player.Enabled(true);
                    if (!ActiveWekitPlayers.Contains(player))
                    {
                        ActiveWekitPlayers.Add(player);
                    }

                    if (player.FrameCount > localMaxFrames)
                    {
                        localMaxFrames = player.FrameCount;
                        ReplayFps = player.ReplayFps;
                    }
                }
            }
        }
        if (localMaxFrames == 0) return false;
        FrameList = new List<ObjectWithName>(new ObjectWithName[localMaxFrames + 1]);
        return true;
    }

    public override void Save()
    {
        //List is copied so we can replace it with a list more suited for saving, then reinstate the old one
        var listcopy = new List<ObjectWithName>(FrameList);
        FrameList.Clear();
        if (SingleSaveFile)
        {
            foreach (WekitPlayer_Base player in ActiveWekitPlayers)
            {
                FrameList.Add(new ObjectWithName(player.GetListAsObject(), player.PlayerName));
            }
        }

        base.Save();

        XMLData data = new XMLData(new XMLFileInfo(UseCompoundArchive ? CompoundZipName : FileName, FileName, UseZip));

        if (!UseZip)
        {
            XmlSerializer serializer=new XmlSerializer(typeof(XMLData));
            FileStream file = File.Open(SavePath + "/" + CustomDirectory + "/" + FileName + ".txt", FileMode.OpenOrCreate);
            serializer.Serialize(file, data);
            file.Close();
        }
        else
        {
            string filestring = SavePath + "/" + CustomDirectory + "/" + (UseCompoundArchive?CompoundZipName:FileName) + ".zip";
            Compression.AddItemToCompoundArchive(filestring, FileName + ".txt", ref data, new XmlSerializer(typeof(XMLData)));
        }

        FrameList = listcopy;
        if (SingleSaveFile) return;
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            player.UseZip = UseZip;
            player.UseCompoundArchive = UseCompoundArchive;
            player.CompoundZipName = CompoundZipName;
            player.FileName = FileName;
            player.Save();
        }
    }

    public override void Delete()
    {
        base.Delete();
        if (SingleSaveFile) return;
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            player.UseZip = UseZip;
            player.UseCompoundArchive = UseCompoundArchive;
            player.DeleteFileName = DeleteFileName;
            player.CompoundZipName = CompoundZipName;
            player.Delete();
        }
    }

    public override void SetIndex(float index, bool relative)
    {
        if ((int) index == (int) Index) return;
        base.SetIndex(index, relative);
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            player.SetIndex(index / FrameCount, true);
        }
    }
    
    //Buttons to activate/deactivate players
    void OnGUI()
    {
        if (Recording) return;
        SingleSaveFile = GUI.Toggle(new Rect(10,30, ButtonWidth, 20), SingleSaveFile, "Save as single file");
        float x = Screen.width/2f - ButtonWidth*_wekitPlayers.Count/2;
        for (int i = 0; i < _wekitPlayers.Count; i++)
        {
            _wekitPlayers[i].Stepsize =(int)GUI.HorizontalSlider(new Rect(x + i*ButtonWidth, Screen.height - 40, ButtonWidth, 20), _wekitPlayers[i].Stepsize, 1, 3);
            bool contained = ActiveWekitPlayers.Contains(_wekitPlayers[i]);
            if (contained)
            {
                bool focus = GUI.Toggle(new Rect(x + i * ButtonWidth, Screen.height - 60, ButtonWidth, 20), _wekitPlayers[i].ForceFocus, "Force focus");
                if (focus != _wekitPlayers[i].ForceFocus)
                {
                    _wekitPlayers[i].ForceFocus = focus;
                    _wekitPlayers[i].SetFocus(true);
                }
            }
            if (!GUI.Button(new Rect(x + i*ButtonWidth, Screen.height - 20, ButtonWidth, 20),
                _wekitPlayers[i].PlayerName + (contained ? " active" : " inactive"))) continue;

            //(De)activate button pressed
            if (contained)
            {
                _wekitPlayers[i].Enabled(false);
                ActiveWekitPlayers.Remove(_wekitPlayers[i]);
                Debug.Log("Removed " + _wekitPlayers[i].PlayerName + " from List");
            }
            else
            {
                _wekitPlayers[i].Playing = Playing;
                _wekitPlayers[i].Recording = Recording;
                _wekitPlayers[i].Replaying = Replaying;
                _wekitPlayers[i].Speed = Speed;
                _wekitPlayers[i].Index = Index;
                _wekitPlayers[i].Enabled(true);
                ActiveWekitPlayers.Add(_wekitPlayers[i]);
                Debug.Log("Added " + _wekitPlayers[i] + " to List");
            }
        }
        //Multi-replay handling
        if (XmlData == null) return;
        //Previous replay
        if (_xmlDataIndex>0)
        {
            if (GUI.Button(new Rect(0, Screen.height/2f, Screen.width/10f, Screen.height/5f), "Previous"))
            {
                _xmlDataIndex--;
                XMLFileInfo data = XmlData.Files[_xmlDataIndex];
                Load(data.Zip, data.FileName, data.EntryName);
                SetIndex(0,false);
            } 
        }
        //Next replay
        if (_xmlDataIndex<XmlData.Files.Length-1)
        {
            if (GUI.Button(new Rect(Screen.width*0.9f, Screen.height/2f, Screen.width/10f, Screen.height/5f), "Next"))
            {
                _xmlDataIndex++;
                XMLFileInfo data = XmlData.Files[_xmlDataIndex];
                Load(data.Zip, data.FileName, data.EntryName);
                SetIndex(0, false);
            } 
        }
    }
}