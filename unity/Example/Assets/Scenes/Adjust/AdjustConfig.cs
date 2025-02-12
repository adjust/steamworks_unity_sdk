using System;
using UnityEngine;

public class AdjustConfig
{
    public string AppToken { get; private set; }
    public AdjustEnvironment Environment { get; private set; }
    public MonoBehaviour MonoBehaviour { get; private set; }

    public AdjustConfig(string appToken, AdjustEnvironment environment, MonoBehaviour monoBehaviour)
    {
        if (!IsAppTokenValid(appToken))
        {
            Debug.LogError("[Adjust]: App token is not valid");
            return;
        }

        if (!IsEnvironmentValid(environment))
        {
            Debug.LogError("[Adjust]: Environment is not valid");
            return;
        }

        if (!IsMonoBehaviourValid(monoBehaviour))
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

        return appToken.Length == 12;
    }

    private static bool IsEnvironmentValid(AdjustEnvironment environment)
    {
        bool isEnvironmentDefined = environment == AdjustEnvironment.Sandbox || environment == AdjustEnvironment.Production;
        if (!isEnvironmentDefined)
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
