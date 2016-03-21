using UnityEngine;
using System.Collections.Generic;

public class WekitPlayerContainer : WekitPlayer<bool, bool>
{
    [SerializeField] private List<WekitPlayer_Base> _wekitPlayers = new List<WekitPlayer_Base>();

    [HideInInspector]
    public List<WekitPlayer_Base> ActiveWekitPlayers;

    public float ButtonWidth = 100;


    public void Reset()
    {
        UncompressedFileExtension = "WEKITData";
        PlayerName = "Container";
    }

    private float _index;
    public override float Index
    {
        get { return _index; }
        set
        {
            _index = value;
            foreach (WekitPlayer_Base player in ActiveWekitPlayers)
            {
                player.Index = Mathf.Min(value,player.FrameCount);
            }
        }
    }

    void Start()
    {
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

    public override bool AddFrame()
    {
        return new bool();
    }

    public override void Replay()
    {
        foreach (WekitPlayer_Base player in ActiveWekitPlayers)
        {
            player.Replaying = Replaying;
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
            player.Stepsize = Stepsize;
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
        base.Load();
        for (int i=ActiveWekitPlayers.Count-1;i>=0;i--)
        {
            WekitPlayer_Base player = ActiveWekitPlayers[i];
            player.Zip = Zip;
            player.UseCompoundArchive = UseCompoundArchive;
            player.LoadFileName=LoadFileName;
            if (!player.Load())
            {
                player.Enabled(false);
                player.ClearFrameList();
                ActiveWekitPlayers.Remove(player);
            }
        }
        return true;
    }

    public override void Save()
    {
        base.Save();
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
                bool contained = ActiveWekitPlayers.Contains(_wekitPlayers[i]);
                if (GUI.Button(new Rect(Screen.width / 2f - ButtonWidth * _wekitPlayers.Count / 2 + i * ButtonWidth, Screen.height - 20, ButtonWidth, 20), _wekitPlayers[i].PlayerName + (contained ? " active" : " inactive")))
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
                        _wekitPlayers[i].Enabled(true);
                        ActiveWekitPlayers.Add(_wekitPlayers[i]);
                        Debug.Log("Added " + _wekitPlayers[i] + " to List");
                    }
                }
            } 
        }
    }

}