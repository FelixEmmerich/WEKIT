using UnityEngine;

public class MyoPlayer : WekitPlayer<MyoData,JointOrientation>
{
    //Standard values
    public override void Reset()
    {
        base.Reset();
        UncompressedFileExtension = "MyoData";
        CustomDirectory = "Myo";
        PlayerName = "Myo";
    }

    public override MyoData AddFrame()
    {
        return new MyoData(Provider.transform.rotation,Provider.thalmicMyo.pose);
    }

}