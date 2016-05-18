using UnityEngine;
using System.Collections;
using Leap;
using Leap.Unity;

public class CapsuleHand_Kinect : CapsuleHand
{
    public Vector3 Offset;
    public GameObject MuhParent;

    public override void updateSpheres()
    {
        Offset = RelativationAndWhatnot();
        //Update all spheres
        var fingers = hand_.Fingers;
        for (int i = 0; i < fingers.Count; i++)
        {
            Finger finger = fingers[i];
            for (int j = 0; j < 4; j++)
            {
                int key = getFingerJointIndex((int)finger.Type, j);
                Transform sphere = _jointSpheres[key];

                sphere.position = finger.Bone((Bone.BoneType) j).NextJoint.ToVector3(); //finger.JointPosition((Finger.FingerJoint)j).ToUnityScaled() + Offset;
            }
        }

        palmPositionSphere.position = hand_.PalmPosition.ToVector3() + Offset;

        Vector3 wristPos = hand_.PalmPosition.ToVector3() + Offset;
        wristPositionSphere.position = wristPos;

        Transform thumbBase = _jointSpheres[THUMB_BASE_INDEX];

        Vector3 thumbBaseToPalm = thumbBase.position - (hand_.PalmPosition.ToVector3() + Offset);
        mockThumbJointSphere.position = hand_.PalmPosition.ToVector3() + Offset + Vector3.Reflect(thumbBaseToPalm, hand_.Basis.xBasis.ToVector3().normalized);

    }

    public Vector3 RelativationAndWhatnot()
    {
        Vector3 a = MuhParent == null ? Vector3.zero : MuhParent.transform.position - hand_.PalmPosition.ToVector3();
        Debug.Log(a);
        return a;
    }
}
