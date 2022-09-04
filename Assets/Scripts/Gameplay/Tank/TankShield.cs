using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShield : MonoBehaviour, IPunObservable
{
    private const string ROTATE_SHIELD_BUTTON = "RotateShield";

    [SerializeField]
    private float rotationSpeed = 150;

    private PhotonView photonView;
    private float CurrentRotation => transform.rotation.eulerAngles.y;


    void Start()
    {
        photonView = GetComponentInParent<PhotonView>();
    }


    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        var rotationInput = Input.GetAxis(ROTATE_SHIELD_BUTTON);
        var newRotation = CurrentRotation + rotationInput * Time.deltaTime * rotationSpeed;

        SetRotation(newRotation);
    }

    private void SetRotation(float rotation)
    {
        transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(CurrentRotation);
        }
        else
        {
            var newRotation = (float)stream.ReceiveNext();
            SetRotation(newRotation);
        }
    }
}
