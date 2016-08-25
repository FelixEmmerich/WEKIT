using UnityEngine;

public abstract class WekitPlayer_Base : MonoBehaviour
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

    public abstract void Save();

    /// <summary>
    /// Loads file using default values
    /// </summary>
    /// <returns>Was loading successful?</returns>
    public virtual bool Load()
    {
        return Load(UseZip, UseZip && UseCompoundArchive ? CompoundZipName : LoadFileName, LoadFileName);
    }

    /// <summary>
    /// Loads a file
    /// </summary>
    /// <param name="zip">Whether the file in question is a .zip file</param>
    /// <param name="fileName"></param>
    /// <param name="entryName">Name of the entry when loading a .zip file</param>
    /// <returns></returns>
    public abstract bool Load(bool zip, string fileName, string entryName);

    public virtual void Delete()
    {
        Debug.Log(PlayerName + ": Delete");
    }

    /// <summary>
    /// Called immediately after the record button is hit. Should distinguish between beginning and end of recording. 
    /// </summary>
    public abstract void Record();

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

    /// <summary>
    /// Clears the frame list in WekitPlayer-derived classes. Implementation may differ for other classes (see AudioPlayer).
    /// </summary>
    public virtual void ClearFrameList()
    {
        Debug.Log(PlayerName + ": ClearFrameList");
    }

    /// <summary>
    /// Get Replay data as a generic object. Used by WekitPlayerContainer for saving multiple replays in a single file.
    /// </summary>
    /// <returns></returns>
    public virtual object GetListAsObject()
    {
        return new object();
    }

    /// <summary>
    /// Get Replay data from a generic object. Used by WekitPlayerContainer for loading multiple replays from a single file.
    /// </summary>
    public virtual void MakeDataContainerFromObject(object source)
    {
    }

    /// <summary>
    /// Set player Index. If relative, expects a normalized value.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="relative"></param>
    public virtual void SetIndex(float index, bool relative)
    {
    }

    /// <summary>
    /// Focus can be used to e.g. disable rendering of the kinect skeleton when no input is detected.
    /// Conditions for this must be defined according to use case.
    /// </summary>
    /// <param name="focus"></param>
    public virtual void SetFocus(bool focus)
    {
        //Always true if focus is enforced
        Focus = ForceFocus || focus;
    }

    /// <summary>
    /// Actions performed after the record button was clicked but before the countdown is finished (e.g. setting up the microphone in AudioPlayer).
    /// </summary>
    public virtual void SetUpRecording()
    {
        Debug.Log(PlayerName + ": SetUpRecording");
    }

    /// <summary>
    /// Begins the recording (e.g. after the countdown is finished). Should set the Recording variable to true. 
    /// </summary>
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

    /// <summary>
    /// Custom GUI. By default only called if HasGui && GuiIsActive. Used by WekitPlayer_Container.
    /// </summary>
    public virtual void CustomGUI()
    {
        Debug.Log(PlayerName + ": CustomGUI");
    }
}