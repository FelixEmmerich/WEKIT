using UnityEngine;
using UnityEngine.Networking;

public class NetworkTest : NetworkBehaviour
{
    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            CmdTest("Original");
        }

        /*var x = Input.GetAxis("Horizontal") * 0.1f;
        var y = Input.GetAxis("Vertical") * 0.1f;

        transform.Translate(x, y, 0);*/
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    [Command]
    void CmdTest(string muhstring)
    {
        Debug.Log("Server: " + muhstring);
        RpcTest(muhstring + " modified");
    }

    [ClientRpc]
    void RpcTest(string muhstring)
    {
        if (!this.hasAuthority)
        {
            Debug.Log("Client: " + muhstring);
        }
        else
        {
            Debug.Log("Authority");
        }
    }
}