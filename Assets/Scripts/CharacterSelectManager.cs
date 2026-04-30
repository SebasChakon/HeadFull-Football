using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Preview cameras")]
    public Camera previewCamera1;
    public Camera previewCamera2;

    [Header("UI Jugador 1")]
    public RawImage player1Preview;
    public TMP_Text player1Name;

    [Header("UI Jugador 2")]
    public RawImage player2Preview;
    public TMP_Text player2Name;

    private GameObject[] characterPrefabs;
    private string[] characterNames;

    private int index1 = 0;
    private int index2 = 0;

    private GameObject previewInstance1;
    private GameObject previewInstance2;

    private Vector3 pos1 = new Vector3(100, 0, 0);
    private Vector3 pos2 = new Vector3(300, 0, 0);

    void Start()
    {
        LoadCharacters();

        previewCamera1.transform.position = new Vector3(100, 51.8f, 159f);
        previewCamera1.transform.rotation = Quaternion.Euler(0, 180, 0);
        previewCamera1.fieldOfView = 40f;
        previewCamera1.farClipPlane = 1000f;

        previewCamera2.transform.position = new Vector3(300, 51.8f, 159f);
        previewCamera2.transform.rotation = Quaternion.Euler(0, 180, 0);
        previewCamera2.fieldOfView = 40f;
        previewCamera2.farClipPlane = 1000f;

        if (characterPrefabs.Length > 0)
        {
            ShowCharacter(1, index1);
            ShowCharacter(2, index2);
        }
    }

    void LoadCharacters()
    {
        GameObject[] loaded = Resources.LoadAll<GameObject>("Characters");

        System.Collections.Generic.List<GameObject> validPrefabs = new();
        System.Collections.Generic.List<string> validNames = new();

        foreach (GameObject obj in loaded)
        {
            if (obj != null)
            {
                validPrefabs.Add(obj);
                validNames.Add(obj.name);
            }
        }

        characterPrefabs = validPrefabs.ToArray();
        characterNames = validNames.ToArray();
        Debug.Log("Total personajes cargados: " + characterPrefabs.Length);
    }

    void ShowCharacter(int player, int index)
    {
        if (characterPrefabs.Length == 0) return;

        if (player == 1)
        {
            if (previewInstance1 != null) Destroy(previewInstance1);
            previewInstance1 = Instantiate(characterPrefabs[index],
                new Vector3(100, 4.5f, 0f),
                Quaternion.Euler(270, 0, 90));
            previewInstance1.transform.localScale = Vector3.one * 9000f;
            FixRotation(previewInstance1);
            player1Name.text = characterNames[index];

            // Debug para ver rotaciones
            foreach (Transform t in previewInstance1.GetComponentsInChildren<Transform>())
                Debug.Log(t.name + " rotación: " + t.localEulerAngles);
        }
        else
        {
            if (previewInstance2 != null) Destroy(previewInstance2);
            previewInstance2 = Instantiate(characterPrefabs[index],
                new Vector3(300, 4.5f, 0f),
                Quaternion.Euler(270, 0, 90));
            previewInstance2.transform.localScale = Vector3.one * 9000f;
            FixRotation(previewInstance2);
            player2Name.text = characterNames[index];
        }
    }

    void FixRotation(GameObject obj)
    {
        Transform root = obj.transform.Find("Root");
        if (root == null) return;

        float xRot = root.localEulerAngles.x;
        bool needsFix = xRot < 1f || xRot > 359f;

        if (needsFix)
        {
            // Este modelo necesita corrección extra de -90 en X
            obj.transform.rotation = Quaternion.Euler(0, 90, 0);
            Debug.Log("Corregido: " + obj.name);
        }
        else
        {
            obj.transform.rotation = Quaternion.Euler(270, 0, 90);
            Debug.Log("Sin corrección: " + obj.name);
        }
    }

    public void Player1Left()
    {
        index1 = (index1 - 1 + characterPrefabs.Length) % characterPrefabs.Length;
        ShowCharacter(1, index1);
    }

    public void Player1Right()
    {
        index1 = (index1 + 1) % characterPrefabs.Length;
        ShowCharacter(1, index1);
    }

    public void Player2Left()
    {
        index2 = (index2 - 1 + characterPrefabs.Length) % characterPrefabs.Length;
        ShowCharacter(2, index2);
    }

    public void Player2Right()
    {
        index2 = (index2 + 1) % characterPrefabs.Length;
        ShowCharacter(2, index2);
    }

    public void StartGame()
    {
        PlayerPrefs.SetString("Player1Character", characterNames[index1]);
        PlayerPrefs.SetString("Player2Character", characterNames[index2]);
        SceneManager.LoadScene("Game");
    }
}