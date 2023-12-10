using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private List<AudioClip> musics;

    // Start is called before the first frame update
    void Start()
    {
        searchingNewMusic = true;
        SetNewMusic();
    }

    bool searchingNewMusic;

    /// <summary>
    /// Change music when the last one ends
    /// </summary>
    private void SetNewMusic()
    {
        int currentId = -1;
        if (musicPlayer.clip != null)
            currentId = musics.IndexOf(musicPlayer.clip);

        int newId;

        do
        {
            newId = Random.Range(0, musics.Count);
        }
        while (newId == currentId);
        musicPlayer.clip = musics[newId];
        musicPlayer.Play();
        searchingNewMusic = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!musicPlayer.isPlaying && !searchingNewMusic)
        {
            searchingNewMusic = true;
            Invoke(nameof(SetNewMusic), 2f);
        }
    }
}
