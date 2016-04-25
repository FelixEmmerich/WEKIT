using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class ReplayGui : MonoBehaviour
{
    public WekitPlayerGui.XMLData XmlData;
    private int _xmlDataIndex;

    //Player controlled by this GUI
    public WekitPlayer_Base Player;

    private bool _showOptions = true;
    public int FontSize;
    public KeyCode HideKey = KeyCode.H;

    public WekitKeyInput KeyInput;

    public float StandardWidth = 100;
    private float StandardHeight;

    private bool _useXML;

    void Start()
    {
        if (KeyInput == null)
        {
            KeyInput = GetComponent<WekitKeyInput>();
        }
        //Make the GUI span the full width of the screen
        StandardWidth = Screen.width / 6f;
        StandardHeight = Screen.height / 20f;
        FontSize = (int)(StandardWidth / 8.5f);
    }

    void Update()
    {
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
        GUI.skin.label.fontSize = GUI.skin.button.fontSize = GUI.skin.textField.fontSize = GUI.skin.toggle.fontSize = FontSize;
        _showOptions = GUI.Toggle(new Rect(0, 0, StandardWidth, StandardHeight), _showOptions, (_showOptions ? "Hide" : "Show") + KeyToText(HideKey));
        //XML file handling
        _useXML = GUI.Toggle(new Rect(0, StandardHeight, StandardWidth, StandardHeight), _useXML, "Use XML");
        if (_showOptions)
        {
            //Compression options
            Player.UseZip = GUI.Toggle(new Rect(0, StandardHeight*3, StandardWidth, StandardHeight), Player.UseZip,
                "Zip");

            if (Player.UseZip)
            {
                Player.UseCompoundArchive = GUI.Toggle(new Rect(0, StandardHeight*4, StandardWidth, StandardHeight),
                    Player.UseCompoundArchive, "Archive");

                if (Player.UseCompoundArchive)
                {
                    Player.CompoundZipName = GUI.TextField(
                        new Rect(0, StandardHeight*5, StandardWidth, StandardHeight), Player.CompoundZipName, 25);
                }
            }

            //Replay button
            if (GUI.Button(new Rect(StandardWidth * 2, 0, StandardWidth, StandardHeight),
                (Player.Replaying?"Stop":"Replay") + (KeyInput != null ? KeyToText(KeyInput.ReplayKey) : "")))
            {
                Player.Replay();
            }

            if (!Player.Replaying)
            {
                //Load button
                if (GUI.Button(new Rect(StandardWidth*3, 0, StandardWidth, StandardHeight), "Load"))
                {
                    Player.Load();

                    if (_useXML)
                    {
                        if (!Player.UseZip)
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(WekitPlayerGui.XMLData));
                            StreamReader reader =
                                new StreamReader(Player.SavePath + "/" + Player.CustomDirectory + "/" +
                                                 Player.LoadFileName + ".txt");
                            XmlData = (WekitPlayerGui.XMLData) serializer.Deserialize(reader);
                            reader.Close();
                        }
                        else
                        {
                            XmlData =
                                Compression.GetItemFromCompoundArchive<WekitPlayerGui.XMLData>(
                                    Player.SavePath + "/" + Player.CustomDirectory + "/" +
                                    (Player.UseCompoundArchive ? Player.CompoundZipName : Player.LoadFileName) + ".zip",
                                    Player.LoadFileName + ".txt", new XmlSerializer(typeof(WekitPlayerGui.XMLData)));
                        }
                        _xmlDataIndex = 0;
                    }
                }

                Player.LoadFileName =
                    GUI.TextField(new Rect(StandardWidth*3, StandardHeight, StandardWidth, StandardHeight),
                        Player.LoadFileName, 25);
            }

            //If replaying
            else
            {
                //Pause/Unpause button
                if (GUI.Button(new Rect(StandardWidth*2, StandardHeight, StandardWidth, StandardHeight),
                    (Player.Playing ? "Pause" : "Unpause") + (KeyInput != null ? KeyToText(KeyInput.PauseKey) : "")))
                {
                    Player.Pause();
                }
                //Index
                float index =
                    GUI.HorizontalSlider(new Rect(StandardWidth*2, StandardHeight*2, StandardWidth, StandardHeight),
                        Player.Index, 0, Player.FrameCount);
                if (index != Player.Index)
                {
                    Player.SetIndex(index, false);
                }

                //Speed
                Player.Speed =
                    GUI.HorizontalSlider(new Rect(StandardWidth*2, StandardHeight*3, StandardWidth, StandardHeight),
                        Player.Speed, 0.1f, 2);
                Player.Speed =
                    Mathf.Clamp(
                        Single.Parse(
                            GUI.TextField(new Rect(StandardWidth*2, StandardHeight*4, StandardWidth, StandardHeight),
                                Player.Speed.ToString(), 25)), 0.1f, 2);
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
                WekitPlayerGui.XMLFileInfo data = XmlData.Files[_xmlDataIndex];
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
                WekitPlayerGui.XMLFileInfo data = XmlData.Files[_xmlDataIndex];
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