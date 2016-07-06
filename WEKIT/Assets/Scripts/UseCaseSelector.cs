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
	public int UseCaseElementIndex = -1;
	public WekitPlayerContainer Container;
	public bool Active = true;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public virtual void OnGUI()
	{
		foreach (UseCase useCase in UseCases)
		{
			
		}
	}

	public void ActivateUseCaseElement(UseCaseElement element)
	{
		
	}

}
