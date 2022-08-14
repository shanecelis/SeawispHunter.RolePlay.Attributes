using System;
using System.Collections;
using System.Collections.Generic;
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
  public class Jessee03Test {// A Test behaves as an ordinary method
    IModifiableValue<float> poisonResistance = new ModifiableValue<float>(1f);
    IModifiableValue<float> maxHealth = new ModifiableValue<float>(100f);
    IModifiable<IReadOnlyValue<float>,float> health;
    IModifier<float> poison;
    IEnumerator coroutine;

    [Flags]
    internal enum DamageType {
      None = 0,
      Fire   = 1,
      Cold   = 2,
      Poison = 4,
      All = 7,
    }
    internal class FightContext {
      public DamageType vulnerable;
      public DamageType incoming;
      public bool targetIsUndead;
    }

    IModifier<DamageType,float> Poison(float damagePerPeriod, float period, int times, out IEnumerator coroutine) {
      Value<float> damage = new Value<float>(0f);
      var wait = new WaitForSeconds(period);
      var modifier = Modifier.Minus(damage, "poison");
      coroutine = Coroutine();
      return modifier.WithContext(DamageType.Poison);

      IEnumerator Coroutine() {
        for (int i = 0; i < times && modifier.enabled; i++) {
          damage.value += damagePerPeriod;
          yield return wait;
        }
      }
    }
    
    public Jessee03Test() {
      health = new Modifiable<IReadOnlyValue<float>, float>(maxHealth);
      poison = Poison(1f, 1f, 10, out coroutine);
    }

    [UnityTest]
    public IEnumerator TestPoisonBasic() {
      health.modifiers.Add(poison);
      Assert.AreEqual(100f, health.value);
      yield return coroutine;
      Assert.AreEqual(90f, health.value);
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
  }
}
