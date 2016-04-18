using UnityEngine;

public class MyoPlayer : WekitPlayer<MyoData,JointOrientation>
{
    public GameObject [] Visualisation;
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

    public override void SetFocus(bool focus)
    {
        base.SetFocus(focus);
        foreach (GameObject go in Visualisation)
        {
            go.SetActive(focus);
        }
    }
}