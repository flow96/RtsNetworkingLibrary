using UnityEngine;

namespace RtsNetworkingLibrary.unity.@base
{
    public abstract class BaseUnit : AttackableUnit
    {

        public float movingSpeed = 10;
        [Header("Healthbar")] public GameObject healthBarCanvas;
        public RectTransform foregroundBar;
    }
}