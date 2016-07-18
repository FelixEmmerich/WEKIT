using System;
using UnityEngine;
using System.Collections.Generic;

public class UseCaseSelector : MonoBehaviour
{

	[Serializable]
	public class UseCase
	{
		public string Name;
		public List<UseCaseElement> Elements;
	}

	[Serializable]
	public class UseCaseElement
	{
		public WekitPlayer_Base Player;
		public string Text;
	}

	public List<UseCase> UseCases;
	public int UseCaseIndex=-1;
	public int UseCaseElementIndex = 0;
	public WekitPlayerContainer Container;
	public float ButtonSize = 20f;
	private bool _disable = false;

	public virtual void OnGUI()
	{
		if (UseCaseIndex < 0)
		{
			for (int i = 0; i < UseCases.Count; i++)
			{
				UseCase useCase = UseCases[i];
				if (GUI.Button(new Rect(0, i*ButtonSize, ButtonSize, ButtonSize), UseCases[i].Name))
				{
					UseCaseIndex = i;
				}
			}
		}
		else
		{
			if (UseCaseElementIndex < UseCases[UseCaseIndex].Elements.Count)
			{
				if (GUI.Button(new Rect(0, 0, ButtonSize, ButtonSize),
					UseCases[UseCaseIndex].Elements[UseCaseElementIndex].Text))
				{
					if (UseCases[UseCaseIndex].Elements[UseCaseElementIndex].Player != null)
					{
						WekitPlayer_Base player = UseCases[UseCaseIndex].Elements[UseCaseElementIndex].Player;
						player.Enabled(true);
						Container.ActiveWekitPlayers.Add(player);
						Container.WekitPlayers.Add(player);
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

}