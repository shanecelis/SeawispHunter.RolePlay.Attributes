using Xunit;

using SeawispHunter.Game.Stat;
namespace SeawispHunter.Game.Stat.Test;

public class UnitTest1 {
  Stat<float> health = new Stat<float> { name = "health", baseValue = 100f };
  DerivedStat<float> currentHealth;
  ModifierFloat boost = new ModifierFloat { name = "10% boost", multiply = 1.10f };
  ModifierFloat boost20 = new ModifierFloat { name = "20% boost", multiply = 1.20f };
  ModifierFloat damage = new ModifierFloat { name = "damage", plus = 0f };
  private int healthNotifications = 0;
  private int currentHealthNotifications = 0;
  private int damageNotifications = 0;
  private int boostNotifications = 0;

  public UnitTest1() {
    currentHealth = new DerivedStat<float>(health) { name = "current health" };
    health.Add(boost);
    currentHealth.Add(damage);
    health.PropertyChanged += (_, _) => healthNotifications++;
    currentHealth.PropertyChanged += (_, _) => currentHealthNotifications++;
    damage.PropertyChanged += (_, _) => damageNotifications++;
    boost.PropertyChanged += (_, _) => boostNotifications++;
  }

  [Fact]
  public void TestUnmodified() {
    health.Clear(); // XXX: This doesn't clear notifications.
    Assert.Equal(100f, health.baseValue);
    Assert.Equal(100f, health.value);
    Assert.Equal(0, healthNotifications);
    Assert.Equal(0, currentHealthNotifications);
    Assert.Equal(0, damageNotifications);
    Assert.Equal(0, boostNotifications);
  }

  [Fact]
  public void TestModified() {
    Assert.Equal(100f, health.baseValue);
    Assert.Equal(110f, health.value);
    Assert.Equal(0, healthNotifications);
    Assert.Equal(0, currentHealthNotifications);
    Assert.Equal(0, damageNotifications);
    Assert.Equal(0, boostNotifications);
  }

  [Fact]
  public void TestNotification() {
    Assert.Equal(100f, health.baseValue);
    Assert.Equal(110f, health.value);
    damage.plus = 10f;
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
    Assert.Equal(100f, health.baseValue);
    Assert.Equal(110f, health.value);
    health.Add(boost20);
    Assert.Equal(132f, health.value);
    Assert.Equal(1, healthNotifications);
    Assert.Equal(1, currentHealthNotifications);
    Assert.Equal(0, damageNotifications);
    Assert.Equal(0, boostNotifications);
  }

  [Fact]
  public void TestDifferentAccumulationStyle() {

    // I can't do this with an int.
    // var strength = new Stat<int> { name = "strength", baseValue = 10 };
    var strength = new Stat<float> { name = "strength", baseValue = 10f };
    var strengthPercentageGain = new Stat<float> { baseValue = 1f };
    strengthPercentageGain.Add(new ModifierFloat { plus = 0.10f });
    strength.Add(new DerivedModifierFloat { multiply = strengthPercentageGain });
    Assert.Equal(11f, strength.value);
  }

  [Fact]
  public void TestDifferentAccumulationStyleMixedTypes() {

    var strength = new Stat<int> { name = "strength", baseValue = 10 };
    // var strength = new Stat<float> { name = "strength", baseValue = 10f };
    var strengthPercentageGain = new Stat<float> { baseValue = 1f };
    strengthPercentageGain.Add(new ModifierFloat { plus = 0.1f });
    strength.Add(new DerivedModifierFloatInt { multiply = strengthPercentageGain });
    Assert.Equal(11, strength.value);
  }

}
