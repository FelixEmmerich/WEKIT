using System;
using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;

public class UseCaseSelector : MonoBehaviour
{

	[Serializable]
	public class UseCase
	{
		public string UseCaseName;
		public UseCaseElement[] Elements;
	}

	[Serializable]
	public class UseCaseElement
	{
		public string PlayerName;
		public string Message;
	}

	public UseCase[] UseCases;
	public int UseCaseIndex=-1;
	public int UseCaseElementIndex;
	public WekitPlayerContainer Container;
	public float ButtonSize = 20f;
	private bool _disable = false;
	public WekitPlayer_Base[] Players;

	void Start()
	{
		//Players = Resources.FindObjectsOfTypeAll(typeof(WekitPlayer_Base)) as WekitPlayer_Base[];
	}

	public virtual void OnGUI()
	{
		if (UseCaseIndex<0)
		{
			if (UseCases.Length>0)
			{
				float height = Screen.height / (float)UseCases.Length;
				for (int i = 0; i < UseCases.Length; i++)
				{
					if (GUI.Button(new Rect(0, i * height, Screen.width, height), UseCases[i].UseCaseName))
					{
						UseCaseIndex = i;
					}
				} 
			}
		}
		else
		{
			if (UseCaseElementIndex < UseCases[UseCaseIndex].Elements.Length)
			{
				if (GUI.Button(new Rect(0, 0, Screen.width, Screen.height),
					UseCases[UseCaseIndex].Elements[UseCaseElementIndex].Message))
				{
					foreach (WekitPlayer_Base entry in Players)
					{
						if (entry.PlayerName == UseCases[UseCaseIndex].Elements[UseCaseElementIndex].PlayerName)
						{
							entry.Enabled(true);
							Container.ActiveWekitPlayers.Add(entry);
							Container.WekitPlayers.Add(entry);
							break;
						}
					}
					UseCaseElementIndex++;

				}
			}
			else
			{
				Container.Enabled(true);
				_disable = true;
			}
		}
	}

	//Disabling the gameobject inside OnGUI causes a Unity error, so this function is called instead
	void Update()
	{
		if (_disable)
		{
			transform.root.gameObject.SetActive(false); 
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			CreateExampleXml("");
		}
	}

	//Creates an example XML text file containing the names of all players (except container (this assumes only one container in the scene)) in case the original gets deleted or broken
	void CreateExampleXml(string name)
	{
		UseCases=new UseCase[Players.Length];
		for (int i = 0; i < Players.Length; i++)
		{
			UseCases[i]=new UseCase();
			UseCases[i].UseCaseName = "Element" + i;
			UseCases[i].Elements = new UseCaseElement[1];
			UseCases[i].Elements[0]=new UseCaseElement();
			UseCases[i].Elements[0].PlayerName = Players[i].PlayerName;
			UseCases[i].Elements[0].Message = "Connect "+Players[i].PlayerName;
		}
	}

}