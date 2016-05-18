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
            return list.Hands;
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
}