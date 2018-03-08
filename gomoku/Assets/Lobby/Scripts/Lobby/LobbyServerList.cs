﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections;
using System.Collections.Generic;

namespace Prototype.NetworkLobby
{
    public class LobbyServerList : MonoBehaviour
    {
        public LobbyManager lobbyManager;

        public RectTransform serverListRect;
        public GameObject serverEntryPrefab;
        public GameObject noServerFound;
        public Text roomNameField;

        protected int currentPage = 0;
        protected int previousPage = 0;

        static Color OddServerColor = new Color(41 / 255f, 46 / 255f, 53 / 255f, 1f);
        static Color EvenServerColor = new Color(30 / 255f, 35 / 255f, 45 / 255f, 1f);

        void OnEnable()
        {
            currentPage = 0;
            previousPage = 0;

            foreach (Transform t in serverListRect)
                Destroy(t.gameObject);

            noServerFound.SetActive(false);

            RequestPage(0);
        }

		public void OnGUIMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
		{
			if (matches.Count == 0)
			{
                if (currentPage == 0)
                {
                    noServerFound.SetActive(true);
                }

                currentPage = previousPage;
               
                return;
            }

            noServerFound.SetActive(false);
            foreach (Transform t in serverListRect)
                Destroy(t.gameObject);

			for (int i = 0; i < matches.Count; ++i)
			{
                GameObject o = Instantiate(serverEntryPrefab) as GameObject;

				o.GetComponent<LobbyServerEntry>().Populate(matches[i], lobbyManager, (i % 2 == 0) ? OddServerColor : EvenServerColor);

				o.transform.SetParent(serverListRect, false);
            }
        }

        public void ChangePage(int dir)
        {
            int newPage = Mathf.Max(0, currentPage + dir);

            //if we have no server currently displayed, need we need to refresh page0 first instead of trying to fetch any other page
            if (noServerFound.activeSelf)
                newPage = 0;

            RequestPage(newPage);
        }

        public void FindRoomByName()
        {
            string roomName = roomNameField.text;
            RequestPage(0, roomName);
		}

        public void RequestPage(int page, string roomName = "")
        {
            if (roomName == "")         // Enable prev/next button to work with roomNameSearch
                roomName = roomNameField.text;
            previousPage = currentPage;
            currentPage = page;
			lobbyManager.matchMaker.ListMatches(page, 6, roomName, true, 0, 0, OnGUIMatchList);
			//lobbyManager.matchMaker.ListMatches(page, 6, roomName, false, 0, 0, OnGUIMatchList); // TODO: need this if you want to implement password
		}
    }
}
