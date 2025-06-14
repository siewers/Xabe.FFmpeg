using FluentAssertions.Extensibility;

[assembly: AssertionEngineInitializer(typeof(AssertionEngineInitializer), nameof(AssertionEngineInitializer.AcknowledgeSoftWarning))]

// ReSharper disable once CheckNamespace
internal static class AssertionEngineInitializer
{
    public static void AcknowledgeSoftWarning()
    {
        License.Accepted = true;
    }
}
