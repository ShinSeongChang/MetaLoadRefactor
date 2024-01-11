using UnityEngine;
using System.Collections.Generic;

public class AudioGroup : MonoBehaviour
{
    [SerializeField]
    GroupList group;

    [SerializeField]
    AudioNode[] nodes;

    public GroupList GetGroup() { return group; }
    public AudioNode[] GetNodes() { return nodes; }



    [System.Serializable]
    public class AudioNode
    {
        public int Id { get; private set; }
        public void SetId(int value) { Id = value; }

        public AudioClip Clip;
    }
}
