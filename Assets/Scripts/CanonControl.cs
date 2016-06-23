using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using UnityEngine.UI;

public class CanonControl : MonoBehaviour {

    SerialPort stream;
    Vector3 startPosition;

    public float smooth = 2.0f;
	float convertAngles = maxDegreeValue/maxSensorValue;
	float maxMovuinoValue = 1f;

	const float maxSensorValue = 1024f;
	const float maxDegreeValue = 360f;


	// interface
	public Slider horizontalSlider;
	public Slider verticalSlider;
	public Toggle connectionToggle;
	public Dropdown hardwareDropdown;
	public Button testButton;

    // mac /dev/cu.usbmodem1421
    // pc COM
    string port;

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
			stream.DiscardInBuffer();
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

	void test()
	{
		string[] ports = SerialPort.GetPortNames();
		string testString = "found ports:";
		foreach(string detectedPort in ports)
		{
			testString += (" "+detectedPort);
		}

		Debug.Log(testString);
	}

	bool setPort()
	{
		string previousPort = port;
		string[] ports = SerialPort.GetPortNames();
		if(1 != ports.Length)
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

       startPosition = transform.position;

		hardwareDropdown.onValueChanged.AddListener(delegate {
			switchHardware();
		});

		connectionToggle.onValueChanged.AddListener(delegate {
			pressConnection();
		});

		testButton.onClick.AddListener(delegate {
			test();
		});
	}

	void Destroy()
	{
		hardwareDropdown.onValueChanged.RemoveAllListeners();
		connectionToggle.onValueChanged.RemoveAllListeners();
		testButton.onClick.RemoveAllListeners();
	}
	
	// Update is called once per frame
	void Update () {

		//if((useArduino || useMovuino) && isPortOK && isPortOpen)
		if(isPortOpen)
		{
			string arduinoString = ReadFromArduino(100);
            
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
					}
					else if(useMovuino && (7 == splitted.Length || 5 == splitted.Length)) // defensive coding
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

						horizontalSlider.value = y/maxMovuinoValue;
						verticalSlider.value = x/maxMovuinoValue;
					}
				}
				catch (Exception e)
				{
					Debug.LogError("Could not parse '"+arduinoString+"': "+e);
				}
			}
		}
        
		// X vertical angle
		float newXAngle = (1-verticalSlider.value) * maxDegreeValue/2;

		// Y horizontal angle
		float newYAngle = (horizontalSlider.value - 0.5f) * maxDegreeValue/2;

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
