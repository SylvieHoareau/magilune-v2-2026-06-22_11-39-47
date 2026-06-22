using System;
using UnityEngine;

namespace VerdantHeart
{
    public class Ground : MonoBehaviour
    {
        public static event Action PlayerLanded;
        public static event Action PlayerLeftGround;

        bool onGround;
        float friction;

        // GETTERS
        public bool OnGround => onGround;
        public float Friction => friction;

        // Check on and during collision if character is grounded.
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision != null)
            {
                EvaluateCollision(collision);
                RetrieveFriction(collision);

                if (onGround)
                    PlayerLanded?.Invoke();
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision != null)
            {
                EvaluateCollision(collision);
                RetrieveFriction(collision);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (onGround)
                PlayerLeftGround?.Invoke();

            onGround = false;
            friction = 0f;
        }

        void EvaluateCollision(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector2 normal = collision.GetContact(i).normal;
                onGround |= normal.y >= 0.9f; // flat ground = 1f. So testing if the player lands on a mostly flat surface
            }
        }

        void RetrieveFriction(Collision2D collision)
        {
            PhysicsMaterial2D material = null;

            if (collision.rigidbody != null)
                material = collision.rigidbody.sharedMaterial;

            friction = 0; // set friction to zero for default

            if (material != null)
                friction = material.friction;
        }
    }
}