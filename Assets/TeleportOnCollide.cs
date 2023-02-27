using UnityEngine;
using KinematicCharacterController;

public class TeleportOnCollide : MonoBehaviour
{
    public GameObject teleportTarget;
    public Transform teleportPosition;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.name);
        Debug.Log("Trigger");
        teleportTarget.GetComponent<KinematicCharacterMotor>().SetPosition(teleportPosition.position, true);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player position: " + other.transform.position + "\n" + "Target position: " + teleportTarget.transform.position);
            other.transform.position = new Vector3(0, 0, 0);
            // other.transform.position = teleporttarget.transform.position;
        }
    }
}