# SeawispHunter.RolePlay.Attributes

There comes a time when many a gamedev sets out for adventure but first must
create their own stat class. That is to say a class which captures a game
"stat", or "statistic" for lack of a better word, like health, attack, defense,
etc. This one is mine. Oh wait! there is a better word: "attribute." But I don't
want to take "attribute"[^1] from your game, so I'll call mine
`IModifiableValue<T>`. Consider it a sound building block for you to create
your own attribute class.

These attributes and their derivatives may affect and be effected by a multitude
of transient things, e.g, a sword that bestows an attack advantage; a shield
that raises one's defense; a ring that regenerates health. Because of that
attributes ought to respect the following requirements.

[^1]: Or "Attribute", "IAttribute", etc.

## Requirements

* An attribute's value shall be altered non-destructively. 
* Because so many things may alter an attribute, it shall notify us when changed.

## Barebones Example

``` c#
var health = new ModifiableValue<float> { baseValue = 100f };
health.modifiers.Add(Modifier.Times(1.10f));
Console.WriteLine($"Health is {health.value}."); // Prints: Health is 110.
health.modifiers.Add(Modifier.Plus(5f, "+5 health"));
Console.WriteLine($"Health is {health.value}."); // Prints: Health is 115.
```

## Attribute

At its root, an attribute has a `baseValue`. With no modifiers present, its
`value` equals its `baseValue`; its `value` is altered by modifiers starting
from its `baseValue`.

``` c#
public interface IModifiableValue<T> {
  T baseValue { get; set; }
  T value { get; }
  /** The list implementation sets up property change events automatically. */
  ICollection<IModifier<T>> modifiers { get; }
  event PropertyChangedEventHandler PropertyChanged;
}
```

## Modifier

A modifier can accept a value and change it arbitrarily. 

``` c#
public interface IModifier<T> {
  bool enabled { get; set; }
  T Modify(T given);
  event PropertyChangedEventHandler PropertyChanged;
}
```

However, often times the changes one wants to make are simple: add a value,
multiple a value, or substitute a value so these are made convenient for `int`
and `float` types.

``` c#
public interface IValuedModifier<T> : IModifier<T> {
  T value { get; set; }
}

public static class Modifier {
  public static IValuedModifier<T> Plus(T value);
  public static IValuedModifier<T> Subtract(T value);
  public static IValuedModifier<T> Times(T value);
  public static IValuedModifier<T> Divide(T value);
  public static IValuedModifier<T> Substitute(T value);
}
```

## Change Propogation

These classes use the `INotifyPropertyChanged` interface to propogate change
events, so any modifier that's changed or added will notify its attribute which
will notify any of its listeners. No need to poll for changes to an attribute.

## Abridged API

The API shown above is abridged to make its most salient points easy to
understand. The actual code includes some abstractions like `IValue<T>` and
`IMutableValue<T>` which are used to make attributes reuseable as modifiers
for instance.

## Further Examples

### Using Notifications

``` c#
var health = new ModifiableValue<float> { baseValue = 100f };
health.PropertyChanged += (_, _) => Console.WriteLine($"Health is {health.value}.");
health.modifiers.Add(Modifier.Times(1.10f));
// Prints: Health is 110.
health.modifiers.Add(Modifier.Plus(5f, "+5 health"));
// Prints: Health is 115.
```

### Using an Attribute as a Modifier

Let's create a max health attribute and we'll create a current health attribute,
which uses the max health as its `baseValue`.

``` c#
var maxHealth = new ModifiableValue<float> { baseValue = 100f };
var health = ModifiableValue.FromValue(maxHealth);
var damage = Modifier.Subtract(0f);

health.PropertyChanged += (_, _) => Console.WriteLine($"Health is {health.value}/{maxHealth.value}.");
health.modifiers.Add(damage);
// Prints: Health is 100/100.
damage.value = 10f;
// Prints: Health is 90/100.
maxHealth.modifier.Add(Modifier.Plus(20f, "+20 level gain"));
// Prints: Health is 110/120.
```

### Creating New Modifiers

New modifiers can be created by implementing the `IModifier<T>` interface or by
using the convenience methods in `Modifier` like `FromFunc()` shown below.

``` c#
var moonArmor = new ModifiableValue<float> { baseValue = 20f };
moonArmor.modifiers.Add(Modifier.FromFunc((float x) => DateTime.Now.IsFullMoon() ? 2 * x : x));
```

Unfortunately there is no such extension method `IsFullMoon()` for DateTime by
default but you can [add it](https://khalidabuhakmeh.com/calculate-moon-phase-with-csharp).

### Ordering Modifiers

The priority of a modifier defines its order. The default priority is `0`. Lower
numbers apply earlier; higher numbers apply later. Modifiers of the same
priority go in order of insertion.

``` c#
var maxMana = new ModifiableValue<float> { baseValue = 50f };
var mana = ModifiableValue.FromValue(maxMana);
var manaCost = Modifier.Subtract(0f);
mana.modifiers.Add(manaCost);
mana.PropertyChanged += (_, _) => Console.WriteLine($"Mana is {mana.value}/{maxMana.value}.");
mana.modifiers.Add(priority: 100, Modifier.FromFunc((float x) => Math.Clamp(x, 0, maxMana.value));
// Prints: Mana is 50/50.
manaCost.value = 1000f;
// Prints: Mana is 0/50.
```

### Time Out a Modifier

There are `EnableAfter()` and `DisableAfter()` extension methods for `IModifier<T>`.

``` c#
var armor = new ModifiableValue<int> { baseValue = 10 };
var powerUp = Modifier.Plus(5);
armor.modifiers.Add(powerUp);
health.PropertyChanged += (_, _) => Console.WriteLine($"Armor is {armor.value}.");
// Prints: Armor is 15.
powerUp.DisableAfter(TimeSpan.FromSeconds(20f));
// ... 
// [Wait 20 seconds.]
// Prints: Armor is 10.
```

## Writing Your Own Attribute Class

You can go far with `IModifiableValue<T>` but you'll probably want to bring some
organization to it beyond the order of modifiers. Here is an example of what
that might look like if organized yours like [Jacob
Penner](https://jkpenner.wordpress.com/2015/06/09/rpgsystems-stat-system-02-modifiers/)
does:

``` c#
public class PennerStat<T> : ModifiableValue<T> {
  public readonly IModifiableValue<T> baseValuePlus = new ModifiableValue<T>();
  public readonly IModifiableValue<T> baseValueTimes = new ModifiableValue<T>() {
    baseValue = one
  };
  public readonly IModifiableValue<T> totalValuePlus = new ModifiableValue<T>();
  public readonly IModifiableValue<T> totalValueTimes = new ModifiableValue<T>() {
    baseValue = one
  };

  public PennerStat() {
    // value = (baseValue * baseValueTimes + baseValuePlus) * totalValueTimes + totalValuePlus
    modifiers.Add(100, Modifier.Times<T,T>(baseValueTimes));
    modifiers.Add(200, Modifier.Plus<T,T>(baseValuePlus));
    modifiers.Add(300, Modifier.Times<T,T>(totalValueTimes));
    modifiers.Add(400, Modifier.Plus<T,T>(totalValuePlus));
  }

  private static T one => Modifier.GetOp<T>().one;
}
```

See the
[Style.cs](https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes/blob/master/src/Style.cs)
file for more examples.


## Notes

### Dealing with Math in Generics

.NET 7 has [generic math
operators](https://devblogs.microsoft.com/dotnet/dotnet-7-generic-math/), which
will be a godsend. It will allow us to write methods like this:

``` c#
T Plus<T>(T a, T b) where T : INumber<T> => a + b;
```

which is invalid for prior versions. 

This attributes library makes use of this generic math, however, we also want to
support `netstandard2.0` because that's what Unity supports. So here's a trick
given by this [article](https://pvs-studio.com/en/blog/posts/csharp/0878/#ID119C0760DC)
to allow you to do generic math without .NET 7's `INumber<T>` support.

``` c#
interface IOperator<T> {
  T Plus(T a, T a)
}

struct OpFloat : IOperator<float> {
  public float Plus(float a, float b) => a + b;
}

void SomeProcessing<T, TOperator>(...) where TOperation : struct, IOperator<T> {
  T var1 = ...;
  T var2 = ...;
  T sum = default(TOperator).Plus(var1, var2);  // This is zero cost!
}

void Caller() {
  SomeProcessing<float, OpFloat>(...);
}
```

## License

This project is released under the MIT license.

## Acknowledgments

This project was inspired and informed by the following sources:

- [How to Make an RPG: Stats](http://howtomakeanrpg.com/a/how-to-make-an-rpg-stats.html) by Dan Schuller;
- [RPGSystems: Stat System 03: Modifiers](https://jkpenner.wordpress.com/2015/06/09/rpgsystems-stat-system-02-modifiers/) by Jacob Penner;
- [Using the Composite Design Pattern for an RPG Attributes System](https://gamedevelopment.tutsplus.com/tutorials/using-the-composite-design-pattern-for-an-rpg-attributes-system--gamedev-243) Daniel Sidhion;
- [Character Stats (aka Attributes) System](https://forum.unity.com/threads/tutorial-character-stats-aka-attributes-system.504095/) by Kryzarel.
  This also has an associated Unity3D [asset](https://assetstore.unity.com/packages/tools/integration/character-stats-106351).
  

