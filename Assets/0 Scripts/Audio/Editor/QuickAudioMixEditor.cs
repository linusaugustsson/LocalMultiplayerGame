using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//~~~~~~~~~~~~~~~~
//
// QuickAudioMixEditor
//
[CanEditMultipleObjects]
[CustomEditor(typeof(QuickAudioMix))]
public class QuickAudioMixEditor : Editor {
    public SerializedProperty soundEffectsName;
    public SerializedProperty musicName;
    public SerializedProperty crossFadeMusic;
    public SerializedProperty crossFadeAtSameLength;
    public SerializedProperty crossFadeLength;


    void DisplayAudioPlayer(ref QuickAudioMix it, SerializedProperty name, AudioList.Data[] list, bool isMusic) {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.Width(60));
        name.stringValue = EditorGUILayout.TextField(name.stringValue);
        GUILayout.EndHorizontal();

        int value = 0;

        string[] names = new string[list.Length];
        for(int index = 0; index < list.Length; index += 1) {
            names[index] = list[index].name;

            if(name.stringValue == list[index].name) {
                value = index;
            }
        }

        int newValue = EditorGUILayout.Popup(value, names);
        if(newValue != value) { name.stringValue = names[newValue]; }

        if(Application.isPlaying) {
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Play")) {
                if(isMusic) {
                    if(it.crossFadeMusic) {
                        if(it.musicA.isPlaying) {
                            it.audioManager.PlayMusic(it.musicB, it.musicName);
                            it.audioManager.AppendCrossFade(it.musicA, it.musicB, it.crossFadeLength);
                            if(it.crossFadeAtSameLength) {
                                it.musicB.time = it.musicA.time;
                            }
                        } else if(it.musicB.isPlaying) {
                            it.audioManager.PlayMusic(it.musicA, it.musicName);
                            it.audioManager.AppendCrossFade(it.musicB, it.musicA, it.crossFadeLength);
                            if(it.crossFadeAtSameLength) {
                                it.musicA.time = it.musicB.time;
                            }
                        } else {
                            it.audioManager.PlayMusic(it.musicA, it.musicName);
                        }
                    } else {
                        it.audioManager.PlayMusic(it.musicA, it.musicName);
                        it.audioManager.ResetTasksFromSource(it.musicB);
                        it.musicB.Stop();
                    }
                } else {
                    it.audioManager.PlaySoundEffect(it.sfx, it.soundEffectName);
                }
            }

            if(GUILayout.Button("Stop")) {
                if(isMusic) {
                    it.audioManager.ResetTasksFromSource(it.musicA);
                    it.audioManager.ResetTasksFromSource(it.musicB);
                    it.musicA.Stop();
                    it.musicB.Stop();
                } else {
                    it.audioManager.ResetTasksFromSource(it.sfx);
                    it.sfx.Stop();
                }
            }
            GUILayout.EndHorizontal();

            if(isMusic) {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Fade In")) {
                    it.audioManager.PlayMusic(it.musicA, it.musicName);
                    it.audioManager.AppendFadeIn(it.musicA, it.crossFadeLength);
                    it.musicB.Stop();
                }

                if(GUILayout.Button("Fade Out")) {
                    it.audioManager.AppendFadeOut(it.musicA, it.crossFadeLength);
                    it.audioManager.AppendFadeOut(it.musicB, it.crossFadeLength);
                }
                GUILayout.EndHorizontal();
            }
        }
    }

    void OnEnable() {
        soundEffectsName = serializedObject.FindProperty("soundEffectName");
        musicName = serializedObject.FindProperty("musicName");

        crossFadeMusic = serializedObject.FindProperty("crossFadeMusic");
        crossFadeAtSameLength = serializedObject.FindProperty("crossFadeAtSameLength");
        crossFadeLength = serializedObject.FindProperty("crossFadeLength");
    }

    public override void OnInspectorGUI() {
        QuickAudioMix it = (QuickAudioMix)target;

        if(it.globalData != null) {
            serializedObject.Update();

            if(it.globalData.audioManager != null) {

                GUILayout.Label("Sound Effects", EditorStyles.boldLabel);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                DisplayAudioPlayer(ref it, soundEffectsName, it.globalData.audioManager.audioList.soundEffects, false);
                GUILayout.EndVertical();

                GUILayout.Space(4);

                GUILayout.Label("Music", EditorStyles.boldLabel);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                DisplayAudioPlayer(ref it, musicName, it.globalData.audioManager.audioList.music, true);

                EditorGUILayout.PropertyField(crossFadeMusic);
                EditorGUILayout.PropertyField(crossFadeAtSameLength);
                crossFadeLength.floatValue = EditorGUILayout.Slider(crossFadeLength.displayName, crossFadeLength.floatValue, 0f, 30f);

                GUILayout.Space(4);

                GUILayout.EndVertical();
            } else {
                GUILayout.Label("Audio Manager", EditorStyles.boldLabel);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                it.globalData.audioManager = (AudioManager)EditorGUILayout.ObjectField("Audio Manager", null, typeof(AudioManager), true);
                GUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
            GUILayout.Space(8f);
        }

        GUILayout.Label("Misc.", EditorStyles.boldLabel);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        base.OnInspectorGUI();
        GUILayout.EndVertical();
    }
}
