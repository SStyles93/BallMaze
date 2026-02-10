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
    private string m_GooglePLayGamesToken;

    public static Action<bool> OnGoogleLogin;


    private void Awake()
    {


#region UNITY

        //if (UnityServices.State == ServicesInitializationState.Uninitialized)
        //{
        //    Debug.Log("Services Initializing");
        //    await UnityServices.InitializeAsync();
        //}

        //PlayerAccountService.Instance.SignedIn += SignInOrLinkWithUnity;
        
#endregion

#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        LoginGooglePlayGames();
#endif
        // FACEBOOK INIT WOULD GO HERE
    }

    private void Start()
    {
        StartSignInWithGooglePlayGames();
        //// --- Unity ----
        //if (!AuthenticationService.Instance.SessionTokenExists)
        //{
        //    Debug.Log("Session Token not found");
        //    return;
        //}
        //Debug.Log("Returning player signing in...");
        //await SignInAnonymouslyAsync();
    }

    // --- PUBLIC ---

    // ------ Anonymous ------
    public async void StartAnonymousSignIn()
    {
        await SignInAnonymouslyAsync();
    }

    // ------ Unity ------
    public async void StartUnitySignInAsync()
    {
        if (PlayerAccountService.Instance.IsSignedIn)
        {
            SignInOrLinkWithUnity();
            return;
        }
        try
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        }catch(RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }
    public void SignOutUnity(bool clearSessionToken = false)
    {
        // Sign out of Unity Authentication, with the option to clear the session token
        AuthenticationService.Instance.SignOut(clearSessionToken);

        // Sign out of Unity Player Accounts
        PlayerAccountService.Instance.SignOut();
    }

    // ------ Google ------
    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("Login wiht Google Play game successful");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("Authorization code: " + code);
                    m_GooglePLayGamesToken = code;
                    // Token example used by the SignInWithGooglePlayGames
                });
            }
            else
            {
                Debug.Log($"Google Play Games login unsuccessful, status: {status}");
            }
        });
    }
    public void StartSignInWithGooglePlayGames()
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.LogWarning("Not yet authenticated with Google Play Games -- attempting login again");
            LoginGooglePlayGames();
            return;
        }
        SignInOrLinkWithGooglePlayGames();
    }

    // --- PRIVATE METHODS ---

    // ------ Anonymous ------

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


    // ------ Unity ------

    async void StartPlayerAccountsSignInAsync()
    {
        if (PlayerAccountService.Instance.IsSignedIn)
        {
            // If the player is already signed into Unity Player Accounts, proceed directly to the Unity Authentication sign-in.
            await SignInWithUnityAuth();
            return;
        }

        try
        {
            // This will open the system browser and prompt the user to sign in to Unity Player Accounts
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (PlayerAccountsException ex)
        {
            // Compare error code to PlayerAccountsErrorCodes
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

    async void SignInOrLinkWithUnity()
    {
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Signing up with Unity Player Account...");
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Successfully signed up with Unity Player Account");
                return;
            }
            if (!HasUnityID()) 
            {
                Debug.Log("Linkin anonymous account to Unity...");
                await LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Successfully linked anonymous account!");
            }

            Debug.Log("Player is already signed in to their Unity Player Account");
        }
        catch(RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    async Task SignInWithUnityAuth()
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
            Debug.Log("SignIn is successful.");
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

    async Task LinkWithUnityAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);
            Debug.Log("Link is successful.");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
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

    async Task UnlinkUnityAsync()
    {
        try
        {
            await AuthenticationService.Instance.UnlinkUnityAsync();
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

    private bool HasUnityID()
    {
        return AuthenticationService.Instance.PlayerInfo.GetUnityId() != null;
    }

    // ------ Google Play Games ------

#if UNITY_ANDROID

    private async void SignInOrLinkWithGooglePlayGames()
    {
        if (string.IsNullOrEmpty(m_GooglePLayGamesToken)){
            Debug.LogWarning("Authorization code is null or empty");
            return;
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await SignInWithGooglePlayGamesAsync(m_GooglePLayGamesToken);
        }
        else
        {
            await LinkWithGooglePlayGamesAsync(m_GooglePLayGamesToken);
        }
    }

    private async Task SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("SignIn is successful.");
            OnGoogleLogin?.Invoke(true);
        }
        catch(AuthenticationException ex)
        {
            // Compare error code to AuthentificationErrorCode
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch(RequestFailedException ex)
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
            OnGoogleLogin?.Invoke(true);
        }
        catch(AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogWarning("This user is already linked with another account. Log in instead.");
        }
        catch(AuthenticationException ex)
        {
            //Compare error code to AuthenticationErrorCodes
            //Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch(RequestFailedException ex)
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
            OnGoogleLogin?.Invoke(false);
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
}
