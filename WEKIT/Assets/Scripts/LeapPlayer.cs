using Leap;
using UnityEngine;

public class LeapPlayer : WekitPlayer<HandList,LeapProvider>
{
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

    public override void SetFocus(bool focus)
    {
        base.SetFocus(focus);
    }
}