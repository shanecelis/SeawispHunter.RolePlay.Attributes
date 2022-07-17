using System;

namespace SeawispHunter.RolePlay.Attributes;

public static class ValueExtensions {
  /** Give ourselves a little projection, as a treat. */
  public static IValue<T> Select<S,T>(this IValue<S> v, Func<S,T> func) {
    var w = Value.FromFunc(() => func(v.value), out var callOnChange);
    v.PropertyChanged += (_, _) => callOnChange();
    return w;
  }

  /* I don't know. This seems overly complicated. It's no longer projection, it's projection and an inverse/coalesce action. */
  public static IMutableValue<T> Select<S,T>(this IMutableValue<S> v, Func<S,T> func, Action<IMutableValue<S>,T> @set) {
    var w = Value.FromFunc(() => func(v.value), x => @set(v, x), out var callOnChange);
    v.PropertyChanged += (_, _) => callOnChange();
    return w;
  }

  // public static IModifier<S,T> Select<S,T>(this IModifier<S,T> m, Func<
}