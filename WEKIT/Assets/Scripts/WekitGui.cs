using System;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Debug = UnityEngine.Debug;

public class WekitGui : MonoBehaviour
{

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
    internal int XmlDataIndex;
    internal bool UseXml;

    //Player controlled by this GUI
    public WekitPlayer_Base Player;

    internal bool ShowOptions = true;
    private int _fontSize;
    public KeyCode HideKey = KeyCode.H;

    public WekitKeyInput KeyInput;

    [HideInInspector]
    internal float StandardWidth,StandardHeight;

    private Texture2D _progressBar;
    private Texture2D _noProgressBar;

    // Use this for initialization
    public virtual void Start ()
    {
        if (KeyInput == null)
        {
            KeyInput = GetComponent<WekitKeyInput>();
        }
        //Make the GUI span the full width of the screen
        StandardWidth = Screen.width / 6f;
        StandardHeight = Screen.height / 20f;
        _fontSize = (int)(StandardWidth / 8.5f);

        //Initialise Progress bar
        _progressBar = new Texture2D(1, 1);
        _noProgressBar = new Texture2D(1,1);
        _progressBar.SetPixel(0,0,Color.green);
        _noProgressBar.SetPixel(0, 0, Color.gray);
        _progressBar.Apply();
        _noProgressBar.Apply();
    }

    public virtual void Update()
    {
        if (Input.GetKeyDown(HideKey))
        {
            ShowOptions = !ShowOptions;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Application.isEditor)
            {
                Application.Quit();
            }
            //Using Application.Quit() for some reason doesn't always work on standalone. 
            //Check if the program is running in editor is needed, otherwise the code below would close the editor itself
            else
            {
                Process.GetCurrentProcess().Kill();
            }
        }
    }

    public virtual void OnGUI()
    {
        ShowOptions = GUI.Toggle(new Rect(0, 0, StandardWidth, StandardHeight), ShowOptions, (ShowOptions ? "Hide" : "Show") + KeyToText(HideKey));
        if (ShowOptions)
        {
            UseXml = GUI.Toggle(new Rect(0, StandardHeight, StandardWidth, StandardHeight), UseXml, "Use XML");
            GUI.skin.label.fontSize = GUI.skin.button.fontSize = GUI.skin.textField.fontSize = GUI.skin.toggle.fontSize = _fontSize;

            if (!Player.Recording)
            {
                if (GUI.Button(new Rect(StandardWidth * 2, 0, StandardWidth, StandardHeight),
                    (Player.Replaying ? "Stop" : "Replay") + (KeyInput != null ? KeyToText(KeyInput.ReplayKey) : "")))
                {
                    Player.Replay();
                }
                //Compression options
                Player.UseZip = GUI.Toggle(new Rect(0, StandardHeight * 3, StandardWidth, StandardHeight), Player.UseZip,
                    "Zip");

                if (Player.UseZip)
                {
                    Player.UseCompoundArchive = GUI.Toggle(new Rect(0, StandardHeight * 4, StandardWidth, StandardHeight),
                        Player.UseCompoundArchive, "Archive");

                    if (Player.UseCompoundArchive)
                    {
                        Player.CompoundZipName = GUI.TextField(
                            new Rect(0, StandardHeight * 5, StandardWidth, StandardHeight), Player.CompoundZipName, 25);
                    }
                }

                if (Player.Replaying)
                {
                    {
                        //Pause/Unpause button
                        if (GUI.Button(new Rect(StandardWidth * 2, StandardHeight, StandardWidth, StandardHeight), (Player.Playing ? "Pause" : "Unpause") + (KeyInput != null ? KeyToText(KeyInput.PauseKey) : "")))
                        {
                            Player.Pause();
                        }
                        //Index
                        float index = GUI.HorizontalSlider(new Rect(StandardWidth * 2, StandardHeight * 2, StandardWidth, StandardHeight), Player.Index, 0, Player.FrameCount);
                        if (index != Player.Index)
                        {
                            Player.SetIndex(index, false);
                        }

                        //Speed
                        Player.Speed = GUI.HorizontalSlider(new Rect(StandardWidth * 2, StandardHeight * 3, StandardWidth, StandardHeight), Player.Speed, 0.1f, 2);
                        Player.Speed = Mathf.Clamp(Single.Parse(GUI.TextField(new Rect(StandardWidth * 2, StandardHeight * 4, StandardWidth, StandardHeight), Player.Speed.ToString(), 25)), 0.1f, 2);
                    }
                }

                //Multi-replay handling
                if (XmlData!=null&&XmlData.Files.Length>0)
                {
                    
                    // Progress bar
                    GUI.DrawTexture(new Rect(Screen.width-StandardWidth, Screen.height-StandardHeight, StandardWidth, StandardHeight), _noProgressBar);
                    GUI.DrawTexture(new Rect(Screen.width-StandardWidth, Screen.height-StandardHeight, StandardWidth * ((XmlDataIndex+1.0f)/XmlData.Files.Length), StandardHeight),_progressBar);
                    
                    //Previous replay
                    if (XmlDataIndex > 0)
                    {
                        if (GUI.Button(new Rect(0, Screen.height/2f, Screen.width/10f, Screen.height/5f), "Previous"))
                        {
                            XmlDataIndex--;
                            XMLFileInfo data = XmlData.Files[XmlDataIndex];
                            Player.Load(data.Zip, data.FileName, data.EntryName);
                            Player.SetIndex(0, false);
                            Player.Speed = 1;
                        }
                    }
                    //Next replay
                    if (XmlDataIndex < XmlData.Files.Length - 1)
                    {
                        if (GUI.Button(
                            new Rect(Screen.width*0.9f, Screen.height/2f, Screen.width/10f, Screen.height/5f),
                            "Next"))
                        {
                            XmlDataIndex++;
                            XMLFileInfo data = XmlData.Files[XmlDataIndex];
                            Player.Load(data.Zip, data.FileName, data.EntryName);
                            Player.SetIndex(0, false);
                            Player.Speed = 1;
                        }
                    }
                }
            }

        }

    }

    public void Load()
    {
        Player.Load();

        //if (UseXml)
        {
            if (!Player.UseZip)
            {
                string path = Player.SavePath + "/" + Player.CustomDirectory + "/" + Player.LoadFileName + ".txt";
                if (File.Exists(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(XMLData));
                    StreamReader reader = new StreamReader(path);
                    XmlData = (XMLData)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            else
            {
                XmlData = Compression.GetItemFromCompoundArchive<XMLData>(Player.SavePath + "/" + Player.CustomDirectory + "/" + (Player.UseCompoundArchive ? Player.CompoundZipName : Player.LoadFileName) + ".zip", Player.LoadFileName + ".txt", new XmlSerializer(typeof(XMLData)));
            }
            XmlDataIndex = 0;
        }
    }

    public void Save()
    {
        Player.Save();
        if (UseXml)
        {
            XMLData data = new XMLData(new XMLFileInfo(Player.UseZip && Player.UseCompoundArchive ? Player.CompoundZipName : Player.FileName, Player.FileName, Player.UseZip));

            if (!Player.UseZip)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(XMLData));
                FileStream file = File.Open(Player.SavePath + "/" + Player.CustomDirectory + "/" + Player.FileName + ".txt", FileMode.OpenOrCreate);
                serializer.Serialize(file, data);
                file.Close();
            }
            else
            {
                string filestring = Player.SavePath + "/" + Player.CustomDirectory + "/" + (Player.UseCompoundArchive ? Player.CompoundZipName : Player.FileName) + ".zip";
                Compression.AddItemToCompoundArchive(filestring, Player.FileName + ".txt", ref data, new XmlSerializer(typeof(XMLData)));
            }
        }
    }

    public static string KeyToText(KeyCode code)
    {
        return " (" + code + ")";
    }
}