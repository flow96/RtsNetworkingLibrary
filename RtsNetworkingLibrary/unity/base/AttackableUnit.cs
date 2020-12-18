using System.Security.Cryptography;
using RtsNetworkingLibrary.unity.attributes;
using UnityEngine;

namespace RtsNetworkingLibrary.unity.@base
{
    public abstract class AttackableUnit : NetworkMonoBehaviour
    {
        [Header("Stats")]
        [SyncVar]
        public float health = 100;
        [SyncVar]
        public float maxHealth = 100;
        [SyncVar]
        public float attackDamage = 3;
        [SyncVar]
        public float attackRange = 10;
        [SyncVar]
        public float attackSpeed = 5;

        protected float attackCoolDown = 0;

        public void TakeDamage(float damage)
        {
            health -= damage;
            Debug.Log("Taking damage: " + health);
            if(health <= 0)
                Destroy();
        }

        protected abstract void Destroy();
    }
}