using System;
using System.ComponentModel;
using System.Text;

namespace SeawispHunter.RolePlay.Attributes {

public class ValuedModifier<S,T> : IValuedModifier<S,T> {
  public string name { get; init; }
  public char symbol { get; init; } = '?';
  private bool _enabled = true;
  public bool enabled {
    get => _enabled;
    set {
      if (_enabled == value)
        return;
      _enabled = value;
      OnChange(nameof(enabled));
    }
  }
  public IValue<S> context => this;

  private S _value;
  public S value {
    get => _value;
    set {
      if (_value != null && _value.Equals(value))
        return;
      _value = value;
      OnChange(nameof(value));
    }
  }
  public Func<T,S,T> op { get; init; }

  public event PropertyChangedEventHandler PropertyChanged;
  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }

  public T Modify(T given) => op(given, value);

  public override string ToString() {
    var builder = new StringBuilder();
    if (name != null) {
      builder.Append('"');
      builder.Append(name);
      builder.Append('"');
      builder.Append(' ');
    }
    // if (symbol != null)
    builder.Append(symbol);

    builder.Append(_value);
    return builder.ToString();
  }
}
}