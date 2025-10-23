using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    #region Singleton
    public static SceneController Instance;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }
    #endregion

    [SerializeField] private LoadingOverlay loadingOverlay;

    // Changed to track scenes by their actual name, as SessionManager will provide names
    private Dictionary<string, string> loadedSceneBySlot = new();
    private bool isBusy = false;

    public event Action OnSceneLoadComplete;

    // API

    public SceneTransitionPlan NewTransition()
    {
        return new SceneTransitionPlan();
    }

    private IEnumerator ExecutePlan(SceneTransitionPlan plan)
    {
        if (isBusy)
        {
            Debug.LogWarning("Scene change already in progress");
            yield break;
        }
        isBusy = true;
        yield return StartCoroutine(ChangeSceneRoutine(plan));
    }

    private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
    {
        if (plan.Overlay)
        {
            yield return loadingOverlay.FadeInBlack();
        }

        // Unload scenes specified in the plan
        foreach (var sceneName in plan.SceneToUnload)
        {
            yield return UnloadSceneRoutine(sceneName);
        }

        if (plan.ClearUnusedAssets) yield return CleanupUnusedAssetsRoutine();

        // Load scenes specified in the plan
        foreach (var kvp in plan.ScenesToLoad)
        {
            string sceneSlot = kvp.Key; // Slot
            string sceneName = kvp.Value; //Scene

            // SCENE IS ALREADY LOADED
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                if (plan.ActiveScene == sceneName)
                {
                    Scene newScene = SceneManager.GetSceneByName(sceneName);
                    if (newScene.IsValid() && newScene.isLoaded)
                    {
                        SceneManager.SetActiveScene(newScene);
                    }
                }
            }
            // SCENE NOT YET LOADED
            else
            {
                yield return LoadAdditiveRoutine(sceneSlot, sceneName, plan.ActiveScene == sceneName);
            }
        }

        if (plan.Overlay)
        {
            yield return loadingOverlay.FadeOutBlack();
        }
        isBusy = false;
    }

    private IEnumerator LoadAdditiveRoutine(string slot, string sceneName, bool setActive)
    {
        // Use sceneName directly for loading, as sceneIndex might not be reliable if not in build settings
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null) yield break;
        loadOp.allowSceneActivation = false;
        while (loadOp.progress < 0.9f)
        {
            yield return null;
        }

        loadOp.allowSceneActivation = true;
        while (!loadOp.isDone)
        {
            yield return null;
        }

        if (setActive)
        {
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid() && newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
            }
        }
    }

    private IEnumerator UnloadSceneRoutine(string sceneName)
    {
        // Check if the scene is actually loaded before attempting to unload
        Scene sceneToUnload = SceneManager.GetSceneByName(sceneName);
        if (!sceneToUnload.IsValid() || !sceneToUnload.isLoaded) yield break;

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
        if (unloadOp != null)
        {
            while (!unloadOp.isDone)
            {
                yield return null;
            }
        }

        // Remove from loadedSceneBySlot
        foreach (var kvp in loadedSceneBySlot) // safe copy for iteration
        {
            if (kvp.Value.Contains(sceneName))
            {
                loadedSceneBySlot.Remove(kvp.Key);
                break; // scene found, stop
            }
        }
    }

    private IEnumerator CleanupUnusedAssetsRoutine()
    {
        AsyncOperation cleanupOp = Resources.UnloadUnusedAssets();
        while (!cleanupOp.isDone)
        {
            yield return null;
        }
    }

    // Transition Plan Class
    public class SceneTransitionPlan
    {
        // Key is slotName (string), Value is SceneDatabase.Scenes enum (for internal tracking if needed)
        public Dictionary<string, string> ScenesToLoad { get; } = new();
        public List<string> SceneToUnload { get; } = new(); // List of scene names to unload

        public string ActiveScene;// Renamed to avoid confusion with string name
        public bool ClearUnusedAssets { get; private set; } = false;
        public bool Overlay { get; private set; } = false;

        public SceneTransitionPlan Load(SceneDatabase.Slots slot, SceneDatabase.Scenes scene, bool setActive = true)
        {
            ScenesToLoad[slot.ToString()] = scene.ToString();
            if (setActive)
            {
                ActiveScene = scene.ToString();
            }
            return this;
        }
        public SceneTransitionPlan Unload(SceneDatabase.Scenes scene)
        {
            SceneToUnload.Add(scene.ToString());
            return this;
        }
        public SceneTransitionPlan WithOverlay()
        {
            Overlay = true;
            return this;
        }
        public SceneTransitionPlan WithClearUnusedAssets()
        {
            ClearUnusedAssets = true;
            return this;
        }

        public void Perform()
        {
            SceneController.Instance.StartCoroutine(SceneController.Instance.ExecutePlan(this));
        }
    }

    // Helper to get scene name from enum (assuming SceneDatabase.Scenes is an enum of scene names)
    private string GetSceneNameFromEnum(SceneDatabase.Scenes sceneEnum)
    {
        return sceneEnum.ToString();
    }

    private SceneDatabase.Scenes GetEnumFromSceneName(string sceneName)
    {
        Enum.TryParse(sceneName, out SceneDatabase.Scenes scene);
        return scene;
    }
}
