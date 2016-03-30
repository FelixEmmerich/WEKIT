using System;
using Thalmic.Myo;
using Quaternion = UnityEngine.Quaternion;

//Serializable container class for data from the myo armband
[Serializable]
public class MyoData
{
    public SerialQuaternion Quaternion;
    public Pose Pose;

    public MyoData()
    {
        Quaternion=new SerialQuaternion(UnityEngine.Quaternion.identity);
        Pose = Pose.Rest;
    }

    public MyoData(SerialQuaternion quaternion, Pose pose)
    {
        Quaternion = quaternion;
        Pose = pose;
    }
}

[Serializable]
public class SerialQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public static implicit operator Quaternion(SerialQuaternion q)
    {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }

    public static implicit operator SerialQuaternion(Quaternion q)
    {
        return new SerialQuaternion(q);
    }

    public SerialQuaternion(Quaternion q)
    {
        x = q.x;
        y = q.y;
        z = q.z;
        w = q.w;
    }

    public SerialQuaternion()
    {
        x = Quaternion.identity.x;
        y = Quaternion.identity.y;
        z = Quaternion.identity.z;
        w = Quaternion.identity.w;
    }

    public SerialQuaternion(float inx, float iny, float inz, float inw)
    {
        x = inx;
        y = iny;
        z = inz;
        w = inw;
    }
}