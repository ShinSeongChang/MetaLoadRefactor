using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SoundManager : MonoBehaviour
{
    static public SoundManager instance { get; private set; }

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioGroup[] audioGroups;

    Dictionary<int, AudioClip> audioNodes;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }

        audioNodes = new Dictionary<int, AudioClip>();

    }

    private void Start()
    {

        for (int i = 0; i < audioGroups.Length; i++)
        {
            var nodes = audioGroups[i].GetNodes();
            var group = audioGroups[i].GetGroup();

            for (int j = 0; j < nodes.Length; j++)
            {
                audioNodes.Add((int)group + j, nodes[j].Clip);
            }
        }
    }

    public void PlaySound(GroupList group, int id)
    {
        int key = (int)group + id;
        audioSource.PlayOneShot(audioNodes[key]);
    }
}

public enum GroupList
{
    Player = 0,
    Gun=100,
    Npc=200,
    UI=300,
    Item=400
}
public enum PlayerSoundList
{
    DefaultWalk = 0,
    GlueWalk,
    Jump 
}

public enum GunSoundList
{
    ChangeMod = 0,
    Reload,
    FireSound
}

public enum NpcSoundList
{
    lowTone01 =0,
    lowTone02 = 1,
    lowTone03 = 2,
    lowTone04 = 3,
    lowTone05 = 4,
    lowTone06 = 5,
    lowTone07 = 6,
    lowTone08 = 7,
    lowTone09 = 8,
    lowTone10 = 9,
    middleTone01 = 10,
    middleTone02 = 11,
    middleTone03 = 12,
    middleTone04 = 13,
    middleTone05 = 14,
    middleTone06 = 15,
    middleTone07 = 16,
    middleTone08 = 17,
    middleTone09 = 18,
    middleTone10 = 19,
    highTone01 = 20,
    highTone02 = 21,
    highTone03 = 22,
    highTone04 = 23,
    highTone05 = 24,
    highTone06 = 25,
    highTone07 = 26,
    highTone08 = 27,
    highTone09 = 28,
    highTone10 = 29
}

public enum UISoundList
{
    ButtonClickSound_Record = 0,
    ChangePageSound,
    ButtonClickSound_Coin,
    Can_BuySound,
    Cant_BuySound,

}
public enum ItemSoundList
{
    GetRecordItemSound = 0,
    GetCoinSound,
}