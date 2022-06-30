using Xunit;

using SeawispHunter.Game.Stat;
namespace SeawispHunter.Game.Stat.Test;

public class UnitTest1 {
  Stat<float> health = new Stat<float> { name = "health", description = "", baseValue = 100f };
  // var currentHealth = new DerivedStat<float> { stat = health, name = "current health" };
  DerivedStat<float> currentHealth;
  MutableModifierFloat boost = new MutableModifierFloat { name = "10% boost", multiply = 1.10f };
  MutableModifierFloat boost20 = new MutableModifierFloat { name = "20% boost", multiply = 1.20f };
  MutableModifierFloat damage = new MutableModifierFloat { name = "damage", plus = 0f };
  private int healthNotifications = 0;
  private int currentHealthNotifications = 0;
  private int damageNotifications = 0;
  private int boostNotifications = 0;

  public UnitTest1() {
    currentHealth = new DerivedStat<float>(health) { name = "current health" };
    health.AddModifier(boost, true);
    currentHealth.AddModifier(damage, true);
    health.PropertyChanged += (_, _) => healthNotifications++;
    currentHealth.PropertyChanged += (_, _) => currentHealthNotifications++;
    damage.PropertyChanged += (_, _) => damageNotifications++;
    boost.PropertyChanged += (_, _) => boostNotifications++;
  }

  [Fact]
  public void TestUnmodified() {
    health.modifiers.Clear(); // XXX: This doesn't clear notifications.
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
    damage.SetPlus(10f);
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
    health.AddModifier(boost20, false);
    Assert.Equal(132f, health.value);
    Assert.Equal(1, healthNotifications);
    Assert.Equal(1, currentHealthNotifications);
    Assert.Equal(0, damageNotifications);
    Assert.Equal(0, boostNotifications);
  }
}
