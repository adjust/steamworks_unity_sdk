using System;
using UnityEngine;

public class AdjustConfig
{
    public string AppToken { get; private set; }
    public string Environment { get; private set; }
    public MonoBehaviour MonoBehaviour { get; private set; }

    public AdjustConfig(string appToken, string environment, MonoBehaviour monoBehaviour)
    {
        if (!IsConfigValid(appToken, environment, monoBehaviour))
        {
            Debug.LogError("[Adjust]: Config is not valid");
            return;
        }

        AppToken = appToken;
        Environment = environment;
        MonoBehaviour= monoBehaviour;
    }

    #region Helper methods
    private bool IsConfigValid(string appToken, string environment, MonoBehaviour monoBehaviourInstance)
    {
        if (!IsAppTokenValid(appToken))
        {
            Debug.LogError("[Adjust]: App token is not valid");
            return false;
        }

        if (!IsEnvironmentValid(environment))
        {
            Debug.LogError("[Adjust]: Environment is not valid");
            return false;
        }

        if (!IsMonoBehaviourValid(monoBehaviourInstance))
        {
            Debug.LogError("[Adjust]: MonoBehaviour instance is not valid");
            return false;
        }

        return true;
    }

    private static bool IsAppTokenValid(string appToken)
    {
        if (string.IsNullOrEmpty(appToken))
        {
            return false;
        }

        return appToken.Length != 12;

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

    private static bool IsMonoBehaviourValid(MonoBehaviour monoBehaviour)
    {
        if (monoBehaviour == null)
        {
            return false;
        }
        return true;
    }
    #endregion
}
