using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LabTesting
{
    [DisallowMultipleComponent]
    [AddComponentMenu("LabTesting/Health HUD")]
    public class HealthHud : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Slider healthBar;

        private bool subscribed;

        public Health Health => health;
        public string DisplayedText { get; private set; } = string.Empty;
        public float DisplayedNormalized { get; private set; }

        private void OnEnable()
        {
            if (health != null)
            {
                Subscribe();
            }

            Refresh();
        }

        private void OnDisable() => Unsubscribe();

        public void Bind(Health target)
        {
            Unsubscribe();
            health = target;
            Subscribe();
            Refresh();
        }

        public void SetLabel(TMP_Text value) => label = value;

        public void SetHealthBar(Slider value) => healthBar = value;

        public void Refresh()
        {
            if (health == null)
            {
                DisplayedText = "--/--";
                DisplayedNormalized = 0f;
            }
            else
            {
                DisplayedText = $"{health.CurrentHP}/{health.MaxHP}";
                DisplayedNormalized = health.Normalized;
            }

            if (label != null)
            {
                label.text = DisplayedText;
            }

            if (healthBar != null)
            {
                healthBar.SetValueWithoutNotify(DisplayedNormalized);
            }
        }

        private void Subscribe()
        {
            if (health == null || subscribed)
            {
                return;
            }

            health.Changed += OnHealthChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (health != null)
            {
                health.Changed -= OnHealthChanged;
            }

            subscribed = false;
        }

        private void OnHealthChanged(int current, int max) => Refresh();
    }
}
