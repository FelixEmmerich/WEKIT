using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
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

	[Serializable]
	public class UseCaseList
	{
		public UseCase[] UseCases;
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
		LoadUseCases();
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
	}

	//Creates an example XML text file containing the names of all players (except container (this assumes only one container in the scene)) in case the original gets deleted or broken
	void CreateExampleXml(string path)
	{
		UseCases=new UseCase[Players.Length];
		for (int i = 0; i < Players.Length; i++)
		{
			UseCases[i] = new UseCase
			{
				UseCaseName = "Element" + i,
				Elements = new UseCaseElement[1]
			};
			UseCases[i].Elements[0] = new UseCaseElement
			{
				PlayerName = Players[i].PlayerName,
				Message = "Connect " + Players[i].PlayerName
			};
		}
		UseCaseList example = new UseCaseList {UseCases = UseCases};
		XmlSerializer serializer = new XmlSerializer(typeof(UseCaseList));
		FileStream file = File.Open(path, FileMode.OpenOrCreate);
		serializer.Serialize(file, example);
		file.Close();
	}

	void LoadUseCases()
	{
		string path = Application.streamingAssetsPath + @"/UseCases.txt";
		if (File.Exists(path))
		{
			XmlSerializer serializer = new XmlSerializer(typeof(UseCaseList));
			StreamReader reader = new StreamReader(path);
			UseCases = ((UseCaseList) serializer.Deserialize(reader)).UseCases;
			reader.Close();
		}
		//Create file if it doesn't exist
		else
		{
			CreateExampleXml(path);
		}
	}

}