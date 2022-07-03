# SeawispHunter.Game.Stat

There comes a time when every gamedev must create their own stat class, that is
a class which captures game "statistics" for lack of a better word like health,
attack, defense, etc. This one is mine.

These stats are often affected by a multitude of transient
things, e.g, a sword that bestows an attack advantage; a shield that raises
one's defense; a ring that regenerates health.

## Requirements

* A stat shall be altered non-destructively. 
* Because so many things may alter a stat, it shall notify us when changed.

## Example

``` c#
var health = new Stat<float> { name = "health", baseValue = 100f };
health.Add(Modifier.Multiply(1.10f, "10% boost"));
Console.WriteLine($"Health is {health.value}."); // Prints: Health is 110.
health.Add(Modifier.Plus(5f, "+5 health"));
Console.WriteLine($"Health is {health.value}."); // Prints: Health is 115.
```

## Stat

At its root, a stat has a `baseValue` that can be altered by modifiers.

``` c#
public interface IStat<T> {
  string name { get; }
  T baseValue { get; set; }
  T value { get; }
  IEnumerable<IModifier<T>> modifiers { get; }
  void Add(IModifier<T> modifier);
  void Remove(IModifier<T> modifier);
  void Clear();
  event PropertyChangedEventHandler PropertyChanged;
}
```

## Modifier

A modifier can accept a value and change it arbitrarily. 

``` c#
public interface IModifier<T> {
  string name { get; }
  T Modify(T given);
  event PropertyChangedEventHandler PropertyChanged;
}
```

However, often times the changes one wants to make are simple: add a value,
multiple a value, or substitute a value so these are made convenient.

``` c#
public interface IModifierValue<T> : IModifier<T> {
  T value { get; set; }
}

public static class Modifier {
  public static IModifierValue<T> Plus(T value, string name = null);
  public static IModifierValue<T> Multiply(T value, string name = null);
  public static IModifierValue<T> Substitute(T value, string name = null);
}
```

## Change Propogation

These classes use the `INotifyPropertyChanged` to propogate change events, so
any modifier that's changed or added will notify its stat which will notify any
of its listeners. So there's no need to poll for changes to a stat.

## Abridged API

The API shown above is abridged to make its most salient points easy to
understand. The actual code includes some abstractions like `IValue<T>` that is
used to make a stat reuseable as a modifier for instance.
