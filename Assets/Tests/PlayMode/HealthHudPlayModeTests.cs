using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LabTesting.Tests
{
    public class HealthHudPlayModeTests
    {
        private GameObject player;
        private GameObject hudRoot;
        private Health health;
        private HealthHud hud;
        private Slider bar;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Player");
            health = player.AddComponent<Health>();
            health.SetMaxHP(100);

            hudRoot = new GameObject("Hud", typeof(RectTransform), typeof(CanvasRenderer), typeof(Slider));
            bar = hudRoot.GetComponent<Slider>();
            hud = hudRoot.AddComponent<HealthHud>();
            hud.SetHealthBar(bar);
            hud.Bind(health);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(hudRoot);
        }

        [UnityTest]
        public IEnumerator Hud_ShowsCurrentAndMaxHP()
        {
            yield return null;

            Assert.That(hud.DisplayedText, Is.EqualTo("100/100"));
        }

        [UnityTest]
        public IEnumerator Hud_UpdatesWhenPlayerTakesDamage()
        {
            yield return null;

            health.Damage(40);
            yield return null;

            Assert.That(hud.DisplayedText, Is.EqualTo("60/100"));
            Assert.That(bar.value, Is.EqualTo(0.6f).Within(0.0001f));
        }

        [UnityTest]
        public IEnumerator Hud_ShowsZeroAndEmptyBar_AtDeath()
        {
            yield return null;

            health.Damage(500);
            yield return null;

            Assert.That(health.CurrentHP, Is.EqualTo(0));
            Assert.That(hud.DisplayedText, Is.EqualTo("0/100"));
            Assert.That(bar.value, Is.EqualTo(0f).Within(0.0001f));
        }

        [UnityTest]
        public IEnumerator Hud_TracksHealingBackToFull()
        {
            health.Damage(100);
            yield return null;

            health.Heal(75);
            yield return null;

            Assert.That(hud.DisplayedText, Is.EqualTo("75/100"));
            Assert.That(bar.value, Is.EqualTo(0.75f).Within(0.0001f));

            health.Heal(1000);
            yield return null;

            Assert.That(hud.DisplayedText, Is.EqualTo("100/100"));
            Assert.That(bar.value, Is.EqualTo(1f).Within(0.0001f));
        }

        [UnityTest]
        public IEnumerator Hud_StaysConsistent_OverRepeatedHits()
        {
            yield return null;

            for (int i = 0; i < 25; i++)
            {
                health.Damage(9);
                yield return null;

                Assert.That(health.CurrentHP, Is.GreaterThanOrEqualTo(0), $"HP went negative on hit {i}.");
                Assert.That(hud.DisplayedText, Is.EqualTo($"{health.CurrentHP}/{health.MaxHP}"),
                    $"HUD desynced on hit {i}.");
                Assert.That(bar.value, Is.EqualTo(health.Normalized).Within(0.0001f));
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Hud_DropsSubscriptionWhenDisabled()
        {
            yield return null;

            hudRoot.SetActive(false);
            health.Damage(10);
            string whileDisabled = hud.DisplayedText;

            hudRoot.SetActive(true);
            yield return null;

            Assert.That(whileDisabled, Is.EqualTo("100/100"), "Disabled HUD must not react to changes.");
            Assert.That(hud.DisplayedText, Is.EqualTo("90/100"), "Re-enabled HUD must catch up once.");
        }
    }
}
