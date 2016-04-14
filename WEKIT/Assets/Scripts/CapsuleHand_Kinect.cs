using UnityEngine;
using System.Collections;
using Leap;

public class CapsuleHand_Kinect : CapsuleHand
{
    public Vector3 Offset;
    public GameObject MuhParent;

    public override void updateSpheres()
    {
        Offset = RelativationAndWhatnot();
        //Update all spheres
        FingerList fingers = hand_.Fingers;
        for (int i = 0; i < fingers.Count; i++)
        {
            Finger finger = fingers[i];
            for (int j = 0; j < 4; j++)
            {
                int key = getFingerJointIndex((int)finger.Type, j);
                Transform sphere = _jointSpheres[key];
                sphere.position = finger.JointPosition((Finger.FingerJoint)j).ToUnityScaled() + Offset;
            }
        }

        palmPositionSphere.position = hand_.PalmPosition.ToUnity() + Offset;

        Vector3 wristPos = hand_.PalmPosition.ToUnity() + Offset;
        wristPositionSphere.position = wristPos;

        Transform thumbBase = _jointSpheres[THUMB_BASE_INDEX];

        Vector3 thumbBaseToPalm = thumbBase.position - (hand_.PalmPosition.ToUnity() + Offset);
        mockThumbJointSphere.position = hand_.PalmPosition.ToUnity() + Offset + Vector3.Reflect(thumbBaseToPalm, hand_.Basis.xBasis.ToUnity().normalized);

    }

    public Vector3 RelativationAndWhatnot()
    {
        Vector3 a = MuhParent == null ? Vector3.zero : MuhParent.transform.position - hand_.PalmPosition.ToUnity();
        Debug.Log(a);
        return a;
    }
}
