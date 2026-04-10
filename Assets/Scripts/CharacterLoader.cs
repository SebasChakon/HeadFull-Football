using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    void Start()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            string key = player.actionMapName == "Player1" 
                ? "Player1Character" 
                : "Player2Character";

            string characterName = PlayerPrefs.GetString(key, "");

            if (characterName != "")
                player.LoadCharacter(characterName);
            else
                Debug.LogWarning("No se encontró personaje guardado para " + key);
        }
    }
}
