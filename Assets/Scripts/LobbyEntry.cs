using System;
using TMPro;
using UnityEngine;

public class LobbyEntry : MonoBehaviour
{
    public static Action<string> OnButtonPressed;
    [SerializeField] private TMP_Text lobbyNameText;
    private string lobbyName;

    public void Start()
    {
        lobbyName = lobbyNameText.text;
    }

    public void OnLobbyButtonClicked()
    {
        OnButtonPressed?.Invoke(lobbyName);
    }
}
