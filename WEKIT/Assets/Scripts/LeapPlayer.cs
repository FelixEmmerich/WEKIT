using System.Collections.Generic;
using Leap;
using Leap.Unity;
using UnityEngine;

public class LeapPlayer : WekitPlayer<List<Hand>,LeapProvider>
{
    //Standard values
    public override void Reset()
    {
        base.Reset();
        UncompressedFileExtension = "LeapData";
        CustomDirectory = "Leap";
        PlayerName = "Leap";
    }

    public override List<Hand> AddFrame()
    {
        return Provider.CurrentFrame.Hands;
    }
}