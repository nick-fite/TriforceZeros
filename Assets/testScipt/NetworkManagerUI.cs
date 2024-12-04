using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    string _playerName;
    private void Awake()
    {
        serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }
    public void NameChanged(string newName)
    {
        _playerName = newName;
        if (!NetworkManager.Singleton.LocalClient.PlayerObject)
        {
            return;
        }
        GameObject playerObj = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        if (!playerObj)
        {
            return;
        }
        NameComponent playerNameComponent = playerObj.GetComponent<NameComponent>();
        if (playerNameComponent)
        { 
            playerNameComponent.OnNameChanged?.Invoke(newName);
        }
    }
    public string GetPlayerName() 
    {
        return _playerName;
    }
}
