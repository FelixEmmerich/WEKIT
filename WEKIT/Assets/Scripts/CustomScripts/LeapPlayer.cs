using Leap;

public class LeapPlayer : WekitPlayer<HandList,LeapProvider>
{
    //Standard values
    public void Reset()
    {
        UncompressedFileExtension = "LeapData";
        CustomDirectory = "Leap";
        PlayerName = "Leap";
    }

    public override HandList AddFrame()
    {
        return Provider.CurrentFrame.Hands;
    }

}
