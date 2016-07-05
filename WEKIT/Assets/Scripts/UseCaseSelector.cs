using System;
using UnityEngine;
using System.Collections.Generic;

public class UseCaseSelector : MonoBehaviour
{

	[Serializable]
	public class UseCase
	{
		public List<UseCaseElement> Elements;
	}

	[Serializable]
	public class UseCaseElement
	{
		public WekitPlayer_Base Player;
		public string Text;
	}

	public List<UseCase> UseCases;
	public int CurrentIndex=-1;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

}
