using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Linq;

namespace Tanks
{
    public class RoomLobbyController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private PlayerLobbyEntry playerLobbyEntryPrefab;
        [SerializeField] private RectTransform entriesHolder;

        // TODO: Create and Delete player entries
        private Dictionary<Player, PlayerLobbyEntry> lobbyEntries;

        private bool IsEveryPlayerReady => lobbyEntries.Values.ToList().TrueForAll(entry => entry.IsPlayerReady);

        private void AddLobbyEntry(Player player)
        {
            var entry = Instantiate(playerLobbyEntryPrefab, entriesHolder);
            entry.Setup(player);

            // TODO: track created player lobby entries

            lobbyEntries.Add(player, entry);
        }

        private void RemoveLobbyEntry(Player player)
        {
            lobbyEntries.Remove(player);
            Destroy(playerLobbyEntryPrefab);
        }

        private void Start()
        {
            LoadingGraphics.Disable();
            DestroyHolderChildren();

            closeButton.onClick.AddListener(OnCloseButtonClicked);
            startButton.onClick.AddListener(OnStartButtonClicked);
            startButton.gameObject.SetActive(false);

            PhotonNetwork.AutomaticallySyncScene = true;

            lobbyEntries = new Dictionary<Player, PlayerLobbyEntry>(PhotonNetwork.CurrentRoom.MaxPlayers);
            foreach(var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                AddLobbyEntry(player);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            //base.OnPlayerEnteredRoom(newPlayer);  base doesn't do anything
            AddLobbyEntry(newPlayer);
            UpdateStartButton();

        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            // base.OnPlayerLeftRoom(otherPlayer); base doesn't do anything
            Destroy(lobbyEntries[otherPlayer].gameObject);
            lobbyEntries.Remove(otherPlayer);
            UpdateStartButton();
        }

        private void UpdateStartButton()
        {
            // TODO: Show start button only to the master client and when all players are ready
            startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && IsEveryPlayerReady);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            //base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            lobbyEntries[targetPlayer].UpdateVisuals();
            UpdateStartButton();

        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            // base.OnMasterClientSwitched(newMasterClient);
            UpdateStartButton();
        }

        private void OnStartButtonClicked()
        {
            // TODO: Load gameplay level for all clients
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("You fool! Trying to start game while not MasterClient!");
                return;
            }
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("Gameplay");
        }

        private void OnCloseButtonClicked()
        {
            // TODO: Leave room
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("MainMenu");
        }

        private void DestroyHolderChildren()
        {
            for (var i = entriesHolder.childCount - 1; i >= 0; i--) {
                Destroy(entriesHolder.GetChild(i).gameObject);
            }
        }
    }
}