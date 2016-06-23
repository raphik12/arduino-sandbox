using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using UnityEngine.UI;

public class CanonControlArduino : MonoBehaviour {

    SerialPort stream;
    Vector3 startPosition;

    public float smooth = 2.0f;
	float convert = maxDegreeValue/maxSensorValue;

	const float maxSensorValue = 1024f;
	const float maxDegreeValue = 360f;
	public Slider horizontalSlider;
	public Slider verticalSlider;

    // mac /dev/cu.usbmodem1421
    // pc COM
    public string port;
	bool isPortWorking = true;

    void Awake()
    { 
		Debug.Log("Awake");

        stream = new SerialPort(port, 9600);
        stream.ReadTimeout = 50;

		try
		{
			stream.Open();
		}
		catch (Exception e) {
			isPortWorking = false;
			Debug.LogError("Opening the stream failed: "+e);
		}        
    }

	// Use this for initialization
	void Start () {
	   Debug.Log("Start");
       startPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        
		// X horizontal angle
		float newXAngle = horizontalSlider.value * maxDegreeValue/2;

		// Y vertical angle
		float newYAngle = (verticalSlider.value + .5f) * maxDegreeValue;

		if(isPortWorking)
		{
			string arduinoString = ReadFromArduino(100);
            
			if(!string.IsNullOrEmpty(arduinoString))
			{
				string[] splitted = arduinoString.Split(' ');

				int sensor1 = Int32.Parse(splitted[0]);
				int sensor2 = Int32.Parse(splitted[1]);

				horizontalSlider.value = sensor2/maxSensorValue;
				verticalSlider.value = sensor1/maxSensorValue;

				newXAngle = -sensor1*convert;
				newYAngle = -sensor2*convert;
			}
		}

		Quaternion target = Quaternion.Euler(newXAngle, newYAngle, 0f);
		transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
	}
    
    string readLine;
    public string ReadFromArduino (int timeout = 0) {
        stream.ReadTimeout = timeout;        
        try {
            readLine = stream.ReadLine();
			stream.DiscardInBuffer();
            return readLine;
        }
        catch (Exception e) {
			stream.DiscardInBuffer();
            return null;
        }
        
    }
}
