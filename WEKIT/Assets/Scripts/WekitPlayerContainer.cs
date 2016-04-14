using System;
using UnityEngine;
using System.Collections.Generic;

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
                player.Zip = Zip;
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
        FrameList = listcopy;
        if (SingleSaveFile) return;
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            player.Zip = Zip;
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
            player.Zip = Zip;
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
        SingleSaveFile = GUI.Toggle(new Rect((Screen.width - ButtonWidth) / 2f, Screen.height - 60, ButtonWidth, 20), SingleSaveFile, "Save as single file");
        float x = Screen.width/2f - ButtonWidth*_wekitPlayers.Count/2;
        for (int i = 0; i < _wekitPlayers.Count; i++)
        {
            _wekitPlayers[i].Stepsize =(int)GUI.HorizontalSlider(new Rect(x + i*ButtonWidth, Screen.height - 40, ButtonWidth, 20), _wekitPlayers[i].Stepsize, 1, 3);
            bool contained = ActiveWekitPlayers.Contains(_wekitPlayers[i]);
            if (!GUI.Button(new Rect(x + i*ButtonWidth, Screen.height - 20, ButtonWidth, 20),
                _wekitPlayers[i].PlayerName + (contained ? " active" : " inactive"))) continue;

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
    }
}