using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine.XR;
using System.Linq;

public class Save_to_text : MonoBehaviour
{
    StreamWriter writer;
    List<InputDevice> devices = new List<InputDevice>();
    string path = Application.streamingAssetsPath + "/data_logs/" + "test1.txt";
    public static ConcurrentQueue<string> data_queue = new ConcurrentQueue<string>();
    public static Stopwatch stopwatch = new Stopwatch();
    private string current_data = null;
    
    
    void Start()  
    {
        stopwatch.Start();
        InputDevices.GetDevices(devices);
        Directory.CreateDirectory(Application.streamingAssetsPath + "/data_logs/");
        writer = new StreamWriter(path, true);
    }

    void Update()
    { 
        if (stopwatch.ElapsedMilliseconds % 100 == 0)
        {
            WriteToFile();
        }

        InputDevices.GetDevices(devices);
        foreach (var item in devices)
        {
            //Get Device Position
            item.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition);
            Add_to_Queue("time: "+stopwatch.ElapsedMilliseconds.ToString() + ","+ item.name+ ", Position: " + devicePosition.ToString());

            //Get Device Rotation
            item.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation);
            Add_to_Queue("time: " + stopwatch.ElapsedMilliseconds.ToString() + "," + item.name + ", Rotation: " + deviceRotation.ToString());

            //Get Device Velocity
            item.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 deviceVelocity);
            Add_to_Queue("time: " + stopwatch.ElapsedMilliseconds.ToString() + "," + item.name + ", Velocity: " + deviceVelocity.ToString());

            //Get Device Acceleration
            item.TryGetFeatureValue(CommonUsages.deviceAcceleration, out Vector3 deviceAcceleration);
            Add_to_Queue("time: " + stopwatch.ElapsedMilliseconds.ToString() + "," + item.name + ", Acceleration: " + deviceAcceleration.ToString());

            //Get Device Trigger Button
            item.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerButton);
            Add_to_Queue("time: " + stopwatch.ElapsedMilliseconds.ToString() + "," + item.name + ", Trigger: " + triggerButton.ToString());

            //Get Device Grip Button
            item.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButton);
            Add_to_Queue("time: " + stopwatch.ElapsedMilliseconds.ToString() + "," + item.name + ", Grip: " + gripButton.ToString());
        }
    }

    public void Add_to_Queue(string input)
    {
        data_queue.Enqueue(input);
    }

    public void WriteToFile()
    {
        int count = data_queue.Count;
        for (int i = 0; i < count; i++)
        {
            if (data_queue.TryDequeue(out current_data))
            {
                print(current_data);
                writer.WriteLine(current_data);
            }
        }
        writer.Flush();
    }

    void OnApplicationQuit()
    {
        writer.Close();
    }


}