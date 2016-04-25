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
    private float StandardHeight;

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
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = GUI.skin.button.fontSize = GUI.skin.textField.fontSize=GUI.skin.toggle.fontSize = FontSize;
        _showOptions = GUI.Toggle(new Rect(0, 0, StandardWidth, StandardHeight), _showOptions, (_showOptions ? "Hide" : "Show")+KeyToText(HideKey));
        if (_showOptions)
        {
            if (!Player.Recording)
            {
                //Compression options
                Player.UseZip = GUI.Toggle(new Rect(0, StandardHeight*2, StandardWidth, StandardHeight), Player.UseZip, "Zip");

                if (Player.UseZip)
                {
                    Player.UseCompoundArchive = GUI.Toggle(new Rect(0, StandardHeight*3, StandardWidth, StandardHeight), Player.UseCompoundArchive, "Archive");

                    if (Player.UseCompoundArchive)
                    {
                        Player.CompoundZipName = GUI.TextField(new Rect(0, StandardHeight*4, StandardWidth, StandardHeight), Player.CompoundZipName, 25);
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
                    Player.CountDown = float.Parse(GUI.TextField(new Rect(StandardWidth, StandardHeight, StandardWidth, StandardHeight), Player.CountDown.ToString(), 25));
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
                    Player.Speed = Mathf.Clamp(float.Parse(GUI.TextField(new Rect(StandardWidth*2, StandardHeight*4, StandardWidth, StandardHeight), Player.Speed.ToString(), 25)),0.1f,2);
                }

                //Save button
                if (GUI.Button(new Rect(StandardWidth*4, 0, StandardWidth, StandardHeight), "Save"))
                {
                    Player.Save();
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

    }

    string KeyToText(KeyCode code)
    {
        return " (" + code + ")";
    }
}