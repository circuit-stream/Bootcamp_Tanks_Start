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

        private Action pendingAction;

        private void Start()
        {
            // TODO (DONE): Connect to photon server (using settings we just configured (app id))
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.ConnectUsingSettings();

            }

            // playButton.onClick.AddListener(JoinRandomRoom);
            playButton.onClick.AddListener(() => OnConnectionDependentActionClicked(JoinRandomRoom));
            lobbyButton.onClick.AddListener(GoToLobbyList);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);

            settingsPopup.gameObject.SetActive(false);
            settingsPopup.Setup();

            if (!PlayerPrefs.HasKey("PlayerName"))
                PlayerPrefs.SetString("PlayerName", "Player #" + Random.Range(0, 9999));
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Debug.Log("Connected to Master");
            pendingAction?.Invoke();
            PhotonNetwork.AutomaticallySyncScene = false;
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
            base.OnJoinedRoom();
            SceneManager.LoadScene("RoomLobby");
        }

        private void OnConnectionDependentActionClicked(Action action)
        {
            if(pendingAction != null)
            {
                return;
            }

            pendingAction = action;

            if (PhotonNetwork.IsConnectedAndReady)
            {
                action();
            }
        }

        private void GoToLobbyList()
        {
            SceneManager.LoadSceneAsync("LobbyList");
        }
    }
}
