using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        private bool allPlayersReady => lobbyEntries.Values.ToList().TrueForAll(entry => entry.IsPlayerReady);

        private void AddLobbyEntry(Player player)
        {
            var entry = Instantiate(playerLobbyEntryPrefab, entriesHolder);
            entry.Setup(player);

            // TODO (DONE): track created player lobby entries
            lobbyEntries.Add(player, entry);
        }

        private void Start()
        {
            LoadingGraphics.Disable();
            DestroyHolderChildren();

            closeButton.onClick.AddListener(OnCloseButtonClicked);
            startButton.onClick.AddListener(OnStartButtonClicked);
            startButton.gameObject.SetActive(false);

            lobbyEntries = new Dictionary<Player, PlayerLobbyEntry>(PhotonNetwork.CurrentRoom.MaxPlayers);

            foreach(var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                AddLobbyEntry(player);
            }
        }

        // add lobby and start button updates for entering and exiting
        public override void OnPlayerEnteredRoom(Player otherPlayer)
        {
            AddLobbyEntry(otherPlayer);
            UpdateStartButton();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Destroy(lobbyEntries[otherPlayer]);
            lobbyEntries.Remove(otherPlayer);
            UpdateStartButton();
        }

        // update changes to player properties
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            lobbyEntries[targetPlayer].UpdateVisuals();

            UpdateStartButton();
        }

        private void UpdateStartButton()
        {
            // TODO: Show start button only to the master client and when all players are ready
            startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && allPlayersReady);
            
        }

        private void OnStartButtonClicked()
        {
            // TODO: Load gameplay level for all clients
        }

        private void OnCloseButtonClicked()
        {
            // TODO: Leave room
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