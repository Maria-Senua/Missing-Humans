using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VoicePlayTrigger : MonoBehaviour
{
    public AudioSource trigSource;
    public AudioClip[] voiceList;
    public GameObject[] subtitles;

    public static VoicePlayTrigger instance;

    private void Awake()
    {
        //if (instance == null)
        //{
            instance = this;
            //DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        //}
        //else
        //{
        //    Destroy(gameObject); // Prevent duplicates
        //}
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🔄 Scene {scene.name} loaded. Checking AudioSource...");

        if (trigSource == null)
        {
            GameObject player = GameObject.FindWithTag("Player"); 

            if (player != null)
            {
                trigSource = player.GetComponent<AudioSource>();

                if (trigSource == null)
                {
                    Debug.LogError("Player found, but no AudioSource attached!");
                }
                else
                {
                    Debug.Log("AudioSource assigned from Player after scene load!");
                }
            }
            else
            {
                Debug.LogError("Player GameObject not found in scene!");
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; //Unregister OnSceneLoaded to prevent errors
    }

    public void PlayCatVoice(int index)
    {
        trigSource.PlayOneShot(voiceList[index]);
        subtitles[index].SetActive(true);

        StartCoroutine(RemoveSub(index, voiceList[index].length));
    }

    private IEnumerator RemoveSub(int index, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        subtitles[index].SetActive(false);
    }
}
