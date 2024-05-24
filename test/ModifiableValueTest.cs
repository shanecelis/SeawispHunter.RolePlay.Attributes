/* Original code[1] Copyright (c) 2022 Shane Celis[2]
   Licensed under the MIT License[3]

   This comment generated by code-cite[3].

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://twitter.com/shanecelis
   [3]: https://opensource.org/licenses/MIT
   [4]: https://github.com/shanecelis/code-cite
*/

using System;
using System.Linq;
using System.Threading;
using Xunit;

using SeawispHunter.RolePlay.Attributes;
namespace SeawispHunter.RolePlay.Attributes.Test {

public class ModifiableValueTest {
  ModifiableValue<float> health = new ModifiableValue<float>(100f);
  // IModifiable<IReadOnlyValue<float>, float> currentHealth;
  IModifiable<float> currentHealth;
  IModifier<float> boost = Modifier.Times(1.10f, "10% boost");// { name = "10% boost", multiply = 1.10f };
  // IModifier<float> boost20 = new ModifierFloat { name = "20% boost", multiply = 1.20f };
  IModifier<float> boost20 = Modifier.Times(1.2f, "20% boost");
  // IModifier<float> damage = new ModifierFloat { name = "damage", plus = 0f };
  IModifier<IValue<float>,float> damage = Modifier.Plus<float>(new Value<float>(), "damage");
  private int healthNotifications = 0;
  private int currentHealthNotifications = 0;
  private int damageNotifications = 0;
  private int boostNotifications = 0;

  public ModifiableValueTest() {

    currentHealth = new ModifiableReadOnlyValue<float>(health);
    // currentHealth.name = "current health";
    health.modifiers.Add(boost);
    // currentHealth.Add(damage.Select(d => -d));
    currentHealth.modifiers.Add(damage);
    health.PropertyChanged += (_, _) => healthNotifications++;
    currentHealth.PropertyChanged += (_, _) => currentHealthNotifications++;
    damage.PropertyChanged += (_, _) => damageNotifications++;
    boost.PropertyChanged += (_, _) => boostNotifications++;
  }

  [Fact]
  public void TestValue() {
    var v = new Value<int> { value = 1 };
    Assert.Equal(1, v.value);
    var iv = (IReadOnlyValue<int>) v;
    Assert.Equal(1, iv.value);
    v.value = 2;
    Assert.Equal(2, v.value);
    Assert.Equal(2, iv.value);
  }

  [Fact]
  public void TestUnmodified() {
    health.modifiers.Clear(); // XXX: This doesn't clear notifications.
    Assert.Equal(100f, health.initial.value);
    Assert.Equal(100f, health.value);
    Assert.Equal(1, healthNotifications);
    Assert.Equal(1, currentHealthNotifications);
    Assert.Equal(0, damageNotifications);
    Assert.Equal(0, boostNotifications);
  }

  [Fact]
  public void TestModified() {
    Assert.Equal(100f, health.initial.value);
    Assert.Equal(110f, health.value);
    Assert.Equal(0, healthNotifications);
    Assert.Equal(0, currentHealthNotifications);
    Assert.Equal(0, damageNotifications);
    Assert.Equal(0, boostNotifications);
  }

  [Fact]
  public void TestDisabled() {
    Assert.Equal(100f, health.initial.value);
    Assert.Equal(110f, health.value);
    boost.enabled = false;
    Assert.Equal(100f, health.value);
    Assert.Equal(1, healthNotifications);
    Assert.Equal(1, currentHealthNotifications);
    Assert.Equal(0, damageNotifications);
    Assert.Equal(1, boostNotifications);
  }

  [Fact]
  public void TestNotification() {
    Assert.Equal(100f, health.initial.value);
    Assert.Equal(110f, health.value);
    damage.context.value = 10f;
    Assert.Equal(0, healthNotifications);
    Assert.Equal(1, currentHealthNotifications);
    Assert.Equal(1, damageNotifications);
    Assert.Equal(0, boostNotifications);
  }

  [Fact]
  public void TestNotificationOnAdd() {
    Assert.Equal(0, healthNotifications);
    Assert.Equal(0, currentHealthNotifications);
    Assert.Equal(0, damageNotifications);
    Assert.Equal(0, boostNotifications);
    Assert.Equal(100f, health.initial.value);
    Assert.Equal(110f, health.value);
    health.modifiers.Add(boost20);
    Assert.Equal(132f, health.value);
    Assert.Equal(1, healthNotifications);
    Assert.Equal(1, currentHealthNotifications);
    Assert.Equal(0, damageNotifications);
    Assert.Equal(0, boostNotifications);
  }

  [Fact]
  public void TestStatToString() {
    Assert.Equal("110", health.ToString());
    Assert.Equal("110", currentHealth.ToString());
  }

  [Fact]
  public void TestModifierToString() {
    var m = Modifier.Plus(1);
    Assert.Equal("+1", m.ToString());

    m = Modifier.Plus(1, "+1 sword");
    Assert.Equal("\"+1 sword\" +1", m.ToString());

    m = Modifier.Times(2, "blah");
    Assert.Equal("\"blah\" *2", m.ToString());
  }

  [Fact]
  public void TestDifferentAccumulationStyle() {

    // I can't do this with an int.
    // var strength = new ModifiableValue<int> { name = "strength", baseValue = 10 };
    var strength = new ModifiableValue<float>(10f);
    var strengthPercentageGain = new ModifiableValue<float>(1f);
    strengthPercentageGain.modifiers.Add(Modifier.Plus(0.10f));
    strength.modifiers.Add(Modifier.Times<float>(strengthPercentageGain));
    Assert.Equal(11f, strength.value);
  }

  [Fact]
  public void TestDifferentAccumulationStyleMixedTypes() {

    var strength = new ModifiableValue<int>(10);
    // var strength = new ModifiableValue<float> { name = "strength", baseValue = 10f };
    var strengthPercentageGain = new ModifiableValue<float>(1f);
    strengthPercentageGain.modifiers.Add(Modifier.Plus(0.1f));
    strength.modifiers.Add(Modifier.Times<float>(strengthPercentageGain).Cast<float,int>());
    Assert.Equal(11, strength.value);
  }

  [Fact]
  /** Taken from Sidhion's article:
      https://gamedevelopment.tutsplus.com/tutorials/using-the-composite-design-pattern-for-an-rpg-attributes-system--gamedev-243
  */
  public void TestSidhionAccumulationStyle() {
    var stat = new ModifiableValue<float>(10f);
    var rawBonusesPlus = new ModifiableValue<float>();
    var rawBonusesTimes = new ModifiableValue<float>(1f);
    var finalBonusesPlus = new ModifiableValue<float>();
    var finalBonusesMultiply = new ModifiableValue<float>(1f);
    Assert.True(stat is IReadOnlyValue<float>);
    Assert.True(rawBonusesPlus is IReadOnlyValue<float>);
    stat.modifiers.Add(Modifier.Plus<float>(rawBonusesPlus));
    stat.modifiers.Add(Modifier.Times<float>(rawBonusesTimes));
    stat.modifiers.Add(Modifier.Plus<float>(finalBonusesPlus));
    stat.modifiers.Add(Modifier.Times<float>(finalBonusesMultiply));
    Assert.Equal(10f, stat.value);
    rawBonusesPlus.modifiers.Add(Modifier.Plus(1f));
    Assert.Equal(11f, stat.value);
    rawBonusesTimes.modifiers.Add(Modifier.Plus(1f));
    Assert.Equal(22f, stat.value);
  }

  [Fact]
  public void TestWaysToAdd() {
    var stat = new ModifiableValue<float>(10f);
    var rawBonusesPlus = new ModifiableValue<float>();
    Assert.True(stat is IReadOnlyValue<float>);
    Assert.True(rawBonusesPlus is IReadOnlyValue<float>);
    var m = Modifier.Plus((IReadOnlyValue<float>)rawBonusesPlus);
    stat.modifiers.Add(m);
    // stat.modifiers.Add(rawBonusesPlus.Plus());
    // XXX: This line does not work.
    // stat.modifiers.Add(Modifier.Plus(rawBonusesPlus));
    stat.modifiers.Add(Modifier.Plus<float>(rawBonusesPlus));
    stat.modifiers.Add(Modifier.Plus<float>(rawBonusesPlus));
  }

  [Fact]
  public void TestWaysToAddLiterals() {
    var stat = new ModifiableValue<float>(10f);
    var m = Modifier.Plus(1f);
    stat.modifiers.Add(m);
    stat.modifiers.Add(Modifier.Plus(1f));
    stat.modifiers.Add(Modifier.Plus<float>(1f));
    // stat.modifiers.Add(Modifier.Plus<float,float>(1f));
  }

  /** Turned Sidhion's into a class. */
  [Fact]
  public void TestSidhionStyle() {
    int notifications = 0;
    int notifications2 = 0;
    var stat = new SidhionStat<float>(10f);
    stat.PropertyChanged += (_, _) => notifications++;
    stat.rawBonusesPlus.PropertyChanged += (_, _) => notifications2++;

    Assert.Equal(0, notifications);
    Assert.Equal(0, notifications2);
    Assert.Equal(10f, stat.value);
    stat.rawBonusesPlus.modifiers.Add(Modifier.Plus(1f));
    Assert.Equal(1, notifications2);
    Assert.Equal(1, notifications);
    Assert.Equal(11f, stat.value);
    stat.rawBonusesTimes.modifiers.Add(Modifier.Plus(1f));
    Assert.Equal(2, notifications);
    Assert.Equal(22f, stat.value);
  }

// #if ! NETCOREAPP
#if NET7_0_OR_GREATER
  #warning Using a Timer.
  [Fact]
  public void TestDisableTimeout() {
    Assert.Equal(110f, health.value);
    Assert.True(boost.enabled);
    boost.DisableAfter(TimeSpan.FromMilliseconds(100f));
    Assert.True(boost.enabled);
    Assert.Equal(110f, health.value);
    Thread.Sleep(TimeSpan.FromMilliseconds(10f));
    Assert.True(boost.enabled);
    Assert.Equal(110f, health.value);
    Thread.Sleep(TimeSpan.FromMilliseconds(200f));
    Assert.False(boost.enabled);
    Assert.Equal(100f, health.value);
  }

  [Fact]
  public void TestEnableTimeout() {
    Assert.Equal(110f, health.value);
    boost.enabled = false;
    Assert.False(boost.enabled);
    boost.EnableAfter(TimeSpan.FromMilliseconds(100f));
    Assert.False(boost.enabled);
    Assert.Equal(100f, health.value);
    Thread.Sleep(TimeSpan.FromMilliseconds(10f));
    Assert.False(boost.enabled);
    Assert.Equal(100f, health.value);
    Thread.Sleep(TimeSpan.FromMilliseconds(200f));
    Assert.True(boost.enabled);
    Assert.Equal(110f, health.value);
  }
#endif

  [Fact]
  public void TestModifierPriority() {
    health.modifiers.Clear();
    health.modifiers.Add(boost);
    health.modifiers.Add(-10, damage);
    Assert.Equal(damage, health.modifiers.First());
    Assert.Equal(boost, health.modifiers.Skip(1).First());
    health.modifiers.Add(boost20);
    Assert.Equal(boost20, health.modifiers.Skip(2).First());
  }
}

}
