using System;
using UnityEngine;

public class RoomTone : MonoBehaviour
{
    public string roomToneName;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GameManager.Instance.mainCamera.gameObject)
        {
            AudioManager.Instance.Transition(AudioManager.Instance.currentRoomTone, roomToneName, 1.5f);
            AudioManager.Instance.currentRoomTone = roomToneName;
        }
    }
}
