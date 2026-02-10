using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;


#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif


public class LoginManager : MonoBehaviour
{
    private string m_GooglePlayGamesToken;
    private bool m_GoogleAuthFinished;

    public static Action OnAuthenticationReady;
    private bool m_AuthReadyFired = false;


    private async void Awake()
    {
        await InitializeUnityServices();

        //OTHER INITS WOULD GO HERE

#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        LoginGooglePlayGames();
#endif
    }

    private async void Start()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Already signed in, skipping login flow.");
            return;
        }

        if (AuthenticationService.Instance.SessionTokenExists)
        {
            Debug.Log("Session token found, restoring session...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            FireAuthReadyOnce();
            return;
        }


#if UNITY_ANDROID
        // Wait for Google auth to finish (success OR failure)
        await WaitForGoogleAuth();
#endif

        if (!string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.Log("Signing in with Google Play Games...");
            await SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
        else
        {
            Debug.Log("Google login unavailable, signing in anonymously...");
            await SignInAnonymouslyAsync();
        }
    }

    private void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("Google Play Games login successful");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    m_GooglePlayGamesToken = code;
                    m_GoogleAuthFinished = true;
                });
            }
            else
            {
                Debug.LogWarning($"Google Play Games login failed: {status}");
                m_GoogleAuthFinished = true;
            }
        });
    }

    private async Task WaitForGoogleAuth(float timeoutSeconds = 5f)
    {
        float timer = 0f;

        while (!m_GoogleAuthFinished && timer < timeoutSeconds)
        {
            timer += Time.deltaTime;
            await Task.Yield();
        }

        while (!m_GoogleAuthFinished)
        {
            Debug.LogWarning("Google Play Games authentication timed out.");
        }
    }

    // --- Sign In ---
    private async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

#if UNITY_ANDROID
    
    private async Task SignInOrLinkWithGooglePlayGamesAsync()
    {
        if (string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.LogWarning("Authorization code is null or empty");
            return;
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
        else
        {
            await LinkWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
    }
    
    private async Task SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("SignIn is successful.");
            FireAuthReadyOnce();
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthentificationErrorCode
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper erroe message
            Debug.LogException(ex);
        }
    }

    private async Task LinkWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGoogleAsync(authCode);
            Debug.Log("Link is successful.");
            FireAuthReadyOnce();
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogWarning("This user is already linked with another account. Log in instead.");
        }
        catch (AuthenticationException ex)
        {
            //Compare error code to AuthenticationErrorCodes
            //Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            //Compare error code to CommonErrorCodes
            //Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    async Task UnlinkGooglePlayGamesAsync()
    {
        try
        {
            await AuthenticationService.Instance.UnlinkGooglePlayGamesAsync();
            Debug.Log("Unlink is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

#endif

    private async Task InitializeUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            Debug.Log("Initializing Unity Services...");
            await UnityServices.InitializeAsync();
        }
    }
    private void FireAuthReadyOnce()
    {
        if (m_AuthReadyFired)
            return;

        m_AuthReadyFired = true;
        Debug.Log("Authentication ready – starting cloud save logic");
        OnAuthenticationReady?.Invoke();
    }

}
