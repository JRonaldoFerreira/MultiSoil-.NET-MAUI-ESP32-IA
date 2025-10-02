namespace MultiSoil_EdgeAI.Utils;

public static class ServiceHelper
{
    public static IServiceProvider Services { get; private set; } = null!;

    public static void Initialize(IServiceProvider provider) => Services = provider;

    public static T GetService<T>() where T : notnull
        => Services.GetRequiredService<T>();
}
