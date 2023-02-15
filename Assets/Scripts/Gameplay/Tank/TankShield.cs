using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShield : MonoBehaviour, IPunObservable
{
    private const string ROTATE_SHIELD_BUTTON = "RotateShield";

    [SerializeField] private float rotationSpeed;

    private PhotonView photonView;

    private float currentRotation => transform.rotation.eulerAngles.y;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponentInParent<PhotonView>();
    }

    private void Update()
    {
        ShieldRotation();
    }

    private void ShieldRotation()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        float shieldRotation = Input.GetAxisRaw(ROTATE_SHIELD_BUTTON);

        SetRotation(currentRotation + shieldRotation * Time.deltaTime * rotationSpeed);
    }

    private void SetRotation(float newYRotation)
    {
        transform.rotation = Quaternion.Euler(0, newYRotation, 0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentRotation);
        } else
        {
            var newYRotation = (float)stream.ReceiveNext();
            SetRotation(newYRotation);
        }
    }
}
