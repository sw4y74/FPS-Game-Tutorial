﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MenuManager : MonoBehaviour



{
	public static MenuManager Instance;

	[SerializeField] Menu[] menus;

	void Awake()
	{
		Instance = this;
	}

	public void OpenMenu(string menuName)
	{
		for(int i = 0; i < menus.Length; i++)
		{
			if(menus[i].menuName == menuName)
			{
				menus[i].Open();
			}
			else if(menus[i].open && menus[i].isClosable)
			{
				CloseMenu(menus[i]);
			}
		}
	}

	public void OpenMenu(Menu menu)
	{
		for(int i = 0; i < menus.Length; i++)
		{
			if(menus[i].open && menus[i].isClosable)
			{
				CloseMenu(menus[i]);
			}
		}
		menu.Open();
	}

	public void OpenMenuOptions(Menu menu)
	{
		for(int i = 0; i < menus.Length; i++)
		{
			if(menus[i].open && menus[i].isClosable)
			{
				CloseMenu(menus[i]);
			}
		}
		if (menu.menuName == "room" && PhotonNetwork.InRoom) {
			menu.Open();
		}
	}

	public void OpenMenuInGame(Menu menu)
	{
		for(int i = 0; i < menus.Length; i++)
		{
			if(menus[i].open && menus[i].isClosable && menus[i].name == "pause")
			{
				CloseMenu(menus[i]);
			}
		}
		menu.Open();
	}

	public void CloseMenu(Menu menu)
	{
		menu.Close();
	}

	public void QuitGame()
		{
			Application.Quit();
		}
}