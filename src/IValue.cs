using System.ComponentModel;

namespace SeawispHunter.RolePlay.Attributes {

/** IValue<T> notifies listeners when changed. That's it. */
public interface IValue<T> : INotifyPropertyChanged {
  /** NOTE: This might seem weird to not have a setter since it will notify you
      when it changes. However, if you consider a value that is not provided by
      a field but by some other thing like a `Func<T>` then it makes more sense.
  */
  T value { get; }
  // event PropertyChangedEventHandler PropertyChanged;
}

// XXX: Consider renaming IValue to IReadOnlyValue and IMutableValue to IValue.
/* IMutableValue<T> is an IValue<T> that can be directly changed. */
public interface IMutableValue<T> : IValue<T> {
  /* We want this to be settable. */
  new T value { get; set; }
}

/** This IModifiableValue<T> class is meant to capture values in games like health,
    strength, etc. that can be modified by various, sometimes distal, effects. */
public interface IModifiableValue<T> : IValue<T>, INotifyPropertyChanged {
  T baseValue { get; set; }
  // T value { get; }
  /** The list implementation will handle chaining property change events. */
  IPriorityCollection<IModifier<T>> modifiers { get; }
  // event PropertyChangedEventHandler PropertyChanged;
}

/** We want to be able to specify a priority. */
public interface IPriorityCollection<T> : ICollection<T> {
  void Add(int priority, T modifier);
}

}
