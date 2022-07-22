using System.ComponentModel;
// https://weblogs.asp.net/dixin/csharp-10-new-feature-callerargumentexpression-argument-check-and-more
#if UNITY_5_3_OR_NEWER || (!NET5_0 && !NET6_0)
namespace System.Runtime.CompilerServices {

/// <summary>
/// Allows capturing of the expressions passed to a method.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.CallerArgumentExpressionAttribute" /> class.
    /// </summary>
    /// <param name="parameterName">The name of the targeted parameter.</param>
    public CallerArgumentExpressionAttribute(string parameterName) => this.ParameterName = parameterName;

    /// <summary>
    /// Gets the target parameter name of the <c>CallerArgumentExpression</c>.
    /// </summary>
    /// <returns>
    /// The name of the targeted parameter of the <c>CallerArgumentExpression</c>.
    /// </returns>
    public string ParameterName { get; }
}
}
#endif

#if NETSTANDARD || UNITY_5_3_OR_NEWER
// https://stackoverflow.com/a/62656145
namespace System.Runtime.CompilerServices {
  [EditorBrowsable(EditorBrowsableState.Never)]
  internal class IsExternalInit{}
}
#endif
