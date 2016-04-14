using Kinect;
using UnityEngine;

public class KinectPlayer : WekitPlayer<SerialSkeletonFrame,KinectInterface>
{
    public GameObject [] Visualisation;
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

    public override void SetFocus(bool focus)
    {
        base.SetFocus(focus);
        foreach (GameObject go in Visualisation)
        {
            go.SetActive(focus);
        }
    }
}
