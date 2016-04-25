using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class WekitPlayerGui : MonoBehaviour
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
    private int _xmlDataIndex;

    //Player controlled by this GUI
    public WekitPlayer_Base Player;

    private bool _showOptions=true;
    private float _currentTime;
    public int FontSize=10;
    public KeyCode HideKey = KeyCode.H;

    public WekitKeyInput KeyInput;

    public float StandardWidth = 100;
    private float StandardHeight;

    private bool _useXML;

    void Start()
    {
        _currentTime = Player.CountDown;
        if (KeyInput == null)
        {
            KeyInput = GetComponent<WekitKeyInput>();
        }
        //Make the GUI span the full width of the screen
        StandardWidth = Screen.width/6f;
        StandardHeight = Screen.height/20f;
        FontSize = (int)(StandardWidth/8.5f);
    }

    void Update()
    {
        if (Player.Recording&&!Player.Playing)
        {
            _currentTime -= Time.deltaTime;
        }
        if (Input.GetKeyDown(HideKey))
        {
            _showOptions = !_showOptions;
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

    void OnGUI()
    {
        GUI.skin.label.fontSize = GUI.skin.button.fontSize = GUI.skin.textField.fontSize=GUI.skin.toggle.fontSize = FontSize;
        _showOptions = GUI.Toggle(new Rect(0, 0, StandardWidth, StandardHeight), _showOptions, (_showOptions ? "Hide" : "Show")+KeyToText(HideKey));
        //XML file handling
        _useXML = GUI.Toggle(new Rect(0, StandardHeight, StandardWidth, StandardHeight), _useXML, "Use XML");
        if (_showOptions)
        {
            if (!Player.Recording)
            {
                //Compression options
                Player.UseZip = GUI.Toggle(new Rect(0, StandardHeight*3, StandardWidth, StandardHeight), Player.UseZip, "Zip");

                if (Player.UseZip)
                {
                    Player.UseCompoundArchive = GUI.Toggle(new Rect(0, StandardHeight*4, StandardWidth, StandardHeight), Player.UseCompoundArchive, "Archive");

                    if (Player.UseCompoundArchive)
                    {
                        Player.CompoundZipName = GUI.TextField(new Rect(0, StandardHeight*5, StandardWidth, StandardHeight), Player.CompoundZipName, 25);
                    }
                }

                if (!Player.Replaying)
                {
                    //Record button
                    if (GUI.Button(new Rect(StandardWidth,0,StandardWidth,StandardHeight), "Record"+ (KeyInput != null ? KeyToText(KeyInput.RecordKey) : "")))
                    {
                        _currentTime = Player.CountDown;
                        Player.Record();
                    }
                    Player.CountDown = Single.Parse(GUI.TextField(new Rect(StandardWidth, StandardHeight, StandardWidth, StandardHeight), Player.CountDown.ToString(), 25));
                    Player.Stepsize = (int)GUI.HorizontalSlider(new Rect(StandardWidth, StandardHeight*2, StandardWidth, StandardHeight), Player.Stepsize, 1, 3);

                    //Replay button
                    if (GUI.Button(new Rect(StandardWidth*2, 0, StandardWidth, StandardHeight), "Replay" + (KeyInput != null ? KeyToText(KeyInput.ReplayKey) : "")))
                    {
                        Player.Replay();
                    }

                    //Load button
                    if (GUI.Button(new Rect(StandardWidth*3, 0, StandardWidth, StandardHeight), "Load"))
                    {
                        Player.Load();

                        if (_useXML)
                        {
                            if (!Player.UseZip)
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(XMLData));
                                StreamReader reader = new StreamReader(Player.SavePath + "/" + Player.CustomDirectory + "/" + Player.LoadFileName + ".txt");
                                XmlData = (XMLData)serializer.Deserialize(reader);
                                reader.Close();
                            }
                            else
                            {
                                XmlData = Compression.GetItemFromCompoundArchive<XMLData>(Player.SavePath + "/" + Player.CustomDirectory + "/" + (Player.UseCompoundArchive ? Player.CompoundZipName : Player.LoadFileName) + ".zip", Player.LoadFileName + ".txt", new XmlSerializer(typeof(XMLData)));
                            }
                            _xmlDataIndex = 0; 
                        }
                    }

                    Player.LoadFileName = GUI.TextField(new Rect(StandardWidth*3, StandardHeight, StandardWidth, StandardHeight), Player.LoadFileName, 25);
                }

                //If not recording but replaying
                else
                {
                    //Replay (stop) button
                    if (GUI.Button(new Rect(StandardWidth*2, 0, StandardWidth, StandardHeight), "Stop" + (KeyInput!=null?KeyToText(KeyInput.ReplayKey):"")))
                    {
                        Player.Replay();
                    }

                    //Pause/Unpause button
                    if (GUI.Button(new Rect(StandardWidth*2, StandardHeight, StandardWidth, StandardHeight), (Player.Playing ? "Pause" : "Unpause")+(KeyInput != null ? KeyToText(KeyInput.PauseKey) : "")))
                    {
                        Player.Pause();
                    }
                    //Index
                    float index = GUI.HorizontalSlider(new Rect(StandardWidth*2, StandardHeight*2, StandardWidth, StandardHeight), Player.Index, 0, Player.FrameCount);
                    if (index != Player.Index)
                    {
                        Player.SetIndex(index,false);
                    }

                    //Speed
                    Player.Speed = GUI.HorizontalSlider(new Rect(StandardWidth*2, StandardHeight*3, StandardWidth, StandardHeight), Player.Speed, 0.1f, 2);
                    Player.Speed = Mathf.Clamp(Single.Parse(GUI.TextField(new Rect(StandardWidth*2, StandardHeight*4, StandardWidth, StandardHeight), Player.Speed.ToString(), 25)),0.1f,2);
                }

                //Save button
                if (GUI.Button(new Rect(StandardWidth*4, 0, StandardWidth, StandardHeight), "Save"))
                {
                    Player.Save();
                    if (_useXML)
                    {
                        XMLData data = new XMLData(new XMLFileInfo(Player.UseCompoundArchive ? Player.CompoundZipName : Player.FileName, Player.FileName, Player.UseZip));

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
                Player.FileName = GUI.TextField(new Rect(StandardWidth*4, StandardHeight, StandardWidth, StandardHeight), Player.FileName, 25);

                //Delete button
                if (GUI.Button(new Rect(StandardWidth*5, 0, StandardWidth, StandardHeight), "Delete"))
                {
                    Player.Delete();
                }
                Player.DeleteFileName = GUI.TextField(new Rect(StandardWidth*5, StandardHeight, StandardWidth, StandardHeight), Player.DeleteFileName, 25);

            }

            //If recording
            else
            {
                //Record (stop) button
                if (GUI.Button(new Rect(StandardWidth, 0, StandardWidth, StandardHeight), "Stop"+ (KeyInput != null ? KeyToText(KeyInput.RecordKey) : "")))
                {
                    _currentTime = Player.CountDown;
                    Player.Record();
                }
                if (_currentTime > 0)
                {
                    GUI.Label(new Rect(StandardWidth, StandardHeight, StandardWidth, StandardHeight), _currentTime.ToString(), new GUIStyle());
                }
            }
        }

        //Multi-replay handling
        if (XmlData == null) return;
        //Previous replay
        if (_xmlDataIndex > 0)
        {
            if (GUI.Button(new Rect(0, Screen.height / 2f, Screen.width / 10f, Screen.height / 5f), "Previous"))
            {
                _xmlDataIndex--;
                XMLFileInfo data = XmlData.Files[_xmlDataIndex];
                Player.Load(data.Zip, data.FileName, data.EntryName);
                Player.SetIndex(0, false);
            }
        }
        //Next replay
        if (_xmlDataIndex < XmlData.Files.Length - 1)
        {
            if (GUI.Button(new Rect(Screen.width * 0.9f, Screen.height / 2f, Screen.width / 10f, Screen.height / 5f), "Next"))
            {
                _xmlDataIndex++;
                XMLFileInfo data = XmlData.Files[_xmlDataIndex];
                Player.Load(data.Zip, data.FileName, data.EntryName);
                Player.SetIndex(0, false);
            }
        }

    }

    string KeyToText(KeyCode code)
    {
        return " (" + code + ")";
    }
}