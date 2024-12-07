using UnityEngine;
using UnityEngine.SceneManagement;

public static class DVSceneConfigs
{
    public enum SceneList { 
        INTRO,
        LOADING,

        // TODO
        MAP,
    }

    public const string INTRO_SCENE_NAME = "Intro";
    public const string LOADING_SCENE_NAME = "Loading";

    public const string MAP_SCENE_NAME = "Map";

    public static void LoadScene(SceneList sceneList) {
        SceneManager.LoadScene(GetSceneName(sceneList));
    }

    public static string GetSceneName(SceneList sceneList) {
        switch (sceneList)
        {
            case SceneList.INTRO:
                return INTRO_SCENE_NAME;
            case SceneList.LOADING:
                return LOADING_SCENE_NAME;
            case SceneList.MAP:
                return MAP_SCENE_NAME;
            default:
                Debug.LogError($"Wrong Access");
                return "";
        }
    }

}
