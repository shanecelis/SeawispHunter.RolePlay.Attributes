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

/** The initial value type S can be an IReadOnlyValue<T> or IValue<T>.
    Generally, it's an IValue<T> but if it was derived from some other
    attribute, the initial value may be read only. */
public interface IModifiableValue<out S,T> : IReadOnlyValue<T>, INotifyPropertyChanged
  where S : IReadOnlyValue<T> {
  S initial { get; }
  // T value { get; }
  /** The list implementation handles property change events properly. */
  IPriorityCollection<IModifier<T>> modifiers { get; }
  // event PropertyChangedEventHandler PropertyChanged;
}

/** This IModifiableValue<T> interface is meant to capture values in games like health,
    strength, etc. that can be modified by various, sometimes distal, effects. */
public interface IModifiableValue<T> : IModifiableValue<IValue<T>,T> { }

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

}