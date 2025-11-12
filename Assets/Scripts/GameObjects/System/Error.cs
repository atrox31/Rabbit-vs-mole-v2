using UnityEngine;

public static class Error
{
    [System.Diagnostics.DebuggerNonUserCode]
    public static bool Message(string message, Object context = null)
    {
        Debug.LogError(message, context);
        return false;
    }
}