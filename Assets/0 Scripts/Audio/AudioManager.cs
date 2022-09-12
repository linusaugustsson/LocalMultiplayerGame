using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//~~~~~~~~~~~~~~~~
//
// AudioManager
//
public class AudioManager : MonoBehaviour {
    // NOTE(Patrik): I wanted to call it AudioEvent, but C# is weird and makes event a keyword.
    [System.Serializable]
    public struct AudioTask {
        public enum Type {
            Done = 0,
            FadeIn = 1,
            FadeOut = 2,
            ScheduledPlay = 3,
            ScheduledStop = 4,
            ScheduledPause = 5,
            ScheduledResume = 6,
        };

        public AudioSource source;
        public int id;
        public float time;
        public float totalTime;
        public float volume;
        public byte type;
    };

    public GlobalData globalData;
    public AudioList audioList;

    [HideInInspector]
    public AudioTask[] tasks;


    //~~~~~~~~~~~~~~~~
    //
    //
    //
    private void OnEnable() { globalData.audioManager = this; }

    void Update() { UpdateTasks(); }

    void Start() { tasks = new AudioTask[8]; }


    //~~~~~~~~~~~~~~~~
    //
    // 
    //
    public int FindSoundEffectIndex(string name) {
        for(int index = 0; index < audioList.soundEffects.Length; index += 1) {
            if(audioList.soundEffects[index].name == name) { return index; }
        }
        return -1;
    }

    public int FindMusicIndex(string name) {
        for(int index = 0; index < audioList.music.Length; index += 1) {
            if(audioList.music[index].name == name) { return index; }
        }
        return -1;
    }


    //~~~~~~~~~~~~~~~~
    //
    // ID
    //
    public int ToID(short dataIndex, ushort entryIndex) {
        int result = ((int)dataIndex << 16) | ((int)entryIndex);
        return result;
    }


    public AudioList.Data GetAudioDataFromID(int id) {
        if(id != 0) {
            short dataIndex = (short)((id >> 16) & 0xFFFF);

            if(id > 0) {
                if(dataIndex <= audioList.soundEffects.Length) { return audioList.soundEffects[dataIndex - 1]; }
            } else if(dataIndex <= audioList.music.Length) {
                return audioList.music[-dataIndex - 1];
            }
        }
        return new AudioList.Data();
    }

    public AudioList.Data GetAudioDataFromID(short dataIndex, ushort entryIndex) { return GetAudioDataFromID(ToID(dataIndex, entryIndex)); }


    public AudioList.Data.Entry GetAudioEntryFromID(int id) {
        if(id != 0) {
            AudioList.Data data = GetAudioDataFromID(id);

            if(data.entries.Length != 0) {
                ushort entryIndex = (ushort)(id & 0xFFFF);
                return data.entries[entryIndex];
            }
        }
        return new AudioList.Data.Entry();
    }

    public AudioList.Data.Entry GetAudioEntryFromID(short dataIndex, ushort entryIndex) { return GetAudioEntryFromID(ToID(dataIndex, entryIndex)); }


    public int GetDataIndexFromSoundEffect(string name) { return FindSoundEffectIndex(name) + 1; }

    public int GetDataIndexFromMusic(string name) { return -(FindMusicIndex(name) + 1); }


    //~~~~~~~~~~~~~~~~
    //
    // AudioEntry
    //
    public void SetAudioEntry(AudioSource source, AudioList.Data.Entry entry, float time = 0f) {
        source.clip = entry.clip;
        source.volume = entry.volume + 1f;
        source.pitch = Random.Range(entry.minPitch, entry.maxPitch) + 1f;
        source.spread = 180;
        source.loop = entry.looping;
        source.panStereo = entry.panStereo;
        source.spatialBlend = entry.spatialBlend + 1f;
        source.time = time;
    }


    public ushort GetRandomSoundEffectEntryIndex(int dataIndex) {
        if(dataIndex >= 0 && dataIndex < audioList.soundEffects.Length) { return (ushort)Random.Range(0, audioList.soundEffects[dataIndex].entries.Length - 1); }
        return 0;
    }

    public ushort GetRandomMusicEntryIndex(int dataIndex) {
        if(dataIndex >= 0 && dataIndex < audioList.music.Length) { return (ushort)Random.Range(0, audioList.music[dataIndex].entries.Length - 1); }
        return 0;
    }

    public ushort GetRandomAudioEntryIndex(int dataIndex) {
        if(dataIndex > 0) { return GetRandomSoundEffectEntryIndex(dataIndex - 1); }
        else if(dataIndex < 0) { return GetRandomSoundEffectEntryIndex(dataIndex - 1); }
        return 0;
    }

    public AudioList.Data.Entry GetRandomAudioEntry(AudioList.Data[] data, int dataIndex) {
        if(dataIndex >= 0 && dataIndex < data.Length) { return data[dataIndex].entries[Random.Range(0, data[dataIndex].entries.Length - 1)]; }
        return new AudioList.Data.Entry();
    }

    public AudioList.Data.Entry GetRandomAudioEntry(AudioList.Data[] data, string name) {
        for(int dataIndex = 0; dataIndex < data.Length; dataIndex += 1) {
            if(data[dataIndex].name == name) { return GetRandomAudioEntry(data, dataIndex); }
        }
        return new AudioList.Data.Entry();
    }


    public AudioList.Data.Entry GetRandomSoundEffectEntry(int index) { return GetRandomAudioEntry(audioList.soundEffects, index); }

    public AudioList.Data.Entry GetRandomSoundEffectEntry(string name) { return GetRandomAudioEntry(audioList.soundEffects, name); }


    public AudioList.Data.Entry GetRandomMusicEntry(int index) { return GetRandomAudioEntry(audioList.music, index); }

    public AudioList.Data.Entry GetRandomMusicEntry(string name) { return GetRandomAudioEntry(audioList.music, name); }


    //~~~~~~~~~~~~~~~~
    //
    // Play
    //
    public void PlayAudioEntry(AudioSource source, AudioList.Data.Entry entry) {
        if(entry.clip != null) {
            ResetTasksFromSource(source);
            SetAudioEntry(source, entry);
            if(source.clip != null) { source.Play(); }
        }
    }

    public void PlayAudioFromID(AudioSource source, int id) {
        if(id != 0) {
            AudioList.Data.Entry entry = GetAudioEntryFromID(id);
            if(entry.clip) {
                PlayAudioEntry(source, entry);
                string transitionTo = GetAudioDataFromID(id).transitionTo;
                if(transitionTo != "") {
                    if(id > 0) {
                        PlayScheduledSoundEffect(source, transitionTo, entry.clip.length);
                    } else {
                        PlayScheduledMusic(source, transitionTo, entry.clip.length);
                    }
                }
            }
        }
    }

    public void PlayAudioFromID(AudioSource source, short dataIndex, ushort entryIndex) { PlayAudioFromID(source, ToID(dataIndex, entryIndex)); }

    public void PlayAudioFromID(GameObject gameObject, int id) {
        if(gameObject != null && gameObject.TryGetComponent(out AudioSource source)) { PlayAudioFromID(source, id); }
    }


    public void PlaySoundEffect(AudioSource source, int index) {
        if(index >= 0 && index < audioList.soundEffects.Length) { PlayAudioFromID(source, ToID((short)(index + 1), GetRandomSoundEffectEntryIndex(index))); }
    }

    public void PlaySoundEffect(AudioSource source, string name) { PlaySoundEffect(source, FindSoundEffectIndex(name)); }

    public void PlaySoundEffect(GameObject gameObject, int index) {
        if(gameObject != null && gameObject.TryGetComponent(out AudioSource source)) { PlaySoundEffect(source, index); }
    }

    public void PlaySoundEffect(GameObject gameObject, string name) { PlaySoundEffect(gameObject, FindSoundEffectIndex(name)); }


    public void PlayMusic(AudioSource source, int index) {
        if(index >= 0 && index < audioList.music.Length) { PlayAudioFromID(source, ToID((short)(-(index + 1)), GetRandomMusicEntryIndex(index))); }
    }

    public void PlayMusic(AudioSource source, string name) { PlayMusic(source, FindMusicIndex(name)); }

    public void PlayMusic(GameObject gameObject, int index) {
        if(gameObject != null && gameObject.TryGetComponent(out AudioSource source)) { PlayMusic(source, index); }
    }

    public void PlayMusic(GameObject gameObject, string name) { PlayMusic(gameObject, FindMusicIndex(name)); }


    //~~~~~~~~~~~~~~~~
    //
    // PlayAndDestroy
    //
    public void PlayAudioEntryAndDestroy(Vector3 position, AudioList.Data.Entry entry) {
        if(entry.clip && !entry.looping) {
            GameObject newObject = Instantiate(audioList.prefab, position, Quaternion.identity);
            Destroy(newObject, entry.clip.length);
            PlayAudioEntry(newObject.GetComponent<AudioSource>(), entry);
        }
    }

    public void PlayAudioEntryAndDestroy(Transform parent, AudioList.Data.Entry entry) {
        if(entry.clip && !entry.looping) {
            GameObject newObject = Instantiate(audioList.prefab, parent);
            Destroy(newObject, entry.clip.length);
            PlayAudioEntry(newObject.GetComponent<AudioSource>(), entry);
        }
    }


    public void PlayAudioFromIDAndDestroy(Vector3 position, int id) {
        PlayAudioEntryAndDestroy(position, GetAudioEntryFromID(id));
    }

    public void PlayAudioFromIDAndDestroy(Transform parent, int id) {
        PlayAudioEntryAndDestroy(parent, GetAudioEntryFromID(id));
    }


    public void PlaySoundEffectAndDestroy(Vector3 position, int index) {
        if(index >= 0 && index < audioList.soundEffects.Length) { PlayAudioEntryAndDestroy(position, GetRandomSoundEffectEntry(index)); }
    }

    public void PlaySoundEffectAndDestroy(Transform parent, int index) {
        if(index >= 0 && index < audioList.soundEffects.Length) { PlayAudioEntryAndDestroy(parent, GetRandomSoundEffectEntry(index)); }
    }

    public void PlaySoundEffectAndDestroy(Vector3 position, string name) { PlaySoundEffectAndDestroy(position, FindSoundEffectIndex(name)); }

    public void PlaySoundEffectAndDestroy(Transform parent, string name) { PlaySoundEffectAndDestroy(parent, FindSoundEffectIndex(name)); }


    public void PlayMusicAndDestroy(Vector3 position, int index) {
        if(index >= 0 && index < audioList.music.Length) { PlayAudioEntryAndDestroy(position, GetRandomSoundEffectEntry(index)); }
    }

    public void PlayMusicAndDestroy(Transform parent, int index) {
        if(index >= 0 && index < audioList.music.Length) { PlayAudioEntryAndDestroy(parent, GetRandomSoundEffectEntry(index)); }
    }

    public void PlayMusicAndDestroy(Vector3 position, string name) { PlayMusicAndDestroy(position, FindSoundEffectIndex(name)); }

    public void PlayMusicAndDestroy(Transform parent, string name) { PlayMusicAndDestroy(parent, FindSoundEffectIndex(name)); }


    //~~~~~~~~~~~~~~~~
    //
    // PlayAndCrossFade
    //
    public void PlayAndCrossFadeAudioEntry(AudioSource sourceFrom, AudioSource sourceTo, AudioList.Data.Entry entry, float time) {
        PlayAudioEntry(sourceTo, entry);
        AppendCrossFade(sourceFrom, sourceTo, time);
    }

    public void PlayAndCrossFadeAudioFromID(AudioSource sourceFrom, AudioSource sourceTo, int id, float time) {
        AudioList.Data.Entry entry = GetAudioEntryFromID(id);
        PlayAndCrossFadeAudioEntry(sourceFrom, sourceTo, entry, time);
    }

    public void PlayAndCrossFadeAudioFromID(AudioSource sourceFrom, AudioSource sourceTo, short dataIndex, ushort entryIndex, float time) {
        PlayAndCrossFadeAudioFromID(sourceFrom, sourceTo, ToID(dataIndex, entryIndex), time);
    }


    public void PlayAndCrossFadeSoundEffect(AudioSource sourceFrom, AudioSource sourceTo, int index, float time) {
        if(index >= 0 && index < audioList.soundEffects.Length) { PlayAndCrossFadeAudioEntry(sourceFrom, sourceTo, GetRandomSoundEffectEntry(index), time); }
    }

    public void PlayAndCrossFadeSoundEffect(AudioSource sourceFrom, AudioSource sourceTo, string name, float time) {
        PlayAndCrossFadeSoundEffect(sourceFrom, sourceTo, FindSoundEffectIndex(name), time);
    }


    public void PlayAndCrossFadeMusic(AudioSource sourceFrom, AudioSource sourceTo, int index, float time) {
        if(index >= 0 && index < audioList.music.Length) { PlayAndCrossFadeAudioEntry(sourceFrom, sourceTo, GetRandomMusicEntry(index), time); }
    }

    public void PlayAndCrossFadeMusic(AudioSource sourceFrom, AudioSource sourceTo, string name, float time) {
        PlayAndCrossFadeMusic(sourceFrom, sourceTo, FindMusicIndex(name), time);
    }


    //~~~~~~~~~~~~~~~~
    //
    // PlayScheduled
    //
    public void PlayScheduledAudioFromID(AudioSource source, int id, float time) { AppendScheduledPlay(source, id, time); }

    public void PlayScheduledAudioFromID(AudioSource source, short dataIndex, ushort entryIndex, float time) { AppendScheduledPlay(source, ToID(dataIndex, entryIndex), time); }


    public void PlayScheduledSoundEffect(AudioSource source, int index, float time) {
        PlayScheduledAudioFromID(source, ToID((short)(index + 1), GetRandomSoundEffectEntryIndex(index)), time);
    }

    public void PlayScheduledSoundEffect(AudioSource source, string name, float time) {
        PlayScheduledSoundEffect(source, FindSoundEffectIndex(name), time);
    }

    public void PlayScheduledSoundEffect(GameObject gameObject, int index, float time) {
        if(gameObject != null && gameObject.TryGetComponent(out AudioSource source)) { PlayScheduledSoundEffect(source, index, time); }
    }

    public void PlayScheduledSoundEffect(GameObject gameObject, string name, float time) {
        PlayScheduledSoundEffect(gameObject, FindSoundEffectIndex(name), time);
    }


    public void PlayScheduledMusic(AudioSource source, int index, float time) {
        PlayScheduledAudioFromID(source, ToID((short)(-(index + 1)), GetRandomMusicEntryIndex(index)), time);
    }

    public void PlayScheduledMusic(AudioSource source, string name, float time) {
        PlayScheduledMusic(source, FindMusicIndex(name), time);
    }

    public void PlayScheduledMusic(GameObject gameObject, int index, float time) {
        if(gameObject != null && gameObject.TryGetComponent(out AudioSource source)) { PlayScheduledMusic(source, index, time); }
    }

    public void PlayScheduledMusic(GameObject gameObject, string name, float time) {
        PlayScheduledMusic(gameObject, FindMusicIndex(name), time);
    }


    //~~~~~~~~~~~~~~~~
    //
    // AudioTask
    //
    public void AppendTask(AudioTask task) {
        if(task.type != (byte)AudioTask.Type.Done && task.source != null) {
            for(int taskIndex = 0; taskIndex < tasks.Length; taskIndex += 1) {
                if(tasks[taskIndex].type == (byte)AudioTask.Type.Done) {
                    tasks[taskIndex] = task;
                    break;
                }
            }
        }
    }

    public void AppendTask(AudioTask.Type type, AudioSource source, int id, float time, float volume = 0f) {
        AudioTask task = new AudioTask();
        task.type = (byte)type;
        task.source = source;
        task.id = id;
        task.totalTime = time;
        task.volume = volume;
        AppendTask(task);
    }

    public void AppendScheduledPlay(AudioSource source, int id, float time) {
        AppendTask(AudioTask.Type.ScheduledPlay, source, id, time);
    }

    public void AppendScheduledStop(AudioSource source, float time) {
        AppendTask(AudioTask.Type.ScheduledStop, source, 0, time);
    }

    public void AppendFadeIn(AudioSource source, float time, float volumeAtEnd = -1f) {
        if(volumeAtEnd <= 0f) {
            volumeAtEnd = 0f;
            for(int dataIndex = 0; dataIndex < audioList.music.Length; dataIndex += 1) {
                for(int entryIndex = 0; entryIndex < audioList.music[dataIndex].entries.Length; entryIndex += 1) {
                    if(source.clip == audioList.music[dataIndex].entries[entryIndex].clip) {
                        volumeAtEnd = audioList.music[dataIndex].entries[entryIndex].volume + 1f;
                        break;
                    }
                }
            }

            if(volumeAtEnd == 0f) {
                for(int dataIndex = 0; dataIndex < audioList.soundEffects.Length; dataIndex += 1) {
                    for(int entryIndex = 0; entryIndex < audioList.soundEffects[dataIndex].entries.Length; entryIndex += 1) {
                        if(source.clip == audioList.soundEffects[dataIndex].entries[entryIndex].clip) {
                            volumeAtEnd = audioList.soundEffects[dataIndex].entries[entryIndex].volume + 1f;
                            break;
                        }
                    }
                }
            }
        }

        AppendTask(AudioTask.Type.FadeIn, source, 0, time, volumeAtEnd);
        source.volume = 0f;
    }

    public void AppendFadeOut(AudioSource source, float time, bool stopFadedSource = true) {
        AppendTask(AudioTask.Type.FadeOut, source, 0, time, source.volume);
        if(stopFadedSource) { AppendScheduledStop(source, time); }
    }

    public void AppendCrossFade(AudioSource sourceFrom, AudioSource sourceTo, float time, bool stopFadedSource = true, float volumeAtEnd = -1f) {
        AppendFadeOut(sourceFrom, time, stopFadedSource);
        AppendFadeIn(sourceTo, time, volumeAtEnd);
    }

    public void ResetTasksFromSource(AudioSource source) {
        for(int taskIndex = 0; taskIndex < tasks.Length; taskIndex += 1) {
            if(tasks[taskIndex].type != (byte)AudioTask.Type.Done && tasks[taskIndex].source == source) {
                switch((AudioTask.Type)tasks[taskIndex].type) {
                    case AudioTask.Type.FadeIn: { tasks[taskIndex].source.volume = tasks[taskIndex].volume; }
                    break;

                    case AudioTask.Type.FadeOut: { tasks[taskIndex].source.volume = 0; }
                    break;
                }

                tasks[taskIndex].type = (byte)AudioTask.Type.Done;
            }
        }
    }

    public void UpdateTasks() {
        for(int taskIndex = 0; taskIndex < tasks.Length; taskIndex += 1) {
            if(tasks[taskIndex].type != (byte)AudioTask.Type.Done) {
                switch((AudioTask.Type)tasks[taskIndex].type) {
                    case AudioTask.Type.FadeIn: {
                        float t = tasks[taskIndex].time / tasks[taskIndex].totalTime;
                        tasks[taskIndex].source.volume = Mathf.Lerp(0f, tasks[taskIndex].volume, t);
                    }
                    break;

                    case AudioTask.Type.FadeOut: {
                        float t = tasks[taskIndex].time / tasks[taskIndex].totalTime;
                        tasks[taskIndex].source.volume = Mathf.Lerp(tasks[taskIndex].volume, 0f, t);
                    }
                    break;
                }

                if(tasks[taskIndex].time >= tasks[taskIndex].totalTime) {
                    switch((AudioTask.Type)tasks[taskIndex].type) {
                        case AudioTask.Type.FadeIn: { tasks[taskIndex].source.volume = tasks[taskIndex].volume; }
                        break;

                        case AudioTask.Type.FadeOut: { tasks[taskIndex].source.volume = 0f; }
                        break;

                        case AudioTask.Type.ScheduledPlay: { PlayAudioFromID(tasks[taskIndex].source, tasks[taskIndex].id); }
                        break;

                        case AudioTask.Type.ScheduledStop: {
                            tasks[taskIndex].source.Stop();
                            ResetTasksFromSource(tasks[taskIndex].source);
                        }
                        break;

                        case AudioTask.Type.ScheduledPause: { tasks[taskIndex].source.Pause(); }
                        break;

                        case AudioTask.Type.ScheduledResume: { tasks[taskIndex].source.UnPause(); }
                        break;
                    }

                    tasks[taskIndex].type = (byte)AudioTask.Type.Done;
                }

                tasks[taskIndex].time += Time.deltaTime;
            }
        }
    }
}
