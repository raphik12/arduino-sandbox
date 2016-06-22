﻿using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using UnityEngine.UI;

public class CanonControl : MonoBehaviour {

    SerialPort stream;
    Vector3 startPosition;

    public float smooth = 2.0f;
	float convertAngles = maxDegreeValue/maxSensorValue;
	float convertMovuino = 1f;

	const float maxSensorValue = 1024f;
	const float maxDegreeValue = 360f;


	// interface
	public Slider horizontalSlider;
	public Slider verticalSlider;
	public Toggle connectionToggle;
	public Dropdown hardwareDropdown;

    // mac /dev/cu.usbmodem1421
    // pc COM
    public string port;

	private bool _isPortOpen = false;
	bool isPortOpen {
		get{
			autoSetPortOpen();
			return _isPortOpen;
		}
	}

	bool isPortOK = true;

	public bool useMovuino = false;
	public bool useArduino = false;

	void autoSetPortOpen()
	{
		_isPortOpen = (null != stream)?stream.IsOpen:false;
		if(_isPortOpen != connectionToggle.isOn)
			connectionToggle.isOn = _isPortOpen;
	}

	void openPort()
	{
		stream = new SerialPort(port, 9600);
		stream.ReadTimeout = 50;

		try
		{
			stream.Open();
			if(useMovuino)
				stream.WriteLine("l");
			isPortOK = true;
		}
		catch (Exception e) {
			isPortOK = false;
			Debug.LogError("Opening the stream failed: "+e);
		}
		autoSetPortOpen();
	}

	void closePort()
	{
		try
		{
			if(useMovuino)
				stream.WriteLine("L");
			stream.Close();
			isPortOK = true;
		}
		catch (Exception e) {
			isPortOK = false;
			Debug.LogError("Closing the stream failed: "+e);
		}
		autoSetPortOpen();
	}

	void switchHardware()
	{
		if (isPortOpen)
		{
			closePort();
		}
		useArduino = (1 == hardwareDropdown.value);
		useMovuino = (2 == hardwareDropdown.value);
	}

	void pressConnection()
	{
		if(useArduino || useMovuino)
		{
			if(connectionToggle.isOn && !isPortOpen)
			{
				openPort();
			}

			if(!connectionToggle.isOn && isPortOpen)
			{
				closePort();
			}
		}
		else
		{
			connectionToggle.isOn = false;
		}
	}

    void Awake()
    { 
		Debug.Log("Awake");
    }

	// Use this for initialization
	void Start () {
	   Debug.Log("Start");
	
		autoSetPortOpen();

       startPosition = transform.position;

		hardwareDropdown.onValueChanged.AddListener(delegate {
			switchHardware();
		});

		connectionToggle.onValueChanged.AddListener(delegate {
			pressConnection();
		});
	}

	void Destroy()
	{
		hardwareDropdown.onValueChanged.RemoveAllListeners();
		connectionToggle.onValueChanged.RemoveAllListeners();
	}
	
	// Update is called once per frame
	void Update () {
        
		// X horizontal angle
		float newXAngle = horizontalSlider.value * maxDegreeValue/2;

		// Y vertical angle
		float newYAngle = (verticalSlider.value + .5f) * maxDegreeValue;

		//if((useArduino || useMovuino) && isPortOK && isPortOpen)
		if(isPortOpen)
		{
			string arduinoString = ReadFromArduino(100);
            
			if(!string.IsNullOrEmpty(arduinoString))
			{

				string[] splitted = arduinoString.Split(' ');

				if(useArduino)
				{
					int sensor1 = Int32.Parse(splitted[0]);
					int sensor2 = Int32.Parse(splitted[1]);

					horizontalSlider.value = sensor2/maxSensorValue;
					verticalSlider.value = sensor1/maxSensorValue;

					newXAngle = -sensor1*convertAngles;
					newYAngle = -sensor2*convertAngles;
				}
				else if(useMovuino) // defensive coding
				{
					char dataType = Char.Parse(splitted[4]);
					float x = 0f,y = 0f,z = 0f;

					switch(dataType) {
					case 'p': // 1 float
						break;
					case 'a': // 3 floats
						break;
					case 'm': // 3 floats
						break;
					case 'g': // 3 floats
						x = float.Parse(splitted[4]);
						y = float.Parse(splitted[5]);
						z = float.Parse(splitted[6]);
						break;
					default:
						break;
					}

					Debug.Log("gyroscopic angles: xyz="+x+","+y+","+z);

					newXAngle = -x*convertMovuino;
					newYAngle = -y*convertMovuino;
				}
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
            return readLine;
        }
        catch (Exception e) {
            return null;
        }
        
    }


	// DEBUG METHODS

	void movuinoPing()
	{
		stream.WriteLine("?");
		//stream.BaseStream.Flush();
		string read = ReadFromArduino(100);
		Debug.LogError(read);
	}

	void movuinoDebug()
	{
		// changes behavior of blinking LED
		stream.WriteLine("r");

		// starts movuino data gathering
		stream.WriteLine("l");
		stream.BaseStream.Flush();

		string read = ReadFromArduino(100);
		string read2 = ReadFromArduino(100);
		string read3 = ReadFromArduino(100);
		string read4 = ReadFromArduino(100);

		Debug.LogError(read);
		Debug.LogError(read2);
		Debug.LogError(read3);
		Debug.LogError(read4);

		string debug = read+read2+read3+read4;
		Debug.LogError(debug);

		// stops movuino data gathering
		stream.WriteLine("L");
	}
}
