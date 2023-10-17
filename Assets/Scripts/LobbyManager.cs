using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject lobbyEntryPrefab;
    public Transform contentParent;
    [SerializeField] private Button _createButton;
    private string _name;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private GameObject ChangeNamePanel;
    private List<GameObject> currentLobbies = new List<GameObject>();

    private void Awake()
    {
        PhotonNetwork.ConnectUsingSettings();
        LobbyEntry.OnButtonPressed += JoinRoom;
    }

    private void Start()
    {
        if (SaveName())
        {
            string filePath = Path.Combine(Application.persistentDataPath, "NickName.txt");
            _name = File.ReadAllText(filePath);
        }
        else
        {
            ChangeNamePanel.SetActive(true);
            _name = "DefaultName";
        }
      PhotonNetwork.NickName =  _name;
    }

    bool SaveName()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "NickName.txt");
        if (File.Exists(filePath))
        {
            return true;
        }
        return false;
    }
    public void ChangeName(string name)
    {
        _name = name;
        string filePath = Path.Combine(Application.persistentDataPath, "NickName.txt");
        File.WriteAllText(filePath, name);
        PhotonNetwork.NickName = name;
    }
    public void JoinRoom(string roomName)
    {
        if(roomName.IsNullOrEmpty())return;
        GameManager.roomToJoin = roomName;
        LoadLoadingScene();
    }
    
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        _createButton.interactable = true;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearLobbies();

        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
                continue;

            AddLobby(room);
        }
    }

    private void ClearLobbies()
    {
        foreach (GameObject lobby in currentLobbies)
        {
            Destroy(lobby);
        }
        currentLobbies.Clear();
    }

    private void AddLobby(RoomInfo room)
    {
        GameObject lobbyGO = Instantiate(lobbyEntryPrefab, contentParent);
        lobbyGO.transform.GetChild(0).GetComponent<TMP_Text>().text = room.Name;
        currentLobbies.Add(lobbyGO);
    }
    
    private void LoadLoadingScene()
    {
        SceneLoader.SceneToLoad = 2;
        SceneManager.LoadScene(1);
    }
    public void CreateLobby()
    {
        if (!string.IsNullOrEmpty(lobbyNameInputField.text))
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 4;
            PhotonNetwork.CreateRoom(lobbyNameInputField.text, roomOptions, null);
            LoadLoadingScene();
        }
        else
        {
            Debug.LogWarning("Lobby name is empty!");
        }
    }

    private void OnDestroy()
    {
        LobbyEntry.OnButtonPressed -= JoinRoom;
    }
}


