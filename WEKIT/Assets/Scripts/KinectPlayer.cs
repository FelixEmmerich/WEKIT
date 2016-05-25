using System;
using Kinect;
using UnityEngine;

public class KinectPlayer : WekitPlayer<KinectPlayer.MinSkeletonFrame,KinectInterface>
{
    [Serializable]
    public class MinSkeletonFrame
    {
        public SerialSkeletonData[] SkeletonData;

        public MinSkeletonFrame()
        {
            SkeletonData=new SerialSkeletonData[6];
        }

        public static implicit operator NuiSkeletonFrame(MinSkeletonFrame frame)
        {
            NuiSkeletonFrame nui = new NuiSkeletonFrame
            {
                liTimeStamp = (long) (Time.deltaTime*1000),
                dwFrameNumber = 0,
                dwFlags = 0,
                vFloorClipPlane = new Vector4(0, 0, 0, 0),
                vNormalToGravity = new Vector4(0, 0, 0, 0),
                SkeletonData = new NuiSkeletonData[6]
            };
            for (int ii = 0; ii < 6; ii++)
            {
                nui.SkeletonData[ii] = frame.SkeletonData[ii].deserialize();
            }
            return nui;
        }

        public static implicit operator MinSkeletonFrame(NuiSkeletonFrame frame)
        {
            MinSkeletonFrame newFrame=new MinSkeletonFrame();
            for (int ii = 0; ii < 6; ii++)
            {
                newFrame.SkeletonData[ii] = new SerialSkeletonData(frame.SkeletonData[ii]);
            }
            return newFrame;
        }
    }

    public DeviceOrEmulator DevOrEmu;
    public override MinSkeletonFrame AddFrame()
    {
        return Provider.getSkeleton();
    }

    public override MinSkeletonFrame DefaultFrame()
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