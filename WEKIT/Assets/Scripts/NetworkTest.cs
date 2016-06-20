﻿using Leap.Unity;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkTest : NetworkBehaviour
{

    public NetworkLeapHandController a;

    void Update()
    {
        if (!isLocalPlayer)
            return;

        /*if (Input.GetKeyDown(KeyCode.A))
        {
            CmdTest("Original");
        }*/

        /*var x = Input.GetAxis("Horizontal") * 0.1f;
        var y = Input.GetAxis("Vertical") * 0.1f;

        transform.Translate(x, y, 0);*/
    }

    public override void OnStartLocalPlayer()
    {
        //GetComponent<MeshRenderer>().material.color = Color.red;
        GameObject[] leapHands = GameObject.FindGameObjectsWithTag("Leap");
        foreach (GameObject leapHand in leapHands)
        {
            NetworkLeapHandController nlhc = leapHand.GetComponent<NetworkLeapHandController>();
            if (nlhc != null&&nlhc.Server==isServer)
            {
                a = nlhc;
                CmdAssignAuthority(leapHand.GetComponent<NetworkIdentity>(), GetComponent<NetworkIdentity>());
                break;
            }
        }
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

    [Command]
    void CmdAssignAuthority(NetworkIdentity grabID, NetworkIdentity playerID)
    {
        grabID.AssignClientAuthority(connectionToClient);
    }
}