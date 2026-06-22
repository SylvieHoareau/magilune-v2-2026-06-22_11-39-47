using UnityEngine;

namespace VerdantHeart
{
    public class DeadZone : MonoBehaviour
    {
        [SerializeField] Transform startingPosition;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                collision.GetComponent<PlayerStateMachine>().gameObject.transform.position = startingPosition.position;
        }
    }
}