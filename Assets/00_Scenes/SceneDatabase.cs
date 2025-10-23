using UnityEngine;

public class SceneDatabase : MonoBehaviour
{
    public enum Slots
    {
        Menu,
        //The content (level the player is loaded in)
        Content
    }

    public enum Scenes
    {
        Null = -1,
        Core,
        MainMenu,
        GamesMenu,
        Game
    }
}
