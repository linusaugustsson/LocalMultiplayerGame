using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{

    public GlobalData globalData;
    private void OnEnable() { GlobalData.gameManager = this; }

    public PlayerInputManager playerInputManager;


    private int playerCount = 0;
    public int playerID;

    public bool canStartGame = false;


    public GameObject lobbyCanvas;
    public GameObject joinText;
    public GameObject startText;
    public TextMeshPro winText;

    public GameObject lobbyCamera;

    public StartPad startPad;

    public AudioSource musicLobby;
    public AudioSource musicStage;


    private List<Player> playerList = new List<Player>();
    private List<Player> playersAliveList = new List<Player>();

    public List<Transform> lobbySpawnPoints = new List<Transform>();


    public enum Gamestate {
        Lobby,
        Game
    }

    [HideInInspector]
    public Gamestate gamestate = Gamestate.Lobby;


    void Start()
    {
        Application.targetFrameRate = 144;
        startPad.playerEnterEvent += OnPlayerEnter;
        startPad.playerLeaveEvent += OnPlayerLeave;
        globalData.audioManager.PlayMusic(musicLobby, "Lobby");
    }

    private void Update() {
        if(playerWon == true) {
            winTimer += Time.deltaTime;
            if(winTimer >= winWaitTIme) {
                GoToLobby();
                playerWon = false;
                winTimer = 0.0f;
            }
        }
    }

    public void OnPlayerJoined() {
        playerCount++;
        //lobbyCamera.SetActive(false);
        if (playerCount >= 2) {
            canStartGame = true;
            //startText.SetActive(true);
            
        }

        if(playerCount == 4) {
            joinText.SetActive(false);
        }

        playerList.Clear();

        Player[] players = FindObjectsOfType<Player>();

        for (int j = 0; j < players.Length; j++) {
            for (int i = 0; i < players.Length; i++) {
                if (players[i].GetComponent<UnityEngine.InputSystem.PlayerInput>().playerIndex == j) {
                    playerList.Add(players[i]);
                }
            }   
        }

        


    }

    public void OnPlayerLeft() {
        playerCount--;
        if(playerCount <= 1) {
            canStartGame = false;
            startText.SetActive(false);
        }
        if(playerCount == 0) {
            //lobbyCamera.SetActive(true);
        }

        joinText.SetActive(true);


        playerList.Clear();

        Player[] players = FindObjectsOfType<Player>();
        for (int j = 0; j < players.Length; j++) {
            for (int i = 0; i < players.Length; i++) {
                if (players[i].GetComponent<UnityEngine.InputSystem.PlayerInput>().playerIndex == j) {
                    playerList.Add(players[i]);
                }
            }
        }
    }

    public void StartGame() {
        globalData.audioManager.PlayAndCrossFadeMusic(musicLobby, musicStage, "Stage", 1f);

        // Start game
        joinText.SetActive(false);
        

        playerInputManager.DisableJoining();
        gamestate = Gamestate.Game;


        for (int i = 0; i < playerList.Count; i++) {
            SpawnPointManager.Respawn(playerList[i].transform);
        }


        playersAliveList.Clear();
        for (int i = 0; i < playerList.Count; i++) {
            playersAliveList.Add(playerList[i]);
        }
    }



    public void GoToLobby() {
        globalData.audioManager.PlayAndCrossFadeMusic(musicStage, musicLobby, "Lobby", 1f);

        playerInputManager.EnableJoining();
        gamestate = Gamestate.Lobby;


        for(int i = 0; i < playerList.Count; i++) {
            playerList[i].Alive();
            playerList[i].OnLobby();
        }

        PlayersRespawnLobby();

        if(playerList.Count != 4) {
            joinText.SetActive(true);
            winText.gameObject.SetActive(true);
        }


    }

    public void OnPlayerEnter() {
       if(startPad.playerCount == playerCount && playerCount > 1) {
            StartGame();
        }

    }

    public void OnPlayerLeave() {


    }

    private bool playerWon = false;

    private float winTimer = 0.0f;
    private float winWaitTIme = 5.0f;

    public void OnPlayerDeath(Player _player) {


        playersAliveList.Remove(_player);

        
        if(playersAliveList.Count == 1) {
            winText.text = "Player " + (playersAliveList[0].GetComponent<UnityEngine.InputSystem.PlayerInput>().playerIndex + 1) + " won!";


            playerWon = true;
        }
    }

    public void PlayersRespawnLobby() {
        for (int i = 0; i < playerList.Count; i++) {
            playerList[i].transform.position = lobbySpawnPoints[i].position;
        }
    }

}
