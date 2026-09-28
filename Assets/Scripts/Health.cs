using System;
using UnityEngine;

namespace LabTesting
{
    [DisallowMultipleComponent]
    [AddComponentMenu("LabTesting/Health")]
    public class Health : MonoBehaviour
    {
        [SerializeField, Min(0)] private int maxHP = 100;
        [SerializeField, Min(0)] private int currentHP = 100;

        private bool initialized;

        public event Action<int, int> Changed;
        public event Action Died;

        public int MaxHP
        {
            get
            {
                EnsureInitialized();
                return maxHP;
            }
        }

        public int CurrentHP
        {
            get
            {
                EnsureInitialized();
                return currentHP;
            }
        }

        public bool IsAlive => CurrentHP > 0;

        public float Normalized => MaxHP <= 0 ? 0f : (float)CurrentHP / MaxHP;

        private void Awake() => EnsureInitialized();

        private void OnValidate()
        {
            maxHP = Mathf.Max(0, maxHP);
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            maxHP = Mathf.Max(0, maxHP);
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        }

        public void SetMaxHP(int value)
        {
            EnsureInitialized();
            int clampedMax = Mathf.Max(0, value);
            bool maxChanged = clampedMax != maxHP;
            int previous = currentHP;

            maxHP = clampedMax;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            if (maxChanged || previous != currentHP)
            {
                Changed?.Invoke(currentHP, maxHP);
            }

            if (previous > 0 && currentHP <= 0)
            {
                Died?.Invoke();
            }
        }

        public void Damage(int amount) => ApplyDelta(-Mathf.Max(0, amount));

        public void Heal(int amount) => ApplyDelta(Mathf.Max(0, amount));

        public void SetHP(int value)
        {
            EnsureInitialized();
            int previous = currentHP;
            currentHP = Mathf.Clamp(value, 0, maxHP);

            if (previous == currentHP)
            {
                return;
            }

            Changed?.Invoke(currentHP, maxHP);

            if (previous > 0 && currentHP <= 0)
            {
                Died?.Invoke();
            }
        }

        public void ResetHP() => SetHP(maxHP);

        private void ApplyDelta(int delta)
        {
            EnsureInitialized();
            int previous = currentHP;
            currentHP = Mathf.Clamp(currentHP + delta, 0, maxHP);

            if (previous == currentHP)
            {
                return;
            }

            Changed?.Invoke(currentHP, maxHP);

            if (previous > 0 && currentHP <= 0)
            {
                Died?.Invoke();
            }
        }
    }
}
