using Kinect;

public class KinectPlayer : WekitPlayer<SerialSkeletonFrame,KinectInterface>
{
    /*[Serializable]
    public class MinSkeletonData
    {
        
    }*/

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

    public override void Start()
    {
        base.Start();
        Provider = DevOrEmu.getKinect();
    }

}