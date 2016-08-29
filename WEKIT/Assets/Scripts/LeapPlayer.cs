using System;
using System.Collections.Generic;
using Leap;
using Leap.Unity;
using UnityEngine;

public class LeapPlayer : WekitPlayer<LeapPlayer.HandList,LeapProvider>
{
    [Serializable]
    public class HandList
    {
        public List<Hand> Hands;

        public HandList()
        {
            Hands=new List<Hand>();
        }

        public HandList(List<Hand> list)
        {
            Hands = list;
        }

        public static implicit operator List<Hand>(HandList list)
        {
            return (list??(list=new HandList())).Hands ?? (list.Hands=new List<Hand>());
        }

        public static implicit operator HandList(List<Hand> list)
        {
            return new HandList(list);
        }
    }
    //Standard values
    public override void Reset()
    {
        base.Reset();
        UncompressedFileExtension = "LeapData";
        CustomDirectory = "Leap";
        PlayerName = "Leap";
    }

    public override HandList AddFrame()
    {
        return Provider.CurrentFrame.Hands;
    }

    public override void CustomGUI()
    {
        if (GUI.Button(new Rect(Screen.width/2f, Screen.height/2f, 100,50), "Gui stuff"))
        {
            Debug.Log("Good job");
        }
    }
}