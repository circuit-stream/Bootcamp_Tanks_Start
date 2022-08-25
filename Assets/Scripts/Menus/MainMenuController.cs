using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

namespace Tanks
{
    public class MainMenuController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsController settingsPopup;
        [SerializeField] private byte maxPlayersInRoom = 4;
        [SerializeField] TextMeshProUGUI connectingText;

        private void Start()
        {
 
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                connectingText.text = "Connecting...";
                PhotonNetwork.ConnectUsingSettings();
            }

            settingsPopup.gameObject.SetActive(false);
            settingsPopup.Setup();

            if (!PlayerPrefs.HasKey("PlayerName"))
                PlayerPrefs.SetString("PlayerName", "Player #" + Random.Range(0, 9999));
        }

        public override void OnConnectedToMaster()
        {
            connectingText.text = " ";
            playButton.onClick.AddListener(JoinRandomRoom);
            lobbyButton.onClick.AddListener(GoToLobbyList);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            connectingText.text = "Disconnected: " + cause.ToString();
        }

        private void OnSettingsButtonClicked()
        {
            settingsPopup.gameObject.SetActive(true);
        }

        public void JoinRandomRoom()
        {
            RoomOptions roomOptions = new RoomOptions{ IsOpen = true, MaxPlayers = maxPlayersInRoom };
            PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: roomOptions);
        }

        public override void OnJoinedRoom()
        {
            SceneManager.LoadScene("RoomLobby");
        }

        private void GoToLobbyList()
        {
            SceneManager.LoadSceneAsync("LobbyList");
        }
    }
}