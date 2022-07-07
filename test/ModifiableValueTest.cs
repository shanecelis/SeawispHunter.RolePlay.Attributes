using Xunit;

using SeawispHunter.RolePlay.Attributes;
namespace SeawispHunter.RolePlay.Attributes.Test;

public class ModifiableValueTest {
  ModifiableValue<float> health = new ModifiableValue<float> { baseValue = 100f };
  IModifiableValue<float> currentHealth;
  IModifierValue<float> boost = Modifier.Multiply(1.10f, "10% boost");// { name = "10% boost", multiply = 1.10f };
  // IModifier<float> boost20 = new ModifierFloat { name = "20% boost", multiply = 1.20f };
  IModifierValue<float> boost20 = Modifier.Multiply(1.2f, "20% boost");
  // IModifier<float> damage = new ModifierFloat { name = "damage", plus = 0f };
  IModifierValue<float> damage = Modifier.Plus(0f, "damage");
  private int healthNotifications = 0;
  private int currentHealthNotifications = 0;
  private int damageNotifications = 0;
  private int boostNotifications = 0;

  public ModifiableValueTest() {

    currentHealth = ModifiableValue.FromValue(health, "current health");
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
    var iv = (IValue<int>) v;
    Assert.Equal(1, iv.value);
    v.value = 2;
    Assert.Equal(2, iv.value);
  }

  [Fact]
  public void TestUnmodified() {
    health.modifiers.Clear(); // XXX: This doesn't clear notifications.
    Assert.Equal(100f, health.baseValue);
    Assert.Equal(100f, health.value);
    Assert.Equal(1, healthNotifications);
    Assert.Equal(1, currentHealthNotifications);
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

    m = Modifier.Multiply(2, "blah");
    Assert.Equal("\"blah\" *2", m.ToString());
  }

  [Fact]
  public void TestDifferentAccumulationStyle() {

    // I can't do this with an int.
    // var strength = new ModifiableValue<int> { name = "strength", baseValue = 10 };
    var strength = new ModifiableValue<float> { baseValue = 10f };
    var strengthPercentageGain = new ModifiableValue<float> { baseValue = 1f };
    strengthPercentageGain.modifiers.Add(Modifier.Plus(0.10f));
    strength.modifiers.Add(Modifier.Multiply(strengthPercentageGain));
    Assert.Equal(11f, strength.value);
  }

  [Fact]
  public void TestDifferentAccumulationStyleMixedTypes() {

    var strength = new ModifiableValue<int> { baseValue = 10 };
    // var strength = new ModifiableValue<float> { name = "strength", baseValue = 10f };
    var strengthPercentageGain = new ModifiableValue<float> { baseValue = 1f };
    strengthPercentageGain.modifiers.Add(Modifier.Plus(0.1f));
    strength.modifiers.Add(Modifier.Multiply<int>(strengthPercentageGain));
    Assert.Equal(11, strength.value);
  }

  [Fact]
  /** Taken from Sidhion's article:
      https://gamedevelopment.tutsplus.com/tutorials/using-the-composite-design-pattern-for-an-rpg-attributes-system--gamedev-243
  */
  public void TestSidhionAccumulationStyle() {
    var stat = new ModifiableValue<float> { baseValue = 10f };
    var rawBonusesPlus = new ModifiableValue<float>();
    var rawBonusesMultiply = new ModifiableValue<float>() { baseValue = 1f };
    var finalBonusesPlus = new ModifiableValue<float>();
    var finalBonusesMultiply = new ModifiableValue<float>() { baseValue = 1f };
    stat.modifiers.Add(Modifier.Plus(rawBonusesPlus));
    stat.modifiers.Add(Modifier.Multiply(rawBonusesMultiply));
    stat.modifiers.Add(Modifier.Plus(finalBonusesPlus));
    stat.modifiers.Add(Modifier.Multiply(finalBonusesMultiply));
    Assert.Equal(10f, stat.value);
    rawBonusesPlus.modifiers.Add(Modifier.Plus(1f));
    Assert.Equal(11f, stat.value);
    rawBonusesMultiply.modifiers.Add(Modifier.Plus(1f));
    Assert.Equal(22f, stat.value);
  }

  [Fact]
  public void TestSidhionStyle() {
    int notifications = 0;
    int notifications2 = 0;
    var stat = new SidhionStat<float> { baseValue = 10f };
    stat.PropertyChanged += (_, _) => notifications++;
    stat.rawBonusesPlus.PropertyChanged += (_, _) => notifications2++;

    Assert.Equal(0, notifications);
    Assert.Equal(0, notifications2);
    Assert.Equal(10f, stat.value);
    stat.rawBonusesPlus.modifiers.Add(Modifier.Plus(1f));
    Assert.Equal(1, notifications2);
    Assert.Equal(1, notifications);
    Assert.Equal(11f, stat.value);
    stat.rawBonusesMultiply.modifiers.Add(Modifier.Plus(1f));
    Assert.Equal(2, notifications);
    Assert.Equal(22f, stat.value);
  }

}
