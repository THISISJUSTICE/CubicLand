using UnityEngine.SceneManagement;

public static class DVSceneConfigs
{
    public enum SceneList
    {
        Intro,
        Loading,

        // TODO
        Map,
    }

    public const string INTRO_SCENE_NAME = "Intro";
    public const string LOADING_SCENE_NAME = "Loading";

    public const string MAP_SCENE_NAME = "Map";

    public static void LoadScene(SceneList sceneList)
    {
        SceneManager.LoadScene(GetSceneName(sceneList));
    }

    public static string GetSceneName(SceneList sceneList)
    {
        switch (sceneList)
        {
            default:
            case SceneList.Intro:
                return INTRO_SCENE_NAME;
            case SceneList.Loading:
                return LOADING_SCENE_NAME;
            case SceneList.Map:
                return MAP_SCENE_NAME;
        }
    }

}
