using System;
using UnityEngine;

public class KinectVideoPlayer : WekitPlayer<KinectVideoPlayer.KinectFrame[],DeviceOrEmulator>
{
    private Kinect.KinectInterface _kinect;

    [Serializable]
    public class SerializableColor
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public SerializableColor()
        {
            A = R = G = B = 1;
        }

        public SerializableColor(Color32 color)
        {
            A = color.a;
            R = color.r;
            G = color.g;
            B = color.b;
        }

        public static implicit operator Color32(SerializableColor frame)
        {
            return new Color32(frame.R,frame.G,frame.B,frame.A);
        }

        public static implicit operator SerializableColor(Color32 color)
        {
            return new SerializableColor(color);
        }
    }

    public class KinectFrame
    {
        SerializableColor[] Colors;

        public KinectFrame()
        {
            Colors=new SerializableColor[1];
        }

        public KinectFrame(Color32[] colors)
        {
            //Colors = colors;
        }
    }

    public override void Start()
    {
        base.Start();
        _kinect = Provider.getKinect();
    }

    //Standard values
    public override void Reset()
    {
        base.Reset();
        UncompressedFileExtension = "KinectVidData";
        CustomDirectory = "KinectVid";
        PlayerName = "KinectVideo";
    }

    public override KinectFrame[] AddFrame()
    {
        return new KinectFrame[1];
    }

}