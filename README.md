# SeawispHunter.RolePlay.Attributes

There comes a time when many a gamedev sets out for adventure but first must
create their own attribute class, that is a class which captures a game "stat",
or "statistic" for lack of a better word, like health, attack, defense, etc.
This one is mine. Oh wait! there is a better word: attribute. But I don't want
to take "attribute" from your game, so I'll call mine `IModifiableValue<T>`.

These attributes and their derivatives are often affected by a multitude of
transient things, e.g, a sword that bestows an attack advantage; a shield that
raises one's defense; a ring that regenerates health. Because of that attributes
ought to respect the following requirements.

## Requirements

* An attribute shall be altered non-destructively. 
* Because so many things may alter an attribute, it shall notify us when changed.

## Example

``` c#
var health = new ModifiableValue<float> { name = "health", baseValue = 100f };
health.modifiers.Add(Modifier.Multiply(1.10f));
Console.WriteLine($"Health is {health.value}."); // Prints: Health is 110.
health.modifiers.Add(Modifier.Plus(5f, "+5 health"));
Console.WriteLine($"Health is {health.value}."); // Prints: Health is 115.
```

## Attribute

At its root, an attribute has a `baseValue`. With no modifiers present, its
`baseValue` equals its `value`; its `value` can be altered by modifiers starting
from its `baseValue`.

``` c#
public interface IModifiableValue<T> {
  T baseValue { get; set; }
  T value { get; }
  /** The list implementation sets up property change events automatically. */
  IList<IModifier<T>> modifiers { get; }
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
multiple a value, or substitute a value so these are made convenient.

``` c#
public interface IValuedModifier<T> : IModifier<T> {
  T value { get; set; }
}

public static class Modifier {
  public static IValuedModifier<T> Plus(T value);
  public static IValuedModifier<T> Multiply(T value);
  public static IValuedModifier<T> Substitute(T value);
}
```

## Change Propogation

These classes use the `INotifyPropertyChanged` to propogate change events, so
any modifier that's changed or added will notify its attribute which will notify any
of its listeners. So there's no need to poll for changes to an attribute.

## Abridged API

The API shown above is abridged to make its most salient points easy to
understand. The actual code includes some abstractions like `IValue<T>` and
`IMutableValue<T>` which is used to make an attribute reuseable as a modifier
for instance.

## License

This project is released under the MIT license.

## Acknowledgments

This project was inspired and informed by the following sources:

- http://howtomakeanrpg.com/a/how-to-make-an-rpg-stats.html
- https://jkpenner.wordpress.com/2015/06/09/rpgsystems-stat-system-02-modifiers/
- https://gamedevelopment.tutsplus.com/tutorials/using-the-composite-design-pattern-for-an-rpg-attributes-system--gamedev-243
