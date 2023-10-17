using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using WebSocketSharp;


public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private TMP_Text winText;
    private PhotonView _view;
    public static string roomToJoin;
    private static bool isJoined;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!roomToJoin.IsNullOrEmpty())
        {
            PhotonNetwork.JoinRoom(roomToJoin);
        }
        if (isJoined)
        {
            _view = GetComponent<PhotonView>();
            PhotonPlayerController.OnPlayerDie += CheckAlive;
            Spawn();
        }
    }

    public override void OnJoinedRoom()
    {
        isJoined = true;
        roomToJoin = null;
        Start();
    }

    private void OnDestroy()
    {
        PhotonPlayerController.OnPlayerDie -= CheckAlive;
    }
    

    void CheckAlive()
    {
        PhotonPlayerController[] controllers = FindObjectsOfType<PhotonPlayerController>();
        if (controllers.Length <= 1)
        {
            _view.RPC("GameOver", RpcTarget.All, controllers[0].name, controllers[0].playerScore);
        }
    }
    [PunRPC]
    private void GameOver(string name, int count)
    {
        winText.gameObject.SetActive(true);
        winText.text = $"Победил игрок \"{name}\", он собрал {count} монет";
        StartCoroutine(WaitForEndGame());
    }

    void Spawn()
    {
        StartCoroutine(WaitForLoad());
    }

    IEnumerator WaitForLoad()
    {
        yield return new WaitForSeconds(1);
        int index = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[index].position, spawnPoints[index].rotation);
    }
    IEnumerator WaitForEndGame()
    {
        yield return new WaitForSeconds(3);
        winText.gameObject.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
            _view.RPC("RestartScene", RpcTarget.All);
        }
    }
    [PunRPC]
    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        isJoined = false;
        PhotonNetwork.Disconnect();
        SceneLoader.SceneToLoad = 0;
        SceneManager.LoadScene(1);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            ReturnToMenu();
    }
}