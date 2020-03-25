﻿using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Teleporting Ghosts to other players 
/// </summary>
public class GhostTeleport : MonoBehaviour
{
	//Players
	//Dictionary of UniqueKey(int), data 
	public IDictionary<int, Tuple<string, string, Vector3, GameObject>> MobList = new Dictionary<int, Tuple<string, string, Vector3, GameObject>>();

	public int Count = 0;
	private string NameOfObject;
	private string Status;
	private Vector3 Position;
	private GameObject mobGameObject;
	private PlayerManager playerManager;
	private PlayerSync playerSync;

	//Places
	public IDictionary<int, Tuple<string, Vector3>> PlacesDict = new Dictionary<int, Tuple<string, Vector3>>();

	public int PlacesCount = 0;
	private string NameOfPlace;
	private Vector3 PlacePosition;

	private void Start()
	{
		playerManager = PlayerManager.Instance;
		playerSync = GetComponent<PlayerSync>();
	}

	//Logic for teleporting to players
	//Dictionary of Key(int), object/mob name, status (alive/dead/ghost/none), postion(vector3), GameObject
	private void AddToDict()
	{
		var entry = new Tuple<string, string, Vector3, GameObject>(NameOfObject, Status, Position, mobGameObject);
		MobList.Add(Count, entry);
		Count += 1;
	}

	//Looks through all objects with PlayerScript and then finds their different values(name, status, position). Then calls AddToDict.
	public void FindData()
	{
		MobList.Clear();//Clears Dictionary so everytime button is pressed list is updated.
		Count = 0;
		var PlayerBodies = FindObjectsOfType(typeof(PlayerScript));

		if (PlayerBodies == null | PlayerBodies.Count() == 0)//If list of PlayerScripts is empty dont run rest of code.
		{
		}
		else
		{
			foreach (PlayerScript player in PlayerBodies)
			{
				//Gets Name of Player
				NameOfObject = player.name;

				if(NameOfObject.Length == 0)
				{
					NameOfObject = "Spectator";
				}

				//Gets Status of Player
				if (player.IsGhost)
				{
					Status = "(Ghost)";
				}
				else if(!player.IsGhost & player.playerHealth.IsDead)
				{
					Status = "(Dead)";
				}
				else if (!player.IsGhost)
				{
					Status = "(Alive)";
				}
				else
				{
					Status = "(Cant tell if Dead/Alive or Ghost)";
				}

				//Gets Position of Player
				var tile = player.gameObject.GetComponent<RegisterTile>();
				Position = tile.WorldPositionClient;

				//Gets gameobject
				mobGameObject = player.gameObject;

				AddToDict();// Adds to dictionary
			}
		}		
	}

	//Grabs data needed for teleport.
	public void DataForTeleport(int index)
	{
		var s4 = MobList[index].Item4;//Grabs gameobject from dictionary

		var s3 = s4.GetComponent<RegisterTile>().WorldPositionClient;// Finds current player coords

		PlayerManager.LocalPlayerScript.playerNetworkActions.CmdGhostPerformTeleport(s3);
	}

	//Logic for Teleport to places:
	public void PlacesAddToDict()
	{
		var entry = new Tuple<string, Vector3>(NameOfPlace, PlacePosition);
		PlacesDict.Add(PlacesCount, entry);
		PlacesCount += 1;
	}

	public void PlacesFindData()
	{
		PlacesDict.Clear();
		PlacesCount = 0;

		var placeGameObjects = FindObjectsOfType(typeof(SpawnPoint));

		if (placeGameObjects == null | placeGameObjects.Count() == 0)//If list of SpawnPoints is empty dont run rest of code.
		{
		}
		else
		{
			foreach (SpawnPoint place in placeGameObjects)
			{
				NameOfPlace = place.name;

				if (NameOfPlace.Length == 0)
				{
					NameOfPlace = "Has No Name";
				}

				PlacePosition = place.transform.position;// Only way to get position of this object.

				PlacesAddToDict();
			}
		}
	}

	//Gets Places Data for teleport, NOTE: unlike to mobs teleport the coords are not updated.
	public void PlacesDataForTeleport(int index)
	{
		var vector = PlacesDict[index].Item2;
		PlayerManager.LocalPlayerScript.playerNetworkActions.CmdGhostPerformTeleport(vector);
	}
}
