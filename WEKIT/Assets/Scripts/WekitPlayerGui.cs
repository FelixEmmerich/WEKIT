using UnityEngine;

public class WekitPlayerGui : MonoBehaviour
{
    //Player controlled by this GUI
    public WekitPlayer_Base Player;

    private bool _showOptions=true;
    private float _currentTime;
    public int FontSize=10;
    public KeyCode HideKey = KeyCode.H;

    public WekitKeyInput KeyInput;

    public float StandardWidth = 100;

    void Start()
    {
        _currentTime = Player.CountDown;
        if (KeyInput == null)
        {
            KeyInput = GetComponent<WekitKeyInput>();
        }
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
            Application.Quit();
        }
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = GUI.skin.button.fontSize = GUI.skin.textField.fontSize=GUI.skin.toggle.fontSize = FontSize;
        _showOptions = GUI.Toggle(new Rect(10, 5, StandardWidth/1.5f, 20), _showOptions, (_showOptions ? "Hide" : "Show")+KeyToText(HideKey));
        if (_showOptions)
        {
            if (!Player.Recording)
            {
                //Compression options
                Player.Zip = GUI.Toggle(new Rect(10, 30, StandardWidth / 2, 20), Player.Zip, "Zip");

                if (Player.Zip)
                {
                    Player.UseCompoundArchive = GUI.Toggle(new Rect(10, 55, StandardWidth, 30), Player.UseCompoundArchive, "Compound \n archive");

                    if (Player.UseCompoundArchive)
                    {
                        Player.CompoundZipName = GUI.TextField(new Rect(10, 85, StandardWidth, 20), Player.CompoundZipName, 25);
                    }
                }

                if (!Player.Replaying)
                {
                    //Record button
                    if (GUI.Button(new Rect(120 - (100 - StandardWidth), 5, StandardWidth, 20), "Record"+ (KeyInput != null ? KeyToText(KeyInput.RecordKey) : "")))
                    {
                        _currentTime = Player.CountDown;
                        Player.Record();
                    }
                    Player.CountDown = float.Parse(GUI.TextField(new Rect(120 - (100 - StandardWidth), 30, StandardWidth, 20), Player.CountDown.ToString(), 25));
                    Player.Stepsize = (int)GUI.HorizontalSlider(new Rect(120 - (100 - StandardWidth), 60, StandardWidth, 20), Player.Stepsize, 1, 3);

                    //Replay button
                    if (GUI.Button(new Rect(230 - (100 - StandardWidth)*2, 5, StandardWidth, 20), "Replay" + (KeyInput != null ? KeyToText(KeyInput.ReplayKey) : "")))
                    {
                        Player.Replay();
                    }

                    //Load button
                    if (GUI.Button(new Rect(340 - (100 - StandardWidth)*3, 5, StandardWidth, 20), "Load"))
                    {
                        Player.Load();
                    }

                    Player.LoadFileName = GUI.TextField(new Rect(340 - (100 - StandardWidth)*3, 30, StandardWidth, 20), Player.LoadFileName, 25);
                }

                //If not recording but replaying
                else
                {
                    //Replay (stop) button
                    if (GUI.Button(new Rect(230 - (100 - StandardWidth)*2, 5, StandardWidth, 20), "Stop" + (KeyInput!=null?KeyToText(KeyInput.ReplayKey):"")))
                    {
                        Player.Replay();
                    }

                    //Pause/Unpause button
                    if (GUI.Button(new Rect(230 - (100 - StandardWidth)*2, 30, StandardWidth, 20), (Player.Playing ? "Pause" : "Unpause")+(KeyInput != null ? KeyToText(KeyInput.PauseKey) : "")))
                    {
                        Player.Pause();
                    }
                    //Index
                    float index = GUI.HorizontalSlider(new Rect(230 - (100 - StandardWidth)*2, 60, StandardWidth, 10), Player.Index, 0, Player.FrameCount);
                    if (index != Player.Index)
                    {
                        Player.SetIndex(index,false);
                    }

                    //Speed
                    Player.Speed = GUI.HorizontalSlider(new Rect(230 - (100 - StandardWidth)*2, 80, StandardWidth, 10), Player.Speed, 0.1f, 2);
                    Player.Speed = Mathf.Clamp(float.Parse(GUI.TextField(new Rect(230 - (100 - StandardWidth)*2, 100, StandardWidth, 20), Player.Speed.ToString(), 25)),0.1f,2);
                }

                //Save button
                if (GUI.Button(new Rect(450 - (100 - StandardWidth)*4, 5, StandardWidth, 20), "Save"))
                {
                    Player.Save();
                }
                Player.FileName = GUI.TextField(new Rect(450 - (100 - StandardWidth)*4, 30, StandardWidth, 20), Player.FileName, 25);

                //Delete button
                if (GUI.Button(new Rect(560 - (100 - StandardWidth)*5, 5, StandardWidth, 20), "Delete"))
                {
                    Player.Delete();
                }
                Player.DeleteFileName = GUI.TextField(new Rect(560 - (100 - StandardWidth)*5, 30, StandardWidth, 20), Player.DeleteFileName, 25);

            }

            //If recording
            else
            {
                //Record (stop) button
                if (GUI.Button(new Rect(120 - (100 - StandardWidth), 5, StandardWidth, 20), "Stop"+ (KeyInput != null ? KeyToText(KeyInput.RecordKey) : "")))
                {
                    _currentTime = Player.CountDown;
                    Player.Record();
                }
                if (_currentTime > 0)
                {
                    GUI.Label(new Rect(120 - (100 - StandardWidth), 30, StandardWidth, 20), _currentTime.ToString(), new GUIStyle());
                }
            }
        }

    }

    string KeyToText(KeyCode code)
    {
        return " (" + code + ")";
    }
}