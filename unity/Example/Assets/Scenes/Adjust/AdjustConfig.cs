using System;
using UnityEngine;

public class AdjustConfig
{
    //constants
    public const string EnvironmentSandbox = "sandbox";
    public const string EnvironmentProduction = "production";

    public string AppToken { get; private set; }
    public string Environment { get; private set; }
    public MonoBehaviour MonoBehaviour { get; private set; }

    #region Public API
    public AdjustConfig(string appToken, string environment, MonoBehaviour monoBehaviour)
    {
        AppToken = appToken;
        Environment = environment;
        MonoBehaviour = monoBehaviour;
    }

    public bool IsValid()
    {
        if (!IsAppTokenValid(AppToken))
        {
            return false;
        }
        if (!IsEnvironmentValid(Environment))
        {
            return false;
        }
        if (!IsMonoBehaviourValid(MonoBehaviour))
        {
            return false;
        }

        return true;
    }
    #endregion

    #region Helper methods
    private bool IsAppTokenValid(string appToken)
    {
        if (appToken == null)
        {
            Debug.LogError("[Adjust]: App token can't be null");
            return false;
        }
        if (appToken.Length != 12)
        {
            Debug.LogError("[Adjust]: App token malformed (" + appToken + ")");
            return false;
        }

        return true;
    }

    private bool IsEnvironmentValid(string environment)
    {
        if (environment == null)
        {
            Debug.LogError("[Adjust]: Environment can't be null");
            return false;
        }
        if (environment == EnvironmentSandbox)
        {
            Debug.Log("SANDBOX: Adjust is running in `sandbox` mode. Use this setting for testing. " +
                      "Don't forget to set the environment to `production` before publishing!");
            return true;
        }
        if (environment == EnvironmentProduction)
        {
            Debug.Log("PRODUCTION: Adjust is running in `production` mode. " +
                      "Use this setting only for the build that you want to publish. " +
                      "Set the environment to `sandbox` if you want to test your app!");
            return true;
        }

        Debug.LogError("[Adjust]: Environment unknown (" + environment + ")");
        return false;
    }

    private bool IsMonoBehaviourValid(MonoBehaviour monoBehaviour)
    {
        if (monoBehaviour == null)
        {
            Debug.LogError("[Adjust]: MonoBehaviour instance is not valid");
            return false;
        }

        return true;
    }
    #endregion
}
