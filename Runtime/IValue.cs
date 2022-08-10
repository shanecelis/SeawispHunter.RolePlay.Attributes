/* Original code[1] Copyright (c) 2022 Shane Celis[2]
   Licensed under the MIT License[3]

   This comment generated by code-cite[3].

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://twitter.com/shanecelis
   [3]: https://opensource.org/licenses/MIT
   [4]: https://github.com/shanecelis/code-cite
*/
using System.ComponentModel;
using System.Collections.Generic;

namespace SeawispHunter.RolePlay.Attributes {

/** IReadOnlyValue<T> notifies listeners when it changes. That's it.

    You can only read this value but that doesn't mean it's immutable or const.
    It may be there are other things that change it.
*/
public interface IReadOnlyValue<out T> : INotifyPropertyChanged {
  /** NOTE: This might seem weird to not have a setter since it will notify you
      when it changes. However, if you consider a value that is not provided by
      a field but by some other thing like a `Func<T>` then it makes more sense.
  */
  T value { get; }
  // event PropertyChangedEventHandler PropertyChanged;
}

/** IValue<T> is an IReadOnlyValue<T> that can be directly changed. */
public interface IValue<T> : IReadOnlyValue<T> {
  /* We want this to be settable. */
  new T value { get; set; }
}

/** An IValue<T> that is bounded by a minValue and maxValue. */
public interface IBoundedValue<T> : IValue<T> {
  T minValue { get; }
  T maxValue { get; }
}

/** The initial value type S is altered by a collection of modifiers. How it's
    modified is implementation dependent, but in general, one could expect the
    initial value to be converted to a T then serially modified by each enabled
    modifier in the collection.

    Generally, the `initial` property is some kind of IReadOnlyValue<T>. */
public interface IModifiable<out S,T> : IReadOnlyValue<T>, INotifyPropertyChanged
  // XXX: Must this interface pin down the S type so dramatically?
  // where S : IReadOnlyValue<T>
{
  S initial { get; }
  // T value { get; }
  /** The list implementation handles property change events properly. */
  IPriorityCollection<IModifier<T>> modifiers { get; }
  // event PropertyChangedEventHandler PropertyChanged;
}
 
/** This IModifiableValue<T> interface is meant to capture values in games like health,
    strength, etc. that can be modified by various, sometimes distal, effects. */
public interface IModifiableValue<T> : IModifiable<IValue<T>,T> { }

/** The IModifiableReadOnly<T>'s initial value is a read only value. It
    best represents the requirements of an attribute only being altered
    non-destructively. */
public interface IModifiableReadOnlyValue<T> : IModifiable<IReadOnlyValue<T>,T> { }

/** We want to be able to specify a priority. */
public interface IPriorityCollection<T> : ICollection<T> {
  void Add(int priority, T modifier);
}

/** A IModifier<T> modifies an IModifiableValue<T>'s value. */
public interface IModifier<T> : INotifyPropertyChanged {
  bool enabled { get; set; }
  T Modify(T given);
  // event PropertyChangedEventHandler PropertyChanged;
}

/** A modifier that also provides a context. Good for exposing
    I(ReadOnly)Values for instance. */
public interface IModifier<out S,T> : IModifier<T> {
  S context { get; }
}

/** A target has a modifier and a selector so we know what to apply it to. */
public interface ITarget<in S, T> {
  IModifier<T> modifier { get; }
  IModifiable<IReadOnlyValue<T>,T> AppliesTo(S thing);

}

/** If a class is a decorator, give us a means of peeking inside if we need to. */
public interface IDecorator<out T> {
  T decorated { get; }
}

// public interface ITarget<out R,in S, T> : ITarget<S, T> {
//   R context { get; }
// }

}
