using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioGroup))]
public class AudioGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        AudioGroup audioGroup = (AudioGroup)target;

        for (int i = 0; i < audioGroup.GetNodes().Length; i++)
        {
            audioGroup.GetNodes()[i].SetId(i);
        }
       
        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}