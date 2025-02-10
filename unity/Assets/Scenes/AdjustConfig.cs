using System;
using UnityEngine;

public class AdjustConfig
{
    public string AppToken { get; private set; }
    public string Environment { get; private set; }
    public MonoBehaviour MonoBehaviour { get; private set; }

    public AdjustConfig(string appToken, string environment, MonoBehaviour monoBehaviour)
    {
        if (IsAppTokenValid(appToken) == false)
        {
            Debug.LogError("[Adjust]: App token is not valid");
            return;
        }

        if (IsEnvironmentValid(environment) == false)
        {
            Debug.LogError("[Adjust]: Environment is not valid");
            return;
        }

        if (IsMonoBehaviourValid(monoBehaviour) == false)
        {
            Debug.LogError("[Adjust]: MonoBehaviour instance is not valid");
            return;
        }

        AppToken = appToken;
        Environment = environment;
        MonoBehaviour = monoBehaviour;
    }

    #region Helper methods
    private static bool IsAppTokenValid(string appToken)
    {
        if (string.IsNullOrEmpty(appToken))
        {
            return false;
        }
        if (appToken.Length != 12)
        {
            return false;
        }
        return true;
    }

    private static bool IsEnvironmentValid(string environment)
    {
        if (string.IsNullOrEmpty(environment))
        {
            return false;
        }
        if (environment != "sandbox" && environment != "production")
        {
            return false;
        }
        return true;
    }

    private static bool IsMonoBehaviourValid(MonoBehaviour monoBehavior)
    {
        if (monoBehavior == null)
        {
            return false;
        }
        return true;
    }
    #endregion
}
