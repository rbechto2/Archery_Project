using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


[System.Serializable]
public class SerializableEvents
{
    public int Time;
    public int Event_ID;

    public SerializableEvents(int time, int event_ID)
    {
        Time = time;
        Event_ID = event_ID;
    } 


}



