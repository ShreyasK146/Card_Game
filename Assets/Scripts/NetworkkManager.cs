using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkkManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private int maxPlayers = 2;

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    private void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        PhotonNetwork.CreateRoom(null, roomOptions, null);
    }
}
