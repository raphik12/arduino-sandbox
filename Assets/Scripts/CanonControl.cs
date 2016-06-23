using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System;
using UnityEngine.UI;

public class CanonControl : MonoBehaviour {

    SerialPort stream;
    Vector3 startPosition;

    public float smooth = 2.0f;
	float convertAngles = maxDegreeValue/maxSensorValue;
	public float maxMovuinoValue;

	float x = 0f;
	float y = 0f;
	float z = 0f;

	const float maxSensorValue = 1024f;
	const float maxDegreeValue = 360f;

	bool isLogExtensive = false;

	// interface
	public Slider horizontalSlider;
	public Slider verticalSlider;
	public Toggle connectionToggle;
	public Toggle logToggle;
	public Dropdown hardwareDropdown;
	public Dropdown portDropdown;
	public Button testButton;

    // mac /dev/cu.usbmodem1421
    // pc COM
    string port;
	string[] ports;

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
		setPort();

		stream = new SerialPort(port, 9600);
		stream.ReadTimeout = 100;

		try
		{
			stream.Open();
			if(useMovuino)
			{
				// TODO FIXME too early
				stream.WriteLine("l");
			}
			isPortOK = true;
			Debug.Log("port successfully open");
		}
		catch (Exception e) {
			isPortOK = false;
			Debug.LogError("Opening the stream failed: "+e);
		}
		autoSetPortOpen();
	}

	void closePort()
	{
		if(null != stream)
		{
			try
			{
				if(useMovuino)
					stream.WriteLine("L");
				stream.DiscardInBuffer();
				stream.Close();
				isPortOK = true;
				Debug.Log("port successfully closed");
			}
			catch (Exception e) {
				isPortOK = false;
				Debug.LogError("Closing the stream failed: "+e);
			}
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

	void setLog()
	{
		isLogExtensive = logToggle.isOn;
	}

	void setPortDropdown()
	{
		portDropdown.ClearOptions();

		ports = SerialPort.GetPortNames();
		if(0 == ports.Length)
		{
			Debug.LogError("no port found!");
		}
		else
		{
			string listOfPorts = "";
			foreach(string foundPort in ports)
			{
				if(!string.IsNullOrEmpty(listOfPorts))
					listOfPorts += "; ";
				listOfPorts += foundPort;
			}
			Debug.Log("found ports: "+listOfPorts);
			portDropdown.AddOptions(new List<string>(ports));
		}
	}

	void switchPort()
	{
		if (isPortOpen)
		{
			closePort();
		}

		port = ports[portDropdown.value];
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


	bool previousWasCapL = false;

	void test()
	{
		/*
		string[] ports = SerialPort.GetPortNames();
		string testString = "found ports:";
		foreach(string detectedPort in ports)
		{
			testString += (" "+detectedPort);
		}

		Debug.Log(testString);
		*/

		if(null != stream)
		{
			if(previousWasCapL)
			{
				stream.WriteLine("l");
				Debug.LogError("wrote l");
			}
			else
			{
				stream.WriteLine("L");
				Debug.LogError("wrote L");
			}
			previousWasCapL = !previousWasCapL;
		}
	}

	bool setPort()
	{
		string previousPort = port;
		string[] ports = SerialPort.GetPortNames();
		if(0 == ports.Length)
		{
			Debug.LogError("could not change port from "+previousPort+": "+ports.Length+" ports found (expected 1)");
			return false;
		}
		else
		{
			port = ports[0];
			Debug.Log("port set to "+port+", was "+previousPort);
			return true;
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
		setPortDropdown();
		setLog();

		startPosition = transform.position;

		hardwareDropdown.onValueChanged.AddListener(delegate {
			switchHardware();
		});

		portDropdown.onValueChanged.AddListener(delegate {
			switchPort();
		});

		connectionToggle.onValueChanged.AddListener(delegate {
			pressConnection();
		});

		logToggle.onValueChanged.AddListener(delegate {
			setLog();
		});

		testButton.onClick.AddListener(delegate {
			test();
		});
	}

	void Destroy()
	{
		hardwareDropdown.onValueChanged.RemoveAllListeners();
		portDropdown.onValueChanged.RemoveAllListeners();
		connectionToggle.onValueChanged.RemoveAllListeners();
		testButton.onClick.RemoveAllListeners();
	}
	
	// Update is called once per frame
	void Update () {
		
		if(isPortOpen)
		{
			string arduinoString = ReadFromArduino();
            
			if(!string.IsNullOrEmpty(arduinoString))
			{

				string[] splitted = arduinoString.Split(' ');
				try
				{
					if(useArduino && (2 == splitted.Length))
					{
						int sensor1 = Int32.Parse(splitted[0]);
						int sensor2 = Int32.Parse(splitted[1]);

						horizontalSlider.value = sensor2/maxSensorValue;
						verticalSlider.value = sensor1/maxSensorValue;

						if(isLogExtensive)
							Debug.LogWarning("arduino ok");
					}
					else if(useMovuino && (9 == splitted.Length || 7 == splitted.Length)) // defensive coding
					{
						char dataType = Char.Parse(splitted[4]);

						switch(dataType) {
						case 'p': // 1 float
							break;
						case 'a': // 3 floats
							break;
						case 'm': // 3 floats
							break;
						case 'g': // 3 floats
							x = Int32.Parse(splitted[6]);
							y = Int32.Parse(splitted[7]);
							z = Int32.Parse(splitted[8]);
							if(isLogExtensive)
								Debug.LogWarning("x = "+x);
							break;
						default:
							if(isLogExtensive)
								Debug.LogWarning("unrecognized char '"+dataType+"'");
							break;
						}

						//Debug.Log("gyroscopic angles: xyz="+x+","+y+","+z);

						horizontalSlider.value = 1-(z+maxMovuinoValue/2)/maxMovuinoValue;
						verticalSlider.value = (x+maxMovuinoValue/2)/maxMovuinoValue;;

						//horizontalSlider.value = y/maxSensorValue;
						//verticalSlider.value = x/maxSensorValue;
						if(isLogExtensive)
							Debug.LogWarning("movuino ok");
					}
					else
					{
						if(isLogExtensive)
							Debug.LogWarning("length="+splitted.Length+" ; string="+arduinoString);
					}
				}
				catch (Exception e)
				{
					Debug.LogError("Could not parse '"+arduinoString+"': "+e);
				}
			}
			else
			{
				if(isLogExtensive)
					Debug.LogWarning("arduinoString == NULL");
			}
		}
		else
		{
			if(isLogExtensive)
				Debug.LogWarning("port closed");
		}
        
		// X vertical angle
		float newXAngle = (1-verticalSlider.value) * maxDegreeValue/2;

		// Y horizontal angle
		float newYAngle = (horizontalSlider.value - 0.5f) * maxDegreeValue/2;

		Quaternion target = Quaternion.Euler(newXAngle, newYAngle, 0f);
		transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);

		if(isLogExtensive)
			Debug.LogWarning("update ok");
	}

    string readLine;
    public string ReadFromArduino () {
        try {
            readLine = stream.ReadLine();

			if(isLogExtensive)
				Debug.LogWarning("readline="+readLine);
			
			//stream.DiscardInBuffer();
            return readLine;
        }
        catch (Exception e) {
			//stream.DiscardInBuffer();
            return null;
        }
        
    }


	// DEBUG METHODS

	void movuinoPing()
	{
		stream.WriteLine("?");
		//stream.BaseStream.Flush();
		string read = ReadFromArduino();
		Debug.LogError(read);
	}

	void movuinoDebug()
	{
		// changes behavior of blinking LED
		stream.WriteLine("r");

		// starts movuino data gathering
		stream.WriteLine("l");
		stream.BaseStream.Flush();

		string read = ReadFromArduino();
		string read2 = ReadFromArduino();
		string read3 = ReadFromArduino();
		string read4 = ReadFromArduino();

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
