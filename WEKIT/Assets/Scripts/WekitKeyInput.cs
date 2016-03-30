using UnityEngine;

public class WekitKeyInput : MonoBehaviour
{
    public WekitPlayer_Base Player;

    public KeyCode RecordKey=KeyCode.F8;
    public KeyCode ReplayKey= KeyCode.F9;
    public KeyCode PauseKey= KeyCode.F10;

    void Start()
    {
        if (Player == null)
        {
            Player = GetComponent<WekitPlayer_Base>();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {

        //Shortcuts
        if (Input.GetKeyDown(RecordKey))
        {
            Player.Record();
        }

        if (Input.GetKeyDown(ReplayKey))
        {
            Player.Replay();
        }

        if (Input.GetKeyDown(PauseKey))
        {
            Player.Pause();
        }
    }
}
