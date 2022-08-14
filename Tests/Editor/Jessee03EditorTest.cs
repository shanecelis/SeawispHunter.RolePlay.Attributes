using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SeawispHunter.RolePlay.Attributes;

namespace SeawispHunter.RolePlay.Attributes.Tests {
  /** Jessee03 writes[1]:

      I've always had an issue with this and still haven't found the best way to
      handle it. How do you apply buffs and debuffs quickly and easily? They
      should be able to applied to either the player or any enemies.

      Example:

      Debuff
      Type: poison
      Description: Does 10 damage every second for 5 seconds.

      Now for the confusing part. Being able to apply multiples of the same
      debuff on a single target, removing specific debuffs ( poison, ice, ect.
      ), and adjusting damage taken from these debuffs compared to resistances
      such as poison resistance.

      [1]: https://forum.unity.com/threads/rpg-buffs-and-debuffs.188882/
    */
  public class Jessee03EditorTest {// A Test behaves as an ordinary method
    IModifiableValue<float>[] resistances = new IModifiableValue<float>[4];
    IModifiableValue<float> maxHealth = new ModifiableValue<float>(100f);
    IModifiableValue<float> health;
    IModifier<float> poison;
    IEnumerator coroutine;
    CancellationTokenSource tokenSource;

    [Flags]
    internal enum DamageType {
      None   = 0,
      Fire   = 1,
      Cold   = 2,
      Poison = 4,
      All    = 7,
    }

    internal class FightContext {
      public DamageType vulnerable;
      public DamageType incoming;
      public bool targetIsUndead;
    }

    [SetUp] public void SetUp() {
      for (int i = 0; i <resistances.Length; i++)
        resistances[i] = new ModifiableValue<float>(1f);
      tokenSource = new CancellationTokenSource();
      poison = Poison(1f, 1f, 10, out coroutine, tokenSource.Token);
      health.initial.value = maxHealth.value;
    }

    [TearDown] public void TearDown() {
      health.modifiers.Clear();
      maxHealth.modifiers.Clear();
    }

    public Jessee03EditorTest() {
      // health = ModifiableValue.FromValue(maxHealth);
      var healthValue = new BoundedValue<float>(maxHealth.value, 0f, maxHealth);
      health = new ModifiableIValue<float>(healthValue);
    }

    IModifier<DamageType,float> Poison(float damagePerPeriod,
                                       float period,
                                       int times,
                                       out IEnumerator coroutine,
                                       CancellationToken token = default) {
      Value<float> damage = new Value<float>(0f);
      var wait = new WaitForSeconds(period);
      var modifier = Modifier.Minus(damage, "poison");
      coroutine = Coroutine();
      return modifier.WithContext(DamageType.Poison);
      // return modifier;

      IEnumerator Coroutine() {
        for (int i = 0; i < times && modifier.enabled && ! token.IsCancellationRequested; i++) {
          damage.value += damagePerPeriod;
          yield return wait;
        }
      }
    }

    // [UnityTest]
    // public IEnumerator TestPoisonBasic() {
    //   health.modifiers.Add(poison);
    //   Assert.AreEqual(100f, health.value);
    //   yield return coroutine;
    //   Assert.AreEqual(90f, health.value);
    // }

    private void Evaluate(IEnumerator coroutine) {
      while (coroutine.MoveNext())
        ;
    }

    [Test]
    public void TestPoisonInterrupted() {
      health.modifiers.Add(poison);
      Assert.AreEqual(100f, health.value);
      coroutine.MoveNext();
      coroutine.MoveNext();
      coroutine.MoveNext();
      poison.enabled = false;
      Assert.IsFalse(coroutine.MoveNext());
      // We disable the poison which stops the coroutine.
      // But because it's disabled, we now have a problem.
      Assert.AreEqual(100f, health.value);
      poison.enabled = true;
      Assert.AreEqual(97f, health.value);
    }

    [Test]
    public void TestPoisonInterruptedWithToken() {
      health.modifiers.Add(poison);
      Assert.AreEqual(100f, health.value);
      coroutine.MoveNext();
      coroutine.MoveNext();
      coroutine.MoveNext();
      tokenSource.Cancel();
      Assert.IsFalse(coroutine.MoveNext());
      Assert.AreNotEqual(100f, health.value);
      // Well, this is cleaner.
      Assert.AreEqual(97f, health.value);
    }

    [Test]
    public void TestPoisonStepByStep() {
      health.modifiers.Add(poison);
      Assert.AreEqual(100f, health.value);
      coroutine.MoveNext();
      Assert.AreEqual(99f, health.value);
      coroutine.MoveNext();
      Assert.AreEqual(98f, health.value);
      coroutine.MoveNext();
      Assert.AreEqual(97f, health.value);
      coroutine.MoveNext();
      Assert.AreEqual(96f, health.value);
      coroutine.MoveNext();
      Assert.AreEqual(95f, health.value);
      coroutine.MoveNext();
      Assert.AreEqual(94f, health.value);
      coroutine.MoveNext();
      Assert.AreEqual(93f, health.value);
      coroutine.MoveNext();
      Assert.AreEqual(92f, health.value);
      coroutine.MoveNext();
      Assert.AreEqual(91f, health.value);
      coroutine.MoveNext();
      Assert.AreEqual(90f, health.value);
      Assert.IsFalse(coroutine.MoveNext());
    }

    [Test]
    public void TestMultiplePoisons() {
      var poison2 = Poison(1f, 1f, 10, out var coroutine2, tokenSource.Token);
      health.modifiers.Add(poison);
      health.modifiers.Add(poison2);
      Assert.AreEqual(100f, health.value);
      Evaluate(coroutine);
      Assert.AreEqual(90f, health.value);
      Evaluate(coroutine2);
      Assert.AreEqual(80f, health.value);
    }

    public void Cure(IModifiable<IReadOnlyValue<float>,float> attribute) {
      foreach (var modifier in attribute.modifiers
               .OfType<IModifier<DamageType, float>>()
               .Where(mod => mod.context == DamageType.Poison)
               .ToList())
        attribute.modifiers.Remove(modifier);
    }

    public float CureApplyDamage(IModifiable<IValue<float>,float> attribute) {
      var poisons = new HashSet<IModifier<float>>(attribute.modifiers
                                                  .OfType<IModifier<DamageType, float>>()
                                                  .Where(mod => mod.context == DamageType.Poison));
        // .ToHashSet();
      var totalDamage = poisons
        .SelectMany(poison => attribute.ProbeAffects(poison))
        .Select(x => x.after - x.before)
        .Sum();
        
      // Remove the modifiers but apply the damage.
      foreach(var modifier in poisons)
        attribute.modifiers.RemoveAll(modifier);
      attribute.initial.value += totalDamage;
      return totalDamage;
    }

    [Test]
    public void TestCurePoisons() {
      var poison2 = Poison(1f, 1f, 10, out var coroutine2, tokenSource.Token);
      health.modifiers.Add(poison);
      health.modifiers.Add(poison2);
      Assert.AreEqual(100f, health.value);
      Evaluate(coroutine);
      Assert.AreEqual(90f, health.value);
      Evaluate(coroutine2);
      Assert.AreEqual(80f, health.value);
      Cure(health);
      // Curing shouldn't actually change the health.
      Assert.AreEqual(100f, health.value);
      // Assert.AreEqual(80f, health.value);
    }

    [Test]
    public void TestCurePoisons2() {
      var poison2 = Poison(1f, 1f, 10, out var coroutine2, tokenSource.Token);
      health.modifiers.Add(poison);
      health.modifiers.Add(poison2);
      Assert.AreEqual(100f, health.value);
      Evaluate(coroutine);
      Assert.AreEqual(90f, health.value);
      Evaluate(coroutine2);
      Assert.AreEqual(80f, health.value);
      Assert.AreEqual(-10f, health.ProbeDelta(poison));
      float damage = CureApplyDamage(health);
      Assert.AreEqual(-20f, damage);
      Assert.AreEqual(80f, health.value);
    }

    /** We can use the same poison multiple times. */
    [Test]
    public void TestCureSameTwoPoisons() {
      var poison2 = poison;
      health.modifiers.Add(poison);
      health.modifiers.Add(poison2);
      Assert.AreEqual(100f, health.value);
      Evaluate(coroutine);
      // Assert.AreEqual(90f, health.value);
      // Evaluate(coroutine2);
      Assert.AreEqual(80f, health.value);
      Assert.AreEqual(-20f, health.ProbeDelta(poison));
      float damage = CureApplyDamage(health);
      Assert.AreEqual(-20f, damage);
      Assert.AreEqual(80f, health.value);
    }

  }
}
