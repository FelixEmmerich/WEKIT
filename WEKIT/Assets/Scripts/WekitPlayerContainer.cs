using System;
using UnityEngine;
using System.Collections.Generic;

//Class for managing multiple players. Type bool is used to keep the data small.
public class WekitPlayerContainer : WekitPlayer<WekitPlayerContainer.Content, bool>
{
    [Serializable]
    public class Content
    {
        public int Framecount;
        public List<ObjectWithName> Lists;

        public Content()
        {
            Framecount = 0;
            Lists = null;
        }

        public Content(int framecount, List<ObjectWithName> lists)
        {
            Framecount = framecount;
            Lists = lists;
        }
    }

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
    }


    [SerializeField] private List<WekitPlayer_Base> _wekitPlayers = new List<WekitPlayer_Base>();

    [HideInInspector]
    public List<WekitPlayer_Base> ActiveWekitPlayers;

    public float ButtonWidth = 100;

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
            if (value!=_speed)
            {
                _speed = value;
                foreach (WekitPlayer_Base player in ActiveWekitPlayers)
                {
                    player.Speed = value;
                } 
            }
        }
    }

    private float _index;
    public override float Index
    {
        get { return _index; }
        set
        {
            _index = value;
            if (value == 0)
            {
                foreach (WekitPlayer_Base player in ActiveWekitPlayers)
                {
                    player.Index = 0;
                }
            }
        }
    }

    public override void Start()
    {
        base.Start();
        ActiveWekitPlayers = new List<WekitPlayer_Base>(_wekitPlayers);
        foreach (WekitPlayer_Base player in _wekitPlayers)
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

    public override Content AddFrame()
    {
        return new Content();
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
        for (int i = _wekitPlayers.Count - 1; i >= 0; i--)
        {
            WekitPlayer_Base player = _wekitPlayers[i];
            for (int j = FrameList[0].Lists.Count - 1; j >= 0; j--)
            {
                if (FrameList[0].Lists[j].MyName == player.PlayerName)
                {
                    player.MakeDataContainerFromObject(FrameList[0].Lists[j].MyObject);
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
        if (localMaxFrames != 0)
        {
            FrameList = new List<Content>(new Content[localMaxFrames + 1]);
            return true;
        }
        return false;
        /*int localmaxframes=0;
        for (int i=_wekitPlayers.Count-1;i>=0;i--)
        {
            WekitPlayer_Base player = _wekitPlayers[i];
            player.Zip = Zip;
            player.UseCompoundArchive = UseCompoundArchive;
            player.LoadFileName=LoadFileName;
            if (!player.Load())
            {
                player.Enabled(false);
                player.ClearFrameList();
                ActiveWekitPlayers.Remove(player);
            }
            else
            {
                if (player.FrameCount > localmaxframes)
                {
                    localmaxframes = player.FrameCount;
                    ReplayFps = player.ReplayFps;
                }
            }
        }
        //If there is no file for the container but there is one for at least one player, make a list the size of that player's
        if (!base.Load()&&localmaxframes!=0)
        {
            FrameList=new List<Content>(new Content[localmaxframes+1]);
        }*/
        return true;
    }

    public override void Save()
    {
        Content content = new Content(FrameCount,new List<ObjectWithName>());
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            content.Lists.Add(new ObjectWithName(player.GetListAsObject(),player.PlayerName));
        }
        FrameList.Clear();
        FrameList.Add(content);
        base.Save();
        /*foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            player.Zip = Zip;
            player.UseCompoundArchive = UseCompoundArchive;
            player.CompoundZipName = CompoundZipName;
            player.FileName = FileName;
            player.Save();
        }*/
    }

    public override void Delete()
    {
        base.Delete();
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            player.Zip = Zip;
            player.UseCompoundArchive = UseCompoundArchive;
            player.DeleteFileName = DeleteFileName;
            player.CompoundZipName = CompoundZipName;
            player.Delete();
        }
    }

    //Buttons to activate/deactivate players
    void OnGUI()
    {
        if (!Recording)
        {
            for (int i = 0; i < _wekitPlayers.Count; i++)
            {
                float x = Screen.width/2f - ButtonWidth*_wekitPlayers.Count/2 + i*ButtonWidth;
                _wekitPlayers[i].Stepsize= (int)GUI.HorizontalSlider(new Rect(x, Screen.height - 40, ButtonWidth, 20), _wekitPlayers[i].Stepsize, 1, 3);
                bool contained = ActiveWekitPlayers.Contains(_wekitPlayers[i]);
                if (GUI.Button(new Rect(x, Screen.height - 20, ButtonWidth, 20), _wekitPlayers[i].PlayerName + (contained ? " active" : " inactive")))
                {
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
    }

}