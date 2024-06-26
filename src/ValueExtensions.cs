/* Original code[1] Copyright (c) 2022 Shane Celis[2]
   Licensed under the MIT License[3]

   This comment generated by code-cite[3].

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://twitter.com/shanecelis
   [3]: https://opensource.org/licenses/MIT
   [4]: https://github.com/shanecelis/code-cite
*/
using System;
using System.ComponentModel;
using System.Collections;
using System.Threading;

#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace SeawispHunter.RolePlay.Attributes {

public static class ValueExtensions {
  /** Give ourselves a little projection, as a treat. */
  public static IReadOnlyValue<T> Select<S,T>(this IReadOnlyValue<S> v, Func<S,T> func) {
    var w = ReadOnlyValue.Create(() => func(v.value), out var callOnChange);
    v.PropertyChanged += (_, _) => callOnChange();
    return w;
  }

  public static IReadOnlyValue<U> Zip<S,T,U>(this IReadOnlyValue<S> v, IReadOnlyValue<T> w, Func<S,T,U> func) {
    var u = ReadOnlyValue.Create(() => func(v.value, w.value), out var callOnChange);
    v.PropertyChanged += (_, _) => callOnChange();
    w.PropertyChanged += (_, _) => callOnChange();
    return u;
  }

  /* I don't know. This seems overly complicated. It's no longer projection, it's projection and an inverse/coalesce action. */
  public static IValue<T> Select<S,T>(this IValue<S> v, Func<S,T> @get, Action<IValue<S>,T> @set) {
    var w = Value.Create(() => @get(v.value), x => @set(v, x), out var callOnChange);
    v.PropertyChanged += (_, _) => callOnChange();
    return w;
  }

  internal class ActionDisposable : IDisposable {
    private Action action;
    public ActionDisposable(Action action) => this.action = action;
    public void Dispose() {
      action();
    }
  }

  /** Convenience method to connect to the property change events. */
  public static IDisposable OnChange<T>(this T v, Action<T> action) where T : INotifyPropertyChanged {
    v.PropertyChanged += PropertyChange;
    return new ActionDisposable(() => v.PropertyChanged -= PropertyChange);

    void PropertyChange(object source, PropertyChangedEventArgs args) => action(v);
  }

  public static IModifier<Y> Cast<X,Y>(this IModifier<X> m)
#if NET7_0_OR_GREATER
    where X : INumber<X> where Y : INumber<Y>
#endif
  {
    return new Modifier.CastingModifier<X,Y>(m);
  }

#if UNITY_5_3_OR_NEWER
  public static IEnumerator LerpTo(this IValue<float> v,
                                   float targetValue,
                                   float duration,
                                   float? period = null,
                                   CancellationToken token = default) {
    float startValue = v.value;
    float start = UnityEngine.Time.time;
    float t = 0f;
    var wait = period.HasValue
      ? new UnityEngine.WaitForSeconds(period.Value)
      : null;
    do {
      t = (UnityEngine.Time.time - start) / duration;
      v.value = UnityEngine.Mathf.Lerp(startValue, targetValue, t);
      yield return wait;
    } while (t <= 1f && ! token.IsCancellationRequested);
    if (! token.IsCancellationRequested)
      v.value = targetValue;
  }

  /** When value v changes, the returned value will update over time. Good for
      displaying changing values. */
  public static IReadOnlyValue<float> LerpOnChange(this IReadOnlyValue<float> v,
                                                   UnityEngine.MonoBehaviour component,
                                                   float duration,
                                                   float? period = null) {
    var w = new Value<float>(v.value);
    v.PropertyChanged += OnChange;
    var source = new CancellationTokenSource();
    var token = source.Token;
    bool isRunning = false;
    return w;
    // var timer = new Timer(Enable, modifier, timeSpan, Timeout.InfiniteTimeSpan);
    // void OnTimer(object modifier) {
    // }
    // XXX: Need to handle this being called multiple times.
    void OnChange(object sender, PropertyChangedEventArgs args) {
      if (isRunning)
        source.Cancel();
      component.StartCoroutine(w.LerpTo(v.value, duration, period, token));
    }

    IEnumerator LerpAround() {
      try {
        isRunning = true;
        yield return w.LerpTo(v.value, duration, period, token);
        if (! source.TryReset()) {
          source.Dispose();
          source = new CancellationTokenSource();
          token = source.Token;
        }
      } finally {
        isRunning = false;
      }
    }
  }

#endif
}
}
