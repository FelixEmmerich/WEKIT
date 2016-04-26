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
    private int _fontSize;
    public KeyCode HideKey = KeyCode.H;

    public WekitKeyInput KeyInput;

    private float _standardWidth;
    private float _standardHeight;

    private bool _useXML;

    void Start()
    {
        _currentTime = Player.CountDown;
        if (KeyInput == null)
        {
            KeyInput = GetComponent<WekitKeyInput>();
        }
        //Make the GUI span the full width of the screen
        _standardWidth = Screen.width/6f;
        _standardHeight = Screen.height/20f;
        _fontSize = (int)(_standardWidth/8.5f);
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
        GUI.skin.label.fontSize = GUI.skin.button.fontSize = GUI.skin.textField.fontSize=GUI.skin.toggle.fontSize = _fontSize;
        _showOptions = GUI.Toggle(new Rect(0, 0, _standardWidth, _standardHeight), _showOptions, (_showOptions ? "Hide" : "Show")+KeyToText(HideKey));
        //XML file handling
        _useXML = GUI.Toggle(new Rect(0, _standardHeight, _standardWidth, _standardHeight), _useXML, "Use XML");
        if (_showOptions)
        {
            if (!Player.Recording)
            {
                //Compression options
                Player.UseZip = GUI.Toggle(new Rect(0, _standardHeight*3, _standardWidth, _standardHeight), Player.UseZip, "Zip");

                if (Player.UseZip)
                {
                    Player.UseCompoundArchive = GUI.Toggle(new Rect(0, _standardHeight*4, _standardWidth, _standardHeight), Player.UseCompoundArchive, "Archive");

                    if (Player.UseCompoundArchive)
                    {
                        Player.CompoundZipName = GUI.TextField(new Rect(0, _standardHeight*5, _standardWidth, _standardHeight), Player.CompoundZipName, 25);
                    }
                }

                if (!Player.Replaying)
                {
                    //Record button
                    if (GUI.Button(new Rect(_standardWidth,0,_standardWidth,_standardHeight), "Record"+ (KeyInput != null ? KeyToText(KeyInput.RecordKey) : "")))
                    {
                        _currentTime = Player.CountDown;
                        Player.Record();
                    }
                    Player.CountDown = Single.Parse(GUI.TextField(new Rect(_standardWidth, _standardHeight, _standardWidth, _standardHeight), Player.CountDown.ToString(), 25));
                    Player.Stepsize = (int)GUI.HorizontalSlider(new Rect(_standardWidth, _standardHeight*2, _standardWidth, _standardHeight), Player.Stepsize, 1, 3);

                    //Replay button
                    if (GUI.Button(new Rect(_standardWidth*2, 0, _standardWidth, _standardHeight), "Replay" + (KeyInput != null ? KeyToText(KeyInput.ReplayKey) : "")))
                    {
                        Player.Replay();
                    }

                    //Load button
                    if (GUI.Button(new Rect(_standardWidth*3, 0, _standardWidth, _standardHeight), "Load"))
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

                    Player.LoadFileName = GUI.TextField(new Rect(_standardWidth*3, _standardHeight, _standardWidth, _standardHeight), Player.LoadFileName, 25);
                }

                //If not recording but replaying
                else
                {
                    //Replay (stop) button
                    if (GUI.Button(new Rect(_standardWidth*2, 0, _standardWidth, _standardHeight), "Stop" + (KeyInput!=null?KeyToText(KeyInput.ReplayKey):"")))
                    {
                        Player.Replay();
                    }

                    //Pause/Unpause button
                    if (GUI.Button(new Rect(_standardWidth*2, _standardHeight, _standardWidth, _standardHeight), (Player.Playing ? "Pause" : "Unpause")+(KeyInput != null ? KeyToText(KeyInput.PauseKey) : "")))
                    {
                        Player.Pause();
                    }
                    //Index
                    float index = GUI.HorizontalSlider(new Rect(_standardWidth*2, _standardHeight*2, _standardWidth, _standardHeight), Player.Index, 0, Player.FrameCount);
                    if (index != Player.Index)
                    {
                        Player.SetIndex(index,false);
                    }

                    //Speed
                    Player.Speed = GUI.HorizontalSlider(new Rect(_standardWidth*2, _standardHeight*3, _standardWidth, _standardHeight), Player.Speed, 0.1f, 2);
                    Player.Speed = Mathf.Clamp(Single.Parse(GUI.TextField(new Rect(_standardWidth*2, _standardHeight*4, _standardWidth, _standardHeight), Player.Speed.ToString(), 25)),0.1f,2);
                }

                //Save button
                if (GUI.Button(new Rect(_standardWidth*4, 0, _standardWidth, _standardHeight), "Save"))
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
                Player.FileName = GUI.TextField(new Rect(_standardWidth*4, _standardHeight, _standardWidth, _standardHeight), Player.FileName, 25);

                //Delete button
                if (GUI.Button(new Rect(_standardWidth*5, 0, _standardWidth, _standardHeight), "Delete"))
                {
                    Player.Delete();
                }
                Player.DeleteFileName = GUI.TextField(new Rect(_standardWidth*5, _standardHeight, _standardWidth, _standardHeight), Player.DeleteFileName, 25);

            }

            //If recording
            else
            {
                //Record (stop) button
                if (GUI.Button(new Rect(_standardWidth, 0, _standardWidth, _standardHeight), "Stop"+ (KeyInput != null ? KeyToText(KeyInput.RecordKey) : "")))
                {
                    _currentTime = Player.CountDown;
                    Player.Record();
                }
                if (_currentTime > 0)
                {
                    GUI.Label(new Rect(_standardWidth, _standardHeight, _standardWidth, _standardHeight), _currentTime.ToString(), new GUIStyle());
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
                Player.Speed = 1;
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
                Player.Speed=1;
            }
        }

    }

    string KeyToText(KeyCode code)
    {
        return " (" + code + ")";
    }
}