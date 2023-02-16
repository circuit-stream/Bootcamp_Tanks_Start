using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class TankShooting : MonoBehaviour
    {
        private const string FIRE_BUTTON = "Fire1";
        private const string HOMING_MISSILE_BUTTON = "Fire2";
        private const string AIRSTRIKE_BUTTON = "Fire3";

        public const int CHARGING_UP_SHOOTING = 2;

        public Rigidbody shell;
        public Transform fireTransform;
        public Slider aimSlider;
        public AudioSource shootingAudio;
        public AudioClip chargingClip;
        public AudioClip fireClip;
        public float minLaunchForce = 15f;
        public float maxLaunchForce = 30f;
        public float maxChargeTime = 0.75f;
        public float homingMissileInstantiateOffset = 4f;

        private PhotonView photonView;

        private float currentLaunchForce;
        private float chargeSpeed;
        private bool fired;

        private void OnEnable()
        {
            currentLaunchForce = minLaunchForce;
            aimSlider.value = minLaunchForce;
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void Start()
        {
            photonView = GetComponent<PhotonView>();

            chargeSpeed = (maxLaunchForce - minLaunchForce) / maxChargeTime;
        }

        private void Update()
        {
            // Only allow owner of this tank to shoot
            if (!photonView.IsMine)
            {
                return;
            }

            aimSlider.value = minLaunchForce;

            if (currentLaunchForce >= maxLaunchForce && !fired)
            {
                currentLaunchForce = maxLaunchForce;
                Fire();
            }
            else if (Input.GetButtonDown(FIRE_BUTTON))
            {
                fired = false;
                currentLaunchForce = minLaunchForce;

                shootingAudio.clip = chargingClip;
                shootingAudio.Play();
            }
            else if (Input.GetButton(FIRE_BUTTON) && !fired)
            {
                currentLaunchForce += chargeSpeed * Time.deltaTime;

                photonView.RPC(
                    "OnUpdateChargingAmount",
                    RpcTarget.All,
                    currentLaunchForce);
            }
            else if (Input.GetButtonUp(FIRE_BUTTON) && !fired)
            {
                Fire();
                photonView.RPC("ResetChargeAmount", RpcTarget.All);
            }

            photonView.RPC("TryFireAirstrike", RpcTarget.All);
        }

        private void TryFireHomingMissile()
        {
            if (!Input.GetButtonDown(HOMING_MISSILE_BUTTON))
            {
                return;
            }

            if (!GetClickPosition(out var clickPosition))
            {
                return;
            }

            Collider[] colliders = Physics.OverlapSphere(clickPosition, 5, LayerMask.GetMask("Players"));

            foreach(var tankCollider in colliders)
            {
                if(tankCollider.gameObject == gameObject)
                {
                    continue;
                }

                var direction = (tankCollider.transform.position - transform.position).normalized;

                var position = transform.position + direction * homingMissileInstantiateOffset + Vector3.up;

                object[] data =
                {
                    tankCollider.GetComponent<PhotonView>().ViewID
                };

                PhotonNetwork.Instantiate(
                    nameof(HomingMissile),
                    position,
                    Quaternion.LookRotation(transform.forward),
                    0,
                    data);
            }
        }

        [PunRPC]
        private void TryFireAirstrike()
        {
            if (!Input.GetButtonDown(AIRSTRIKE_BUTTON)) { return; }
            if (!GetClickPosition(out var clickPosition)) { return; }

            var position = clickPosition;

            PhotonNetwork.Instantiate(
                nameof(AirStrike),
                position,
                Quaternion.identity,
                0);
        }

        private bool GetClickPosition(out Vector3 clickPosition)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var gotHit = Physics.Raycast(ray, out var hit, 1000, LayerMask.GetMask("Default"));

            clickPosition = gotHit ? hit.point : Vector3.zero;
            return gotHit;
        }

        private void Fire()
        {
            fired = true;

            // Instantiate the projectile on all clients
            photonView.RPC
                (
                "Fire",
                RpcTarget.All,
                fireTransform.position,
                fireTransform.rotation,
                currentLaunchForce * fireTransform.forward
                );

            currentLaunchForce = minLaunchForce;
        }

        [PunRPC]
        private void Fire(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            Rigidbody shellInstance = Instantiate(shell, position, rotation);
            shellInstance.velocity = velocity;

            shootingAudio.clip = fireClip;
            shootingAudio.Play();

        }

        [PunRPC]
        private void OnUpdateChargingAmount(float launchForce)
        {
            aimSlider.value = launchForce;
        }

        [PunRPC]
        private void ResetChargeAmount()
        {
            aimSlider.value = 0;
        }

    }
}