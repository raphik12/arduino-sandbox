using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;

public class Board : MonoBehaviour {

    SerialPort stream;
    Vector3 startPosition;
    //Quaternion startRotation;
    public float smooth = 2.0F;
    float convert = 360f/1024f;

    // mac /dev/cu.usbmodem1421
    // pc COM
    public string port;


	// Use this for initialization
	void Start () {
	   Debug.LogError("start");
       startPosition = transform.position;
       //startRotation = transform.rotation;
	}
	
    void Awake()
    { 
        stream = new SerialPort(port, 9600);
        stream.ReadTimeout = 50;
        stream.Open();
    }

	// Update is called once per frame
	void Update () {
		string arduinoString = ReadFromArduino(100);
		
		if(string.IsNullOrEmpty(arduinoString))
			return;
		
		string[] splitted = arduinoString.Split(' ');
		
		int sensor1 = Int32.Parse(splitted[0]);
		int sensor2 = Int32.Parse(splitted[1]);
		
		Quaternion target = Quaternion.Euler(-sensor1*convert, 0f, -sensor2*convert);
		transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
	}
    
    string readLine;
    public string ReadFromArduino (int timeout = 0) {
        stream.ReadTimeout = timeout;        
        try {
            readLine = stream.ReadLine();
            return readLine;
        }
        catch (Exception e) {
            return null;
        }
        
    }
}
