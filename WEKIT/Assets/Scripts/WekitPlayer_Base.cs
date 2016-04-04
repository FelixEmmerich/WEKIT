using UnityEngine;

public class WekitPlayer_Base : MonoBehaviour
{
    [HideInInspector]
    public bool Zip, UseCompoundArchive, Recording, Playing, Replaying;

    [HideInInspector] public string FileName = "Replay Name",
        LoadFileName = "Replay Name",
        DeleteFileName = "Replay Name",
        CompoundZipName = "Archive Name",
        PlayerName = "Player";

    public static implicit operator string(WekitPlayer_Base wekit)
    {
        return wekit.PlayerName;
    }

    [HideInInspector]
    public float CountDown=3, ReplayFps;

    public virtual float Index { get; set; }
    public virtual float Speed { get; set; }

    [HideInInspector] public int PreviousIndex = -1, CurrentStep = 0, Stepsize = 1;

    public virtual int FrameCount
    {
        get { return 0; }
    }

    public virtual void Save()
    {
        Debug.Log("Save");
    }

    public virtual bool Load()
    {
        Debug.Log("Load");
        return true;
    }

    public virtual void Delete()
    {
        Debug.Log("Delete");
    }

    public virtual void Record()
    {
        Debug.Log("Record");
    }

    public virtual void Replay()
    {
        Debug.Log("Replay");
    }

    public virtual void Pause()
    {
        Debug.Log("Pause");
    }

    public virtual void Enabled(bool value)
    {
        transform.root.gameObject.SetActive(value);
    }

    public virtual void ClearFrameList()
    {
    }

    public virtual object GetListAsObject()
    {
        return new object();
    }

    public virtual void MakeDataContainerFromObject(object source)
    {
    }
}