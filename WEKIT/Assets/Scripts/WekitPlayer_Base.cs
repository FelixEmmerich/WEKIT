using UnityEngine;

public class WekitPlayer_Base : MonoBehaviour
{

    [HideInInspector]
    public bool UseZip, UseCompoundArchive, Recording, Playing, Replaying, ForceFocus=false;

    [HideInInspector] public string FileName = "Replay Name",
        LoadFileName = "Replay Name",
        DeleteFileName = "Replay Name",
        CompoundZipName = "Archive Name",
        PlayerName = "Player";

    //Directory within persistent datapath that files are saved in and loaded from
    public string CustomDirectory;

    public bool HasGui, GuiIsActive = true;

    public static implicit operator string(WekitPlayer_Base wekit)
    {
        return wekit.PlayerName;
    }

    //Location where data is saved. By default Unity's persistent data path.
    [HideInInspector]
    public string SavePath;

    [HideInInspector]
    public float CountDown=3, ReplayFps;

    public bool Focus { get; protected set; }

    public virtual float Index { get; set; }
    public virtual float Speed { get; set; }

    [HideInInspector] public int PreviousIndex = -1, CurrentStep = 0, Stepsize = 1;

    public virtual int FrameCount
    {
        get { return 0; }
    }

    public virtual void Save()
    {
        Debug.Log(PlayerName + ": Save");
    }

    public virtual bool Load()
    {
        Debug.Log(PlayerName + ": Load");
        return true;
    }

    public virtual bool Load(bool zip, string fileName, string entryName)
    {
        Debug.Log(PlayerName + ": Load");
        return true;
    }

    public virtual void Delete()
    {
        Debug.Log(PlayerName + ": Delete");
    }

    public virtual void Record()
    {
        Debug.Log(PlayerName + ": Record");
    }

    public virtual void Replay()
    {
        if (Recording) return;
        Replaying = !Replaying;
        if (Replaying)
        {
            Playing = true;
            PreviousIndex = -1;
        }
        else
        {
            Index = 0;
            Speed = 1;
            Playing = false;
        }
    }

    public virtual void Pause()
    {
        if (!Replaying) return;
        Playing = !Playing;
        if (Playing)
        {
            PreviousIndex = -1;
        }
    }

    public virtual void Enabled(bool value)
    {
        SetFocus(value);
        transform.root.gameObject.SetActive(value);
    }

    public virtual void ClearFrameList()
    {
        Debug.Log(PlayerName + ": ClearFrameList");
    }

    public virtual object GetListAsObject()
    {
        return new object();
    }

    public virtual void MakeDataContainerFromObject(object source)
    {
    }

    public virtual void SetIndex(float index, bool relative)
    {
    }

    public virtual void SetFocus(bool focus)
    {
        //Always true if focus is enforced
        Focus = ForceFocus || focus;
    }

    public virtual void SetUpRecording()
    {
        Debug.Log(PlayerName + ": SetUpRecording");
    }

    public virtual void InitiateRecording()
    {
        Debug.Log(PlayerName+": InitiateRecording");
    }

    public virtual void OnGUI()
    {
        if (HasGui && GuiIsActive)
        {
            CustomGUI();
        }
    }

    public virtual void CustomGUI()
    {
        Debug.Log(PlayerName + ": CustomGUI");
    }
}