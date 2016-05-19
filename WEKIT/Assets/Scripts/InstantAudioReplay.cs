using UnityEngine;
using System.Collections;

public class InstantAudioReplay : MonoBehaviour
{
    public AudioSource Audio;

	// Use this for initialization
	void Start ()
    {
        Audio.clip = Microphone.Start("", true, 100, 44100);
        Audio.loop = true;
        while (!(Microphone.GetPosition("") > 0)) { }
        Audio.Play();
    }
}
