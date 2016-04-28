using UnityEngine;

public class WekitGui_Replay : WekitGui
{

    public override void OnGUI()
    {
        base.OnGUI();

        if (!ShowOptions || Player.Replaying) return;


        Player.LoadFileName =
            GUI.TextField(new Rect(StandardWidth * 3, StandardHeight, StandardWidth, StandardHeight),
                Player.LoadFileName, 25);

        //Load button
        if (GUI.Button(new Rect(StandardWidth*3, 0, StandardWidth, StandardHeight), "Load"))
        {
            Load();
        }
    }
}