namespace UnrealSharp.Plugins;

public static class NativeAotRegistration
{
    private static readonly List<Action> PendingRegistrations = [];

    public static void Enqueue(Action registration)
    {
        PendingRegistrations.Add(registration);
    }

    internal static void RunPending()
    {
        foreach (Action registration in PendingRegistrations)
        {
            registration();
        }

        PendingRegistrations.Clear();
    }
}
