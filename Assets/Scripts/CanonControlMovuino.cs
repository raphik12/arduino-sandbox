using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;

public class CanonControlMovuino : MonoBehaviour {

    SerialPort stream;
    Vector3 startPosition;
    
    public float smooth = 2.0F;
    float convert = 360f/1024f;
    public string port;

    void Awake()
    { 
		Debug.LogError("Awake");
        stream = new SerialPort(port, 9600);
        stream.ReadTimeout = 50;
        stream.Open();


		/*
		//stream.WriteLine("r");
		stream.WriteLine("l");
		stream.BaseStream.Flush();

		string read = ReadFromMovuino(100);
		string read2 = ReadFromMovuino(100);
		string read3 = ReadFromMovuino(100);
		string read4 = ReadFromMovuino(100);
		//string debug = String.IsNullOrEmpty(read)?"NULL":read;

		Debug.LogError(read);
		Debug.LogError(read2);
		Debug.LogError(read3);
		Debug.LogError(read4);

		string debug = read+read2+read3+read4;
		Debug.LogError(debug);

		stream.WriteLine("L");
		*/

    }

	// Use this for initialization
	void Start () {
	   Debug.LogError("Start");
       startPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {

		stream.WriteLine("?");
		//stream.BaseStream.Flush();
		string read = ReadFromMovuino(100);
		Debug.LogError(read);

		/*
            string arduinoString = ReadFromMovuino(100);

		if(String.IsNullOrEmpty(arduinoString))
		{
			Debug.Log("FAIL");
			//stream.WriteLine("l");
			return;
		}


		Debug.LogWarning("all ok:"+arduinoString);
            
            if(string.IsNullOrEmpty(arduinoString))
                return;
            
            string[] splitted = arduinoString.Split(' ');
            
		char dataType = Char.Parse(splitted[4]);
		float x = 0f,y = 0f,z = 0f;

		switch(dataType) {
		case 'p': // 1 float
			break;
		case 'a':case 'm': // 3 floats
			break;
		case 'g': // 3 floats
			x = float.Parse(splitted[4]);
			y = float.Parse(splitted[5]);
			z = float.Parse(splitted[6]);
			break;
		default:
			break;
		}

		Debug.LogError("xyz="+x+","+y+","+z);

		Quaternion target = Quaternion.Euler(-x*convert, -y*convert, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
        */
	}
    
    string readLine;
    public string ReadFromMovuino (int timeout = 0) {
        stream.ReadTimeout = timeout;        
        try {
			Debug.LogWarning("reading...");
            readLine = stream.ReadLine();
			Debug.LogWarning("all ok");
            return readLine;
        }
        catch (Exception e) {
			Debug.Log("exc:"+e);
            return null;
        }
        
    }
}
