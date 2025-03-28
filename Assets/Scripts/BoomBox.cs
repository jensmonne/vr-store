using UnityEngine;

public class BoomBox : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] songs;
    
    private void Start()
    {
        GetSongs();
        PlayRandomSong();
    }

    private void GetSongs()
    {
        songs = Resources.LoadAll<AudioClip>("Sounds/Boombox");

        if (songs.Length == 0)
        {
            Debug.LogError("No songs found in Resources/Sounds/Boombox!");
        }
    }

    public void PlayRandomSong()
    {
        if (songs.Length == 0) return;

        int randomIndex = Random.Range(0, songs.Length);
        audioSource.clip = songs[randomIndex];
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void NextSong()
    {
        PlayRandomSong();
    }
}
