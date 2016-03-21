public class MyoPlayer : WekitPlayer<MyoData,JointOrientation>
{
    //Standard values
    public void Reset()
    {
        UncompressedFileExtension = "MyoData";
        CustomDirectory = "Myo";
        PlayerName = "Myo";
    }

    public override MyoData AddFrame()
    {
        return new MyoData(Provider.transform.rotation,Provider.thalmicMyo.pose);
    }

    public override MyoData DefaultFrame()
    {
        return new MyoData();
    }
}