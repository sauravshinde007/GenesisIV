using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    [SerializeField] PlayableDirector director;

    private void Start()
    {
        Cursor.visible = true;
    }

    public void Play(float time)
    {
        director.time = time;
    }
}
