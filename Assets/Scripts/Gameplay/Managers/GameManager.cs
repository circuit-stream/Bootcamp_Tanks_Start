using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;


namespace Tanks
{
    [Serializable]
    public class TeamConfig
    {
        public Transform spawnPoint;
        public Color color;
    }

    public class GameManager : MonoBehaviourPunCallbacks
    {
        private const float MAX_DEPENETRATION_VELOCITY = float.PositiveInfinity;
        private const int ROUND_START_PHOTON_EVENT = 1;

        [Header("Balance")]
        [SerializeField] private int numRoundsToWin = 5;
        [SerializeField] private float startDelay = 3f;
        [SerializeField] private float endDelay = 3f;

        [Header("References")]
        [SerializeField] private CameraController cameraController;
        [SerializeField] private Text messageText;
        [SerializeField] private GameObject tankPrefab;
        [SerializeField] private TeamConfig[] teamConfigs;

        private List<TankManager> tankManagers;

        private int roundNumber;
        private TankManager roundWinner;
        private TankManager gameWinner;

        public TeamConfig RegisterTank(TankManager tankManager, int team)
        {
            tankManagers.Add(tankManager);
            cameraController.targets.Add(tankManager.transform);
            return teamConfigs[team];
        }

        private void Start()
        {
            Physics.defaultMaxDepenetrationVelocity = MAX_DEPENETRATION_VELOCITY;

            tankManagers = new List<TankManager>();
            SpawnPlayerTank();

            StartRound();
        }

        private void SpawnPlayerTank()
        {
            // TODO (DONE): Get team from photon
            var team = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"];

            var config = teamConfigs[team];
            var spawnPoint = config.spawnPoint;

            PhotonNetwork.Instantiate(tankPrefab.name, spawnPoint.position, spawnPoint.rotation);
        }

        private void StartRound()
        {
            StartCoroutine(RoundStarting());
        }

        private IEnumerator RoundStarting()
        {
            ResetAllTanks();
            DisableTankControl();

            cameraController.SetStartPositionAndSize();

            roundNumber++;
            messageText.text = $"ROUND {roundNumber}";

            yield return new WaitForSeconds(startDelay);

            EnableTankControl();
            messageText.text = string.Empty;
        }

        private IEnumerator RoundEnding()
        {
            PhotonNetwork.LeaveRoom();

            DisableTankControl();

            roundWinner = null;
            roundWinner = GetRoundWinner();

            if (roundWinner != null) roundWinner.Wins++;

            gameWinner = GetGameWinner();

            messageText.text = EndMessage();

            // NOTE: The null check on gameWinner is done before calling WaitForSeconds
            // because the object will be null by the time the delay is finished

            if (gameWinner != null)
            {
                yield return new WaitForSeconds(endDelay);
                // TODO (DONE): Leave photon room
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                yield return new WaitForSeconds(endDelay);
                StartRound();
            } 
                
        }

        private bool OneTankLeft()
        {
            int numTanksLeft = 0;

            foreach (var tankManager in tankManagers)
            {
                if (tankManager.gameObject != null && tankManager.gameObject.activeSelf)
                    numTanksLeft++;
            }

            return numTanksLeft <= 1;
        }

        private TankManager GetRoundWinner()
        {
            foreach (var tankManager in tankManagers)
            {
                if (tankManager.gameObject.activeSelf)
                    return tankManager;
            }

            return null;
        }

        private TankManager GetGameWinner()
        {

            foreach (var tankManager in tankManagers)
            {
                if (tankManager.gameObject != null)
                {
                    if (tankManager.Wins == numRoundsToWin)
                        return tankManager;
                }

            }

            if (tankManagers.Count == 1)
            {
                return tankManagers[0];
            }

            return null;
        }

        private string EndMessage()
        {
            string message = "DRAW!";

            if (roundWinner != null)
                message = $"{roundWinner.ColoredPlayerName} WINS THE ROUND!";

            message += "\n\n\n\n";

            foreach (var tankManager in tankManagers)

            {
                message += $"{tankManager.ColoredPlayerName}: {tankManager.Wins} WINS\n";
            }

            if (gameWinner != null)
                message = $"{gameWinner.ColoredPlayerName} WINS THE GAME!";

            return message;
        }

        private void ResetAllTanks()
        {
            
            foreach (var tankManager in tankManagers)
            {
                if(tankManager.GetComponent<TankManager>() != null)
                {
                    tankManager.Reset();
                }
                
            }
                
        }

        private void EnableTankControl()
        {
            foreach (var tankManager in tankManagers)
                tankManager.EnableControl();
        }

        private void DisableTankControl()
        {
            foreach (var tankManager in tankManagers)
                tankManager.DisableControl();
        }

        private IEnumerator HandleTankDeath()
        {
            yield return null; // Allow TankHealth to process the PhotonEvent first.

            if (!OneTankLeft()) yield break;

            StartCoroutine(RoundEnding());
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);

        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);

        }

        public void OnEvent(EventData photonEvent)
        {
            if(photonEvent.Code == TankHealth.TANK_DIED_PHOTON_EVENT)
            {
                StartCoroutine(HandleTankDeath());
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {

            TankManager otherTank = null;

            // stop tracking the tank (managers and camera)
            foreach (var tankManager in tankManagers)
            {
                if (tankManager.photonView.ControllerActorNr == otherPlayer.ActorNumber)
                {
                    otherTank = tankManager;
                }
            }

            if (otherTank != null)
            {
                cameraController.targets.Remove(otherTank.transform);
                tankManagers.Remove(otherTank);
            }

            // finish game if only one tank left
            if (OneTankLeft())
            {
                StartCoroutine(RoundEnding());
            }

        }
    }
}