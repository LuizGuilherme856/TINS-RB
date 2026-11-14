using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOrgan : InteractWithPlayer
{
    [SerializeField] private AudioClip[] songs;
    [SerializeField] [Range(0,1)] private float volume = 0.6f;
    [SerializeField] bool playOnce = false;


    [Header("Bonus drop")]
    [SerializeField] private Transform dropPos;
    [SerializeField] private GameObject[] drops;
    [SerializeField] private float dropCooldown = 60f;

    PlayerInputHandler inputHandler;

    private float timer = 0, playTimer;
    private bool hasPlayed = false;

    private int currentSong =0;
    [SerializeField] private AudioSource audioSource;

    private void Update()
    {
        if (timer >= 0) timer -= Time.deltaTime;
        if(playTimer>=0) playTimer -= Time.deltaTime;
    }

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        inputHandler = player.GetComponent<PlayerInputHandler>();

        if(inputHandler == null)
        {
            inputHandler = Object.FindFirstObjectByType<PlayerInputHandler>();
        }
    }

    private void OnEnable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnInteract += PlaySong;
        }
    }


    private void PlaySong()
    {
        if (playOnce && hasPlayed) return;

        if (isTouchingPlayer == false) return;
        if (playTimer >0) return;

       
 
        Debug.Log("Play organ");
        audioSource.Stop();
        audioSource.volume = volume;
        audioSource.clip = songs[currentSong];
        audioSource.Play();

        playTimer = songs[currentSong].length - 0.5f;

        currentSong++;

        if(currentSong >= songs.Length)
        {
            currentSong = 0;
            TryToSpawnDrop();
        }

        hasPlayed = true;

    }

    private void TryToSpawnDrop()
    {
        if (timer > 0) return;

        var drop = drops[Random.Range(0,drops.Length)];

        Instantiate(drop, dropPos.position, dropPos.rotation);
        timer = dropCooldown;

    }

}
