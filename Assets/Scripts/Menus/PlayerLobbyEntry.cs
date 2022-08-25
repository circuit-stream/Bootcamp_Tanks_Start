using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Tanks
{
    public class PlayerLobbyEntry : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button readyButton;
        [SerializeField] private GameObject readyText;
        [SerializeField] private Button waitingButton;
        [SerializeField] private GameObject waitingText;

        [SerializeField] private TMP_Text playerName;
        [SerializeField] private Button changeTeamButton;
        [SerializeField] private Image teamHolder;
        [SerializeField] private List<Sprite> teamBackgrounds;

        private Player player;
        private const int nonPlayerID = -1;
        private int changeToTeam;
        private int changeFromTeam;

        public int PlayerTeam // TODO(DONE): Update player team to other clients
        { 
            get => player.CustomProperties.ContainsKey("Team") ? (int)player.CustomProperties["Team"] : 0;
            set
            {
                ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable { { "Team", value } };
                player.SetCustomProperties(hash);
                
            }
        }  

        public bool IsPlayerReady // TODO(DONE): Update player ready status to other clients
        { 
            get => player.CustomProperties.ContainsKey("IsReady") && (bool)player.CustomProperties["IsReady"];
            set
            {

                ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable { { "IsReady", value } };
                player.SetCustomProperties(hash);
            }

        } 


        // TODO (DONE): Get if this entry belongs to the local player
        private bool IsLocalPlayer => Equals(player, PhotonNetwork.LocalPlayer);

        public void Setup(Player enteringPlayer)
        {
            // TODO (DONE): Store and update player information
            player = enteringPlayer;

            if (IsLocalPlayer)
            {
                // Update the lobby's team property with the player
                int teamNumber = player.ActorNumber - 1;
                string teamName = $"T{teamNumber}";

                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { teamName, player.ActorNumber } });

                PlayerTeam = (player.ActorNumber - 1) % PhotonNetwork.CurrentRoom.MaxPlayers;
                player.NickName = PlayerPrefs.GetString("PlayerName");
            }
            else
            {
                Destroy(changeTeamButton);
            }

            playerName.text = player.NickName;
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            teamHolder.sprite = teamBackgrounds[PlayerTeam];
            playerName.text = player.NickName;
            waitingText.SetActive(!IsPlayerReady);
            readyText.SetActive(IsPlayerReady);
        }

        private void Start()
        {
            waitingButton.onClick.AddListener(() => OnReadyButtonClick(true));
            readyButton.onClick.AddListener(() => OnReadyButtonClick(false));
            changeTeamButton.onClick.AddListener(OnChangeTeamButtonClicked);

            waitingButton.gameObject.SetActive(IsLocalPlayer);
            readyButton.gameObject.SetActive(false);
        }

        private void OnChangeTeamButtonClicked()
        {

            bool openTeamFound = false;
            int totalTeams = (int)PhotonNetwork.CurrentRoom.MaxPlayers;
            string tryTeamName = "";
            changeFromTeam = PlayerTeam;

            // check each team index to find an unoccupied one
            
            for(int teamOffset = 1; teamOffset < totalTeams; teamOffset++)
            {
                tryTeamName = $"T{(PlayerTeam + teamOffset) % totalTeams}";
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(tryTeamName) && !openTeamFound)
                {
                    // see if the team is available
                    openTeamFound = (int)PhotonNetwork.CurrentRoom.CustomProperties[tryTeamName] == nonPlayerID;
                    changeToTeam = (PlayerTeam + teamOffset) % totalTeams;
 
                }

            }

            if (openTeamFound)
            {
                string newTeamName = $"T{changeToTeam}";
                string oldTeamName = $"T{changeFromTeam}";

                // update the team, as long as it is still available; also free up the old team at the same time
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { newTeamName, player.ActorNumber }, { oldTeamName, -1 } }, 
                                                              new Hashtable { { newTeamName, -1} });
            }

        }

        private void OnReadyButtonClick(bool isReady)
        {
            waitingButton.gameObject.SetActive(!isReady);
            waitingText.SetActive(!isReady);
            readyButton.gameObject.SetActive(isReady);
            readyText.SetActive(isReady);

            IsPlayerReady = isReady;
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
   
            PlayerTeam = changeToTeam;

            //for (int i = 0; i < 4; i++)
            //{
            //    string teamName = $"T{i}";
            //    Debug.Log($"Team {i} is occupied by {(int)PhotonNetwork.CurrentRoom.CustomProperties[teamName]}");
            //}
        }
    }
}