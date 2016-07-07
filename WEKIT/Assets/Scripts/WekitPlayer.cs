using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

//Base class for players
public class WekitPlayer <T,TProvider>: WekitPlayer_Base
{
    //Data is saved along with approximate framerate, for the sake of retaining intended speed
    [Serializable]
    private struct DataContainer
    {
        public readonly List<T> FrameList;
        public readonly float Fps;

        public DataContainer(List<T> frameList, float fps)
        {
            FrameList = frameList;
            Fps = fps;
        }

        public static bool operator == (DataContainer a, DataContainer b)
        {
            return a.FrameList == b.FrameList;
        }

        public static bool operator != (DataContainer a, DataContainer b)
        {
            return !(a == b);
        }
    }

    [Tooltip("GameObjects to hide if focus is lost")]
    public GameObject [] Visualisation;

    public List<T> FrameList;
    public TProvider Provider;

    //File extension for files not saved as .zip
    public string UncompressedFileExtension;

    private bool _updatedIndex;

    //Gives number of frames. Necessary since FrameList can only be used if you know the specific type of player
    public override int FrameCount
    {
        get
        {
            return Math.Max(0,FrameList.Count-1);
        }
    }

    private IEnumerator _coroutine;

    public virtual void Reset()
    {
        SavePath = Application.streamingAssetsPath+@"/Replays/"+CustomDirectory+"/";
    }

    public virtual void Start()
    {
        Debug.Log(PlayerName+" start");
        Speed = 1;
        SavePath = Application.streamingAssetsPath + @"/Replays/" + CustomDirectory + "/";
        Focus = true;
    }
        
    public override void Save()
    {
        string filestring= SavePath;
        Directory.CreateDirectory(filestring);
        DataContainer container = new DataContainer(FrameList, ReplayFps);

        //Save uncompressed
        if (!UseZip)
        {
            filestring += (FileName + "."+UncompressedFileExtension);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filestring, FileMode.OpenOrCreate);
            bf.Serialize(file,container);
            file.Close();
        }

        //If Zip is true, use Ionic.zip compression (smaller filesize, slower)
        else
        {
            filestring += (UseCompoundArchive?CompoundZipName:FileName) + ".zip";
            Compression.AddItemToCompoundArchive(filestring, FileName, container);
        }
        Debug.Log("Saved " + filestring);
    }

    public override bool Load()
    {
        return Load(UseZip, UseZip && UseCompoundArchive ? CompoundZipName : LoadFileName, LoadFileName);
    }

    public override bool Load(bool useZip, string fileName, string entryName)
    {
        string filestring = SavePath;
        DataContainer container;

        //Load uncompressed file
        if (!useZip)
        {
            filestring += fileName + "." + UncompressedFileExtension;
            if (File.Exists(filestring))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(filestring, FileMode.Open);
                container = (DataContainer)bf.Deserialize(file);
                FrameList = container.FrameList;
                ReplayFps = container.Fps;
                file.Close();
                Debug.Log("Loaded " + filestring);
                return true;
            }
            else
            {
                Debug.Log("File can't be loaded: " + filestring + " doesn't exist");
                return false;
            }
        }

        //Load from compressed file
        else
        {
            filestring += (fileName + ".zip");
            if (File.Exists(filestring))
            {
                container = Compression.GetItemFromCompoundArchive<DataContainer>(filestring, entryName);
                if (container!=default(DataContainer))
                {
                    FrameList = container.FrameList;
                    ReplayFps = container.Fps;
                    Debug.Log("Loaded entry " + entryName + " from " + filestring);
                    return true; 
                }
            }
            Debug.Log("File can't be loaded: " + filestring + " doesn't exist");
            return false;
        }
    }

    public override void Delete()
    {
        string filestring= SavePath;

        //Delete uncompressed file
        if (!UseZip)
        {
            filestring += (DeleteFileName + "."+UncompressedFileExtension);
            if (File.Exists(filestring))
            {
                File.Delete(filestring);
                Debug.Log("Deleted" + filestring);
            }
            else
            {
                Debug.Log("File can't be deleted: " + filestring + " doesn't exist");
            }
        }

        //Delete compressed file
        else
        {
            string zipname = UseCompoundArchive ? CompoundZipName : DeleteFileName;
            filestring += (zipname + ".zip");
            if (!UseCompoundArchive)
            {
                if (File.Exists(filestring))
                {
                    File.Delete(filestring);
                    Debug.Log("Deleted" + filestring);
                }
                else
                {
                    Debug.Log("File can't be deleted: " + filestring + " doesn't exist");
                }
            }

            //Delete from compound archive
            else
            {
                if (File.Exists(filestring))
                {
                    if (Compression.RemoveItemFromCompoundArchive(filestring, DeleteFileName, true))
                    {
                        Debug.Log("Deleted " + DeleteFileName + " from " + filestring);
                    }
                    else
                    {
                        Debug.Log("Could not delete " + DeleteFileName + " from " + filestring+": Archive doesn't contain entry");
                    }
                }
                else
                {
                    Debug.Log("Could not delete " + DeleteFileName + " from " + filestring + ": Archive doesn't exist");
                }
            }
        }
    }

    public T GetCurrentFrame()
    {
        if (FrameList.Count == 0)
        {
            Debug.Log(PlayerName+": No frame found");
            return DefaultFrame();
        }
        T myFrame = FrameList[(int)Index];

        //Only update index once per frame
        if (_updatedIndex) return myFrame;
        _updatedIndex = true;
        PreviousIndex = (int) Index;
        if (Playing)
        {
            //Update index, looping
            Index = (Index + Speed * (Time.deltaTime / ReplayFps)) % FrameList.Count;
        }
        return myFrame;
    }

    public virtual T DefaultFrame()
    {
        return default(T);
    }

    public virtual void Update()
    {
        //Resets whether the (replay) index was updated this frame
        _updatedIndex = false;

        //Record data
        if (!Playing || !Recording || Replaying || Provider == null) return;
        CurrentStep = ++CurrentStep%Stepsize;
        //Don't record every frame if stepsize >1
        if (CurrentStep == 0)
        {
            FrameList.Add(AddFrame());
        }
    }

    public override void Record()
    {
        if (Replaying) return;
        //If not recording, begin recording, otherwise stop
        if (!Recording)
        {
            //Start countdown
            _coroutine=RecordAfterTime(CountDown);
            StartCoroutine(_coroutine);
        }
        else
        {
            if (Playing)
            {
                ReplayFps = (ReplayFps + Time.deltaTime)/2*Stepsize;
            }
            else
            {
                StopCoroutine(_coroutine);
            }
            Recording = false;
            Playing = false;
        }
    }

    /// <summary>
    /// Override to suit input device
    /// </summary>
    /// <returns></returns>
    public virtual T AddFrame()
    {
        Debug.Log("Add frame");
        return DefaultFrame();
    }

    public virtual IEnumerator RecordAfterTime(float time)
    {
        if (Recording) yield break;
        Recording = true;
        Index = 0;
        yield return new WaitForSeconds(time);
        //After countdown, only begin the recording process if it wasn't cancelled
        if (!Recording) yield break;
        Debug.Log("Start recording " + PlayerName);
        FrameList.Clear();
        Playing = true;
        ReplayFps = Time.deltaTime;
    }

    //Method is necessary if the type of FrameList isn't known
    public override void ClearFrameList()
    {
        FrameList.Clear();
    }

    public override object GetListAsObject()
    {
        return new DataContainer(FrameList, ReplayFps);
    }

    public override void MakeDataContainerFromObject(object source)
    {
        DataContainer container = (DataContainer)source;
        FrameList = container.FrameList;
        ReplayFps = container.Fps;
    }

    public override void SetIndex(float index, bool relative)
    {
        Index = relative ? FrameCount*index : index;
        _updatedIndex = true;
    }

    public override void SetFocus(bool focus)
    {
        base.SetFocus(focus);
        foreach (GameObject go in Visualisation)
        {
            go.SetActive(Focus);
        }
    }
}