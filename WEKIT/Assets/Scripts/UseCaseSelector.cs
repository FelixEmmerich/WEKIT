using System;
using UnityEngine;
using System.Collections.Generic;

public class UseCaseSelector : MonoBehaviour
{

	[Serializable]
	public class UseCase
	{
		public List<UseCaseElement> Elements;
		public string Name;
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

	public virtual void OnGUI()
	{
		if (UseCaseIndex < 0)
		{
			for (int i = 0; i < UseCases.Count; i++)
			{
				UseCase useCase = UseCases[i];
				if (!GUI.Button(new Rect(0, i*ButtonSize, ButtonSize, ButtonSize), UseCases[i].Name))
				{
					UseCaseIndex = i;
				}
			}
		}
		else
		{

		}
	}

}
