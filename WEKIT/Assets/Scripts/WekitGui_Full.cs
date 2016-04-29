using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class WekitGui_Full : WekitGui
{
    private float _currentTime;

    public override void Update()
    {
        if (Player.Recording&&!Player.Playing)
        {
            _currentTime -= Time.deltaTime;
        }
    }

    public override void OnGUI()
    {
        base.OnGUI();
        if (!ShowOptions) return;
        if (!Player.Recording)
        {
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

                //Load button
                if (GUI.Button(new Rect(StandardWidth*3, 0, StandardWidth, StandardHeight), "Load"))
                {
                    Load();
                }

                Player.LoadFileName = GUI.TextField(new Rect(StandardWidth*3, StandardHeight, StandardWidth, StandardHeight), Player.LoadFileName, 25);
            }

            //Save button
            if (GUI.Button(new Rect(StandardWidth*4, 0, StandardWidth, StandardHeight), "Save"))
            {
                Player.Save();
                if (UseXml)
                {
                    XMLData data = new XMLData(new XMLFileInfo(Player.UseZip&&Player.UseCompoundArchive ? Player.CompoundZipName : Player.FileName, Player.FileName, Player.UseZip));

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

}