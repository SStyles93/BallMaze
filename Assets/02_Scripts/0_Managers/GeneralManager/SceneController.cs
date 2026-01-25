using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // --- Singleton ---

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


    // --- Variables --- 

    [SerializeField] private LoadingOverlay loadingOverlay;
    private Dictionary<string, string> loadedSceneBySlot = new();
    private bool isBusy = false;

    public SceneDatabase.Scenes PreviousActiveScene;

    // --- API ---

    public bool IsGameLoaded()
    {
        Scene scene = SceneManager.GetSceneByName(SceneDatabase.Scenes.Game.ToString());

        // Check the isLoaded property
        if (scene.isLoaded)
            return true;
        return false;
    }


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

        if (plan.ActiveScene != SceneManager.GetActiveScene().name)
        {
            Scene activeScene = SceneManager.GetSceneByName(plan.ActiveScene);
            SceneManager.SetActiveScene(activeScene);
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
            PreviousActiveScene = DatabaseSceneFromScene(SceneManager.GetActiveScene());
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid() && newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
            }
        }

        loadedSceneBySlot[slot] = sceneName;
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


    // --- Transition Plan Class ---
    public class SceneTransitionPlan
    {
        // Key is slotName (string), Value is SceneDatabase.Scenes enum (for internal tracking if needed)
        public Dictionary<string, string> ScenesToLoad { get; } = new();
        public List<string> SceneToUnload { get; } = new();

        public string ActiveScene;
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

        public SceneTransitionPlan SetActive(SceneDatabase.Scenes scene)
        {
            ActiveScene = scene.ToString();
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
    private SceneDatabase.Scenes DatabaseSceneFromScene(Scene scene)
    {
        if (Enum.TryParse<SceneDatabase.Scenes>(
               scene.name, out SceneDatabase.Scenes databaseScene))
        {
            return databaseScene;
        }
        else return SceneDatabase.Scenes.Null;
    }
}
