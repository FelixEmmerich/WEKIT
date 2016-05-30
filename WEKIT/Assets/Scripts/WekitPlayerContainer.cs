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
    private readonly List<WekitPlayer_Base> _wekitPlayers = new List<WekitPlayer_Base>();

    [HideInInspector]
    public List<WekitPlayer_Base> ActiveWekitPlayers;

    private float _buttonWidth = 100;

    public bool SingleSaveFile=true;

    public bool RecordGUI=true;

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
        _buttonWidth=Screen.width / 6f;
    }

    public override void Update()
    {
        base.Update();
        if (Replaying&&Playing)
        {
            //This ensures the index is updated properly
            GetCurrentFrame();

            //If the replay completes a loop, sync all the players to mitigate desynchronization
            if (PreviousIndex > Index)
            {
                SetIndex(0,false);
            }
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

    public override bool Load(bool useZip, string fileName, string entryName)
    {
        if (!base.Load(useZip,fileName,entryName)) return false;
        int localMaxFrames = 0;

        if (FrameList.Count>0&&FrameList[0].MyName=="SingleSave"&&(bool)FrameList[0].MyObject)
        {
            for (int i = _wekitPlayers.Count - 1; i >= 0; i--)
            {
                WekitPlayer_Base player = _wekitPlayers[i];
                player.ClearFrameList();

                for (int j = FrameList.Count - 1; j >= 1; j--)
                {
                    if (FrameList[j].MyName == player.PlayerName)
                    {
                        player.MakeDataContainerFromObject(FrameList[j].MyObject);
                        //Debug.Log(String.Format("Player: {0} | FrameCount: {1} | ReplayFps: {2}", player.PlayerName, player.FrameCount, player.ReplayFps));

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
            FrameList.Add(new ObjectWithName(true,"SingleSave"));
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
            player.UseZip = UseZip;
            player.UseCompoundArchive = UseCompoundArchive;
            player.CompoundZipName = CompoundZipName;
            player.FileName = FileName;
            player.Save();
        }
    }

    public override void Delete()
    {
        if (!base.Load(UseZip,UseZip&&UseCompoundArchive?CompoundZipName:DeleteFileName,DeleteFileName))
        {
            return;
        }
        if (!(FrameList[0].MyName=="SingleSave"&&(bool)FrameList[0].MyObject))
        {
            foreach (WekitPlayer_Base player in ActiveWekitPlayers)
            {
                player.UseZip = UseZip;
                player.UseCompoundArchive = UseCompoundArchive;
                player.DeleteFileName = DeleteFileName;
                player.CompoundZipName = CompoundZipName;
                player.Delete();
            }
        }
        base.Delete();
    }

    public override void SetIndex(float index, bool relative)
    {
        if (index == Index) return;
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
        float x = Screen.width/2f - _buttonWidth*_wekitPlayers.Count/2;
        for (int i = 0; i < _wekitPlayers.Count; i++)
        {
            bool contained = ActiveWekitPlayers.Contains(_wekitPlayers[i]);

            if (RecordGUI)
            {
                SingleSaveFile = GUI.Toggle(new Rect(0, Screen.height / 20 * 2, Screen.width / 6f, Screen.height / 20f), SingleSaveFile, "Save as 1 file");

                _wekitPlayers[i].Stepsize =(int)GUI.HorizontalSlider(new Rect(x + i*_buttonWidth, Screen.height - (Screen.height / 20f*2), _buttonWidth, Screen.height / 20f), _wekitPlayers[i].Stepsize, 1, 3);

                if (contained)
                {
                    bool focus = GUI.Toggle(new Rect(x + i * _buttonWidth, Screen.height - (Screen.height / 20f*3), _buttonWidth, Screen.height / 20f), _wekitPlayers[i].ForceFocus, "Force focus");
                    if (focus != _wekitPlayers[i].ForceFocus)
                    {
                        _wekitPlayers[i].ForceFocus = focus;
                        _wekitPlayers[i].SetFocus(true);
                    }
                }

            }

            if (!GUI.Button(new Rect(x + i*_buttonWidth, Screen.height - (Screen.height / 20f), _buttonWidth, Screen.height / 20f),
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
    }
}