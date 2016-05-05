using UnityEngine;
using System.Collections;

public class AudioPlayer : WekitPlayer<bool,bool>
{
    //A boolean that flags whether there's a connected microphone
    private bool _micConnected;

    //The maximum and minimum available recording frequencies
    private int _minFreq;
    private int _maxFreq;

    //A handle to the attached AudioSource
    public AudioSource AudioSource;

    //Standard values
    public override void Reset()
    {
        base.Reset();
        UncompressedFileExtension = "AudioData";
        CustomDirectory = "Audio";
        PlayerName = "Audio";
    }

    //Use this for initialization
    public override void Start()
    {
        base.Start();
        //Check if there is at least one microphone connected
        if (Microphone.devices.Length <= 0)
        {
            //Throw a warning message at the console if there isn't
            Debug.LogWarning("Microphone not connected!");
        }
        else //At least one microphone is present
        {
            //Set 'micConnected' to true
            _micConnected = true;

            //Get the default microphone recording capabilities
            Microphone.GetDeviceCaps(null, out _minFreq, out _maxFreq);

            //According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...
            if (_minFreq == 0 && _maxFreq == 0)
            {
                //...meaning 44100 Hz can be used as the recording sampling rate
                _maxFreq = 44100;
            }

            //Get the attached AudioSource component
            if (AudioSource == null)
            {
                AudioSource = GetComponent<AudioSource>();
            }
        }
    }

    public override void Update()
    {
        
    }

    public override void Record()
    {
        base.Record();
        if (!Recording)
        {
            Microphone.End(null); //Stop the audio recording
        }
    }

    public override IEnumerator RecordAfterTime(float time)
    {
        if (Recording) yield break;
        Recording = true;
        yield return new WaitForSeconds(time);
        //After countdown, only begin the recording process if it wasn't cancelled
        if (!Recording) yield break;
        Debug.Log("Start recording " + PlayerName);
        Playing = true;
        //Currently the max audio recording time is 30 seconds
        AudioSource.clip = Microphone.Start(null, true, 30, _maxFreq);
    }

    public override void Replay()
    {
        base.Replay();
        if (Replaying)
        {
            Debug.Log("Play audio");
            AudioSource.Play();
        }
        else
        {
            AudioSource.Stop();
            Debug.Log("Stop audio");
        }
    }

    public override void Pause()
    {
        base.Pause();
        if (Playing)
        {
            AudioSource.UnPause();
        }
        else
        {
            AudioSource.Pause();
        }
    }

    /*
    void OnGUI()
    {
        //If there is a microphone
        if (_micConnected)
        {
            //If the audio from any microphone isn't being captured
            if (!Microphone.IsRecording(null))
            {
                //Case the 'Record' button gets pressed
                if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Record"))
                {
                    //Start recording and store the audio captured from the microphone at the AudioClip in the AudioSource
                    AudioSource.clip = Microphone.Start(null, true, 20, _maxFreq);
                }
            }
            else //Recording is in progress
            {
                //Case the 'Stop and Play' button gets pressed
                if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Stop and Play!"))
                {
                    Microphone.End(null); //Stop the audio recording
                    AudioSource.Play(); //Playback the recorded audio
                }

                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 25, 200, 50), "Recording in progress...");
            }
        }
        else // No microphone
        {
            //Print a red "Microphone not connected!" message at the center of the screen
            GUI.contentColor = Color.red;
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Microphone not connected!");
        }

    }*/
}
