using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LabTesting.Tests
{
    public class SceneSmokeTests
    {
        private const string SceneName = "SampleScene";
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";

        [UnityTest]
        public IEnumerator SampleScene_IsInBuildSettingsAndLoads()
        {
            SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
            yield return null;

            Scene scene = SceneManager.GetSceneByName(SceneName);

            Assert.That(scene.IsValid(), Is.True, $"'{SceneName}' is not a valid scene.");
            Assert.That(scene.isLoaded, Is.True, $"'{ScenePath}' failed to load.");
            Assert.That(scene.name, Is.EqualTo(SceneName));
        }

        [UnityTest]
        public IEnumerator SampleScene_HasCameraAndLight()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);

            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

            Assert.That(cameras, Is.Not.Empty, "Scene has no camera; nothing would render.");
            Assert.That(lights, Is.Not.Empty, "Scene has no light.");

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator LabComponents_RunWithoutErrorsInLoadedScene()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);

            var player = new GameObject("SmokeTestPlayer", typeof(CharacterController));
            var controller = player.AddComponent<PlayerController>();
            var health = player.AddComponent<Health>();
            health.SetMaxHP(50);
            health.ResetHP();

            var hudRoot = new GameObject("SmokeTestHud", typeof(RectTransform));
            var hud = hudRoot.AddComponent<HealthHud>();
            hud.Bind(health);

            var frameTimes = new float[5];
            for (int i = 0; i < 5; i++)
            {
                yield return null;
                frameTimes[i] = Time.deltaTime;
                controller.Move(Vector2.right);
            }

            Assert.That(health.CurrentHP, Is.EqualTo(50));

            health.Damage(20);
            yield return null;

            Assert.That(health.CurrentHP, Is.EqualTo(30));
            Assert.That(hud.DisplayedText, Is.EqualTo("30/50"));
            Assert.That(frameTimes.Any(t => t < 0f), Is.False, "Time.deltaTime should never be negative.");

            Object.DestroyImmediate(player);
            Object.DestroyImmediate(hudRoot);

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator HealthStaysNonNegative_DuringPlayModeUpdates()
        {
            yield return null;

            var go = new GameObject("InvariancePlayer");
            var health = go.AddComponent<Health>();
            health.SetMaxHP(80);

            var hudRoot = new GameObject("InvarianceHud", typeof(RectTransform));
            var hud = hudRoot.AddComponent<HealthHud>();
            hud.Bind(health);

            for (int i = 0; i < 20; i++)
            {
                health.Damage(7);
                yield return null;

                Assert.That(health.CurrentHP, Is.InRange(0, health.MaxHP), $"HP escaped range on frame {i}.");
                Assert.That(hud.DisplayedText, Is.EqualTo($"{health.CurrentHP}/{health.MaxHP}"));
            }

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(hudRoot);

            LogAssert.NoUnexpectedReceived();
        }
    }
}
