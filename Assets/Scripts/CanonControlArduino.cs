using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;

public class CanonControlArduino : MonoBehaviour {

    SerialPort stream;
    Vector3 startPosition;
    //Quaternion startRotation;
    public float smooth = 2.0F;
    float convert = 360f/1024f;
    public string port;

    void Awake()
    { 
        stream = new SerialPort(port, 9600);
        stream.ReadTimeout = 50;
        stream.Open();
    }

	// Use this for initialization
	void Start () {
	   Debug.LogError("Start");
       startPosition = transform.position;
       //startRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetKey("space"))
        //{
            string arduinoString = ReadFromArduino(100);
            
            if(string.IsNullOrEmpty(arduinoString))
                return;
            
            string[] splitted = arduinoString.Split(' ');
            
            int sensor1 = Int32.Parse(splitted[0]);
            int sensor2 = Int32.Parse(splitted[1]);
            Debug.LogError(sensor1+","+sensor2);
            
            /*
            float newX = startPosition.x+outputInt/100f;
            float newY = startPosition.y+sensorInt/500f;
            float newZ = startPosition.z;
            
            transform.position = new Vector3(newX, newY, newZ);
            */
            
            //Quaternion target = Quaternion.Euler(0f, 0f, -sensorInt*convert);
            Quaternion target = Quaternion.Euler(-sensor1*convert, -sensor2*convert, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
        //}
	}
    
    string readLine;
    public string ReadFromArduino (int timeout = 0) {
        stream.ReadTimeout = timeout;        
        try {
            readLine = stream.ReadLine();
            //stream.DiscardInBuffer();
            return readLine;
        }
        catch (Exception e) {
            //stream.DiscardInBuffer();
            return null;
        }
        
    }
}
