using System;
using UnityEngine;

public class WekitPlayer_Base : MonoBehaviour
{

    [Serializable]
    public class XMLData
    {
        public XMLFileInfo[] Files;

        public XMLData(XMLFileInfo fileInfo)
        {
            Files=new XMLFileInfo[1];
            Files[0] = fileInfo;
        }

        public XMLData(XMLFileInfo[] fileInfo)
        {
            Files = fileInfo;
        }

        public XMLData()
        {
            Files=new XMLFileInfo[1];
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

    [HideInInspector]
    public bool UseZip, UseCompoundArchive, Recording, Playing, Replaying, ForceFocus=false;

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
        Debug.Log("Save");
    }

    public virtual bool Load()
    {
        Debug.Log("Load");
        return true;
    }

    public virtual bool Load(bool zip, string fileName, string entryName)
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
        SetFocus(value);
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

    public virtual void SetIndex(float index, bool relative)
    {
    }


    public virtual void SetFocus(bool focus)
    {
        //Always true if focus is enforced
        Focus = ForceFocus || focus;
    }
}