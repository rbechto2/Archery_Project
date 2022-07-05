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
    StreamWriter left_controller_writer;
    StreamWriter right_controller_writer;
    StreamWriter headset_writer;
    StreamWriter events_writer;
    List<InputDevice> devices = new List<InputDevice>();
    string path = Application.streamingAssetsPath + "/data_logs/";
    public static ConcurrentQueue<string> left_controller_queue = new ConcurrentQueue<string>();
    public static ConcurrentQueue<string> right_controller_queue = new ConcurrentQueue<string>();
    public static ConcurrentQueue<string> headset_queue = new ConcurrentQueue<string>();
    public static ConcurrentQueue<string> events_queue = new ConcurrentQueue<string>();
    public static Stopwatch stopwatch = new Stopwatch();


    void Start()
    {
        stopwatch.Start();
        InputDevices.GetDevices(devices);
        Directory.CreateDirectory(Application.streamingAssetsPath + "/data_logs/");
        string common_labels = "Timestamp, X_Position, Y_Position, Z_Position, Q0_Rotation, Q1_Rotation, Q2_Rotation, Q3_Rotation, X_Velocity, Y_Velocity, Z_Velocity, X_Accel, Y_Accel, Z_Accel";

        left_controller_writer = new StreamWriter(path + "left_controller.csv", true);
        left_controller_writer.WriteLine(common_labels + ", Trigger, Grip");

        right_controller_writer = new StreamWriter(path + "right_controller.csv", true);
        right_controller_writer.WriteLine(common_labels + ", Trigger, Grip");

        headset_writer = new StreamWriter(path + "headset.csv", true);
        headset_writer.WriteLine(common_labels);

        events_writer = new StreamWriter(path + "events.csv", true);
        events_writer.WriteLine("Timestamp", "Event_ID");

        logEvents(0);

    }
    
    void Update()
    { 
        if (stopwatch.ElapsedMilliseconds % 100 == 0)
        {
            WriteToFile(left_controller_queue, left_controller_writer);
            WriteToFile(right_controller_queue, right_controller_writer);
            WriteToFile(headset_queue, headset_writer);
            WriteToFile(events_queue, events_writer);
        }
        InputDevices.GetDevices(devices);
        foreach (var item in devices)
        {
            //Get Device Position
            item.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition);
            //Get Device Rotation
            item.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation);
            //Get Device Velocity
            item.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 deviceVelocity);
            //Get Device Acceleration
            item.TryGetFeatureValue(CommonUsages.deviceAcceleration, out Vector3 deviceAcceleration);
            //Get Device Trigger Button
            item.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerButton);
            //Get Device Grip Button
            item.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButton);

            string output = stopwatch.ElapsedMilliseconds.ToString() + "," + 
                devicePosition[0].ToString() + "," + devicePosition[1].ToString() + "," + devicePosition[2].ToString() + "," +
                deviceRotation[0].ToString() + "," + deviceRotation[1].ToString() + "," + deviceRotation[2].ToString() + "," + deviceRotation[3].ToString() + "," +
                deviceVelocity[0].ToString() + "," + deviceVelocity[1].ToString() + "," + deviceVelocity[2].ToString() + "," +
                deviceAcceleration[0].ToString() + "," + deviceAcceleration[1].ToString() + "," + deviceAcceleration[2].ToString();
            if (item.name.Contains("Right")) 
            {
                left_controller_queue.Enqueue(output + "," + triggerButton.ToString() + "," + gripButton.ToString());
            } 
            else if (item.name.Contains("Left"))
            {
                right_controller_queue.Enqueue(output + "," + triggerButton.ToString() + "," + gripButton.ToString());
            } 
            else
            {
                headset_queue.Enqueue(output);
            }
        }
    }

    public void logEvents(int events) 
    {
        events_queue.Enqueue(stopwatch.ElapsedMilliseconds.ToString() + ", " + events);
    }

    public void WriteToFile(ConcurrentQueue<string> myQueue, StreamWriter myStream)
    {
        int count = myQueue.Count;
        string data_to_write;
        for (int i = 0; i < count; i++)
        {
            if (myQueue.TryDequeue(out data_to_write))
            {
                myStream.WriteLine(data_to_write);
            }
        }
        myStream.Flush();
    }

    void OnApplicationQuit()
    {
        left_controller_writer.Close();
        right_controller_writer.Close();
        headset_writer.Close();
        events_writer.Close();
    }


}