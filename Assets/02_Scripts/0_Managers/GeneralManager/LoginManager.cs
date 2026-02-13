using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;


#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif


public class LoginManager : MonoBehaviour
{
    [SerializeField] private bool verboseLogging = false;

    private string m_GooglePlayGamesToken;
    private bool m_GoogleAuthFinished;

    public static Action OnAuthenticationReady;
    public Task AuthenticationTask => _authenticationTcs.Task;
    private TaskCompletionSource<bool> _authenticationTcs = new TaskCompletionSource<bool>();
    private bool m_AuthReadyFired = false;

    public static LoginManager Instance;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


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
#if UNITY_ANDROID
        // Wait for Google authentication attempt (success OR failure)
        await WaitForGoogleAuth();

        if (!string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            if (verboseLogging)
                Debug.Log("Google token available → Signing in with Google Play Games");

            try
            {
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);

                if (verboseLogging)
                {
                    Debug.Log("Google Sign-In successful");
                    Debug.Log("PlayerID: " + AuthenticationService.Instance.PlayerId);
                }
            }
            catch (AuthenticationException ex)
            {
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
            }
        }
        else
#endif
        {
            if (verboseLogging)
                Debug.Log("Google not available → Falling back to Anonymous");

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                if (verboseLogging)
                {
                    Debug.Log("Anonymous Sign-In successful");
                    Debug.Log("PlayerID: " + AuthenticationService.Instance.PlayerId);
                }
            }
            catch (AuthenticationException ex)
            {
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
            }
        }

        FireAuthReadyOnce();
    }


    private void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                if (verboseLogging)
                    Debug.Log("Google Play Games login successful");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    m_GooglePlayGamesToken = code;
                    m_GoogleAuthFinished = true;
                });

            }
            else
            {
                if (verboseLogging)
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
            if (verboseLogging)
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
            if (verboseLogging)
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
            if (verboseLogging)

                Debug.Log("Initializing Unity Services...");
            await UnityServices.InitializeAsync();
        }
    }
    private void FireAuthReadyOnce()
    {
        if (m_AuthReadyFired)
            return;

        m_AuthReadyFired = true;

        _authenticationTcs.TrySetResult(true); // complete task
        OnAuthenticationReady?.Invoke();
    }
}
