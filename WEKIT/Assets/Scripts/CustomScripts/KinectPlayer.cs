using Kinect;

public class KinectPlayer : WekitPlayer<SerialSkeletonFrame,KinectInterface>
{
    public DeviceOrEmulator DevOrEmu;
    public override SerialSkeletonFrame AddFrame()
    {
        return Provider.getSkeleton();
    }

    public override SerialSkeletonFrame DefaultFrame()
    {
        return new NuiSkeletonFrame();
    }

    //Standard values
    public override void Reset()
    {
        base.Reset();
        UncompressedFileExtension = "KinectData";
        CustomDirectory = "Kinect";
        PlayerName = "Kinect";
    }

    void Start()
    {
        Provider = DevOrEmu.getKinect();
    }
}
