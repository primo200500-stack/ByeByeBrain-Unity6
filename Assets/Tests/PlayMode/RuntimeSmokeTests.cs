using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ByeByeBrain;

public class RuntimeSmokeTests
{
    [UnitySetUp]
    public IEnumerator WaitForRuntimeBootstrap()
    {
        // RuntimeInitializeOnLoadMethod creates the prototype after the scene loads.
        yield return null;
        yield return null;
        Assert.IsNotNull(GameBootstrap.Instance, "Runtime bootstrap did not create GameBootstrap.");
    }

    [UnityTest]
    public IEnumerator BootstrapCreatesPlayerHudAndFirstWave()
    {
        yield return null;
        Assert.IsNotNull(GameBootstrap.Instance.Player, "Player was not created.");
        Assert.IsNotNull(GameBootstrap.Instance.HUD, "HUD was not created.");
        Assert.GreaterOrEqual(GameBootstrap.Instance.Wave, 1, "Wave counter should start at one or higher.");
    }

    [UnityTest]
    public IEnumerator PlayerDamageReducesHealth()
    {
        var player = GameBootstrap.Instance.Player.GetComponent<PlayerController>();
        int before = player.health;
        player.TakeDamage(10);
        Assert.AreEqual(Mathf.Max(0, before - 10), player.health);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ScoreCanIncrease()
    {
        int before = GameBootstrap.Instance.Score;
        GameBootstrap.Instance.AddScore(25);
        Assert.AreEqual(before + 25, GameBootstrap.Instance.Score);
        yield return null;
    }

    [UnityTest]
    public IEnumerator RescueBeaconCanBeUsedAtCloseRange()
    {
        int before = GameBootstrap.Instance.Rescued;
        Transform player = GameBootstrap.Instance.Player;
        Vector3 original = player.position;
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller) controller.enabled = false;
        player.position = new Vector3(0, 1, 12);
        GameBootstrap.Instance.UseRescue();
        Assert.AreEqual(before + 1, GameBootstrap.Instance.Rescued);
        player.position = original;
        if (controller) controller.enabled = true;
        yield return null;
    }
}
