namespace Commar.CubicLand
{
    public static class SceneList
    {
        public enum Scene
        {
            Intro,
            World
        }

        public static string GetSceneName(Scene scene)
        {
            switch (scene)
            {
                case Scene.Intro:
                    return "Intro";
                case Scene.World:
                    return "World";
            }

            return null;
        }
    }
}