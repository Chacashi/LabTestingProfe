using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace LabTesting.Tests
{
    public class HealthEditModeTests
    {
        private readonly List<GameObject> created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int i = created.Count - 1; i >= 0; i--)
            {
                if (created[i] != null)
                {
                    Object.DestroyImmediate(created[i]);
                }
            }

            created.Clear();
        }

        private Health CreateHealth(int maxHP)
        {
            var go = new GameObject("HealthUnderTest");
            created.Add(go);
            var health = go.AddComponent<Health>();
            health.SetMaxHP(maxHP);
            health.SetHP(maxHP);
            return health;
        }

        [Test]
        public void NewHealth_StartsFull()
        {
            Health health = CreateHealth(100);

            Assert.That(health.MaxHP, Is.EqualTo(100));
            Assert.That(health.CurrentHP, Is.EqualTo(100));
            Assert.That(health.IsAlive, Is.True);
            Assert.That(health.Normalized, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Damage_ReducesCurrentHP()
        {
            Health health = CreateHealth(100);

            health.Damage(30);

            Assert.That(health.CurrentHP, Is.EqualTo(1));
        }

        [Test]
        public void HP_IsNeverNegative_WhenDamageExceedsMax()
        {
            Health health = CreateHealth(50);

            health.Damage(9999);

            Assert.That(health.CurrentHP, Is.GreaterThanOrEqualTo(0));
            Assert.That(health.CurrentHP, Is.EqualTo(0));
            Assert.That(health.IsAlive, Is.False);
            Assert.That(health.Normalized, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void HP_IsNeverNegative_AcrossRepeatedOverkillDamage()
        {
            Health health = CreateHealth(10);
            int deathCount = 0;
            health.Died += () => deathCount++;

            for (int i = 0; i < 50; i++)
            {
                health.Damage(7);

                Assert.That(health.CurrentHP, Is.GreaterThanOrEqualTo(0), $"HP became negative on hit {i}.");
                Assert.That(health.CurrentHP, Is.LessThanOrEqualTo(health.MaxHP), $"HP exceeded max on hit {i}.");
            }

            Assert.That(health.CurrentHP, Is.EqualTo(0));
            Assert.That(deathCount, Is.EqualTo(1), "Died must fire only on the transition to zero.");
        }

        [Test]
        public void HP_NeverExceedsMax_WhenHealingPastFull()
        {
            Health health = CreateHealth(40);

            health.Damage(10);
            health.Heal(100);

            Assert.That(health.CurrentHP, Is.EqualTo(40));
            Assert.That(health.CurrentHP, Is.LessThanOrEqualTo(health.MaxHP));
        }

        [Test]
        public void Heal_FromZero_ReturnsToPositive()
        {
            Health health = CreateHealth(20);

            health.Damage(20);
            Assert.That(health.CurrentHP, Is.EqualTo(0));

            health.Heal(5);

            Assert.That(health.CurrentHP, Is.EqualTo(5));
            Assert.That(health.IsAlive, Is.True);
        }

        [Test]
        public void NegativeAmounts_AreTreatedAsNoOp()
        {
            Health health = CreateHealth(60);

            health.Damage(-25);
            Assert.That(health.CurrentHP, Is.EqualTo(60));

            health.Heal(-25);
            Assert.That(health.CurrentHP, Is.EqualTo(60));
        }

        [Test]
        public void SetHP_IsClampedToRange()
        {
            Health health = CreateHealth(30);

            health.SetHP(-500);
            Assert.That(health.CurrentHP, Is.EqualTo(0));

            health.SetHP(500);
            Assert.That(health.CurrentHP, Is.EqualTo(30));
        }

        [Test]
        public void SetMaxHP_ClampsCurrentHP()
        {
            Health health = CreateHealth(100);

            health.SetMaxHP(25);

            Assert.That(health.MaxHP, Is.EqualTo(25));
            Assert.That(health.CurrentHP, Is.EqualTo(25));
            Assert.That(health.CurrentHP, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void MaxHP_CannotBecomeNegative()
        {
            Health health = CreateHealth(100);

            health.SetMaxHP(-80);

            Assert.That(health.MaxHP, Is.EqualTo(0));
            Assert.That(health.CurrentHP, Is.GreaterThanOrEqualTo(0));
            Assert.That(health.Normalized, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void ResetHP_RestoresFullHealth()
        {
            Health health = CreateHealth(15);

            health.Damage(15);
            health.ResetHP();

            Assert.That(health.CurrentHP, Is.EqualTo(15));
        }

        [Test]
        public void Changed_ReportsCurrentAndMax()
        {
            Health health = CreateHealth(100);
            int lastCurrent = -1;
            int lastMax = -1;
            health.Changed += (current, max) =>
            {
                lastCurrent = current;
                lastMax = max;
            };

            health.Damage(25);

            Assert.That(lastCurrent, Is.EqualTo(75));
            Assert.That(lastMax, Is.EqualTo(100));
        }

        [Test]
        public void Changed_DoesNotFire_WhenValueIsUnchanged()
        {
            Health health = CreateHealth(10);
            int changes = 0;
            health.Changed += (current, max) => changes++;

            health.Damage(0);
            health.Heal(0);
            health.Damage(100);
            health.Damage(100);

            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void RandomizedOperations_NeverBreakInvariants()
        {
            Health health = CreateHealth(120);
            var random = new System.Random(20260928);

            for (int i = 0; i < 2000; i++)
            {
                int amount = random.Next(-200, 200);

                if (random.Next(0, 3) == 0)
                {
                    health.Damage(amount);
                }
                else
                {
                    health.Heal(amount);
                }

                Assert.That(health.CurrentHP, Is.InRange(0, health.MaxHP), $"Invariant broken at iteration {i}.");
            }
        }
    }
}
