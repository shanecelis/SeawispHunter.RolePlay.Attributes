using Xunit;

using SeawispHunter.Game.Stat;
namespace SeawispHunter.Game.Stat.Test;

public class UnitTest2 {
  Stat<float> health = new Stat<float> { name = "health", baseValue = 100f };
  IStat<float> currentHealth;
  IModifierValue<float> boost = Modifier.Multiply(1.10f, "10% boost");// { name = "10% boost", multiply = 1.10f };
  // IModifier<float> boost20 = new ModifierFloat { name = "20% boost", multiply = 1.20f };
  IModifierValue<float> boost20 = Modifier.Multiply(1.2f, "20% boost");
  // IModifier<float> damage = new ModifierFloat { name = "damage", plus = 0f };
  IModifierValue<float> damage = Modifier.Plus(0f, "damage");
  private int healthNotifications = 0;
  private int currentHealthNotifications = 0;
  private int damageNotifications = 0;
  private int boostNotifications = 0;

  public UnitTest2() {

    currentHealth = Stat.FromValue(health, "current health");
    // currentHealth.name = "current health";
    health.Add(boost);
    // currentHealth.Add(damage.Select(d => -d));
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
    damage.value = 10f;
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
    strengthPercentageGain.Add(Modifier.Plus(0.10f));
    strength.Add(Modifier.Multiply(strengthPercentageGain));
    Assert.Equal(11f, strength.value);
  }

  [Fact]
  public void TestDifferentAccumulationStyleMixedTypes() {

    var strength = new Stat<int> { name = "strength", baseValue = 10 };
    // var strength = new Stat<float> { name = "strength", baseValue = 10f };
    var strengthPercentageGain = new Stat<float> { baseValue = 1f };
    strengthPercentageGain.Add(Modifier.Plus(0.1f));
    strength.Add(Modifier.Multiply<int>(strengthPercentageGain));
    Assert.Equal(11, strength.value);
  }

  [Fact]
  /** Taken from Sidhion's article:
      https://gamedevelopment.tutsplus.com/tutorials/using-the-composite-design-pattern-for-an-rpg-attributes-system--gamedev-243
  */
  public void TestSidhionAccumulationStyle() {
    var stat = new Stat<float> { name = "strength", baseValue = 10f };
    var rawBonusesPlus = new Stat<float>();
    var rawBonusesMultiply = new Stat<float>() { baseValue = 1f };
    var finalBonusesPlus = new Stat<float>();
    var finalBonusesMultiply = new Stat<float>() { baseValue = 1f };
    stat.Add(Modifier.Plus(rawBonusesPlus));
    stat.Add(Modifier.Multiply(rawBonusesMultiply));
    stat.Add(Modifier.Plus(finalBonusesPlus));
    stat.Add(Modifier.Multiply(finalBonusesMultiply));
    Assert.Equal(10f, stat.value);
  }

}
