// Author: Slavomir Svorada
// Date: 17.2.2024

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Threading;
using UnityEngine.Windows.Speech;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class HelmetMountedDisplay : MonoBehaviour
{
    // set  variables for connection
    public int port = 49007;
    UdpClient udpClient;
    Thread receiveThread;

    // define constants
    public const int MAX_HEADING = 360;
    public const int MIDDLE_SPEED = 10;
    public const int MAX_SPEED = 100;
    public const int MAX_ALTITUDE = 500;
    public const int MAX_ENGINE_RPM = 4500;
    public const int MAX_ROTATION_SPEED = 90;
    public const int VERTICAL_SPEED = 5;
    public const int  HORIZONTAL_SPEED = 5;

    private KeywordRecognizer keywordRecognizer;
    private string[] keywords = { "hey helmet", "stop helmet", "weather conditions", "gear info", "home", "map" };
    private bool aircraftRecognized = false;
    private bool inMapScreen = false;
    private bool inGearScreen = false;

    // wetaher information
    public string apiKey = "MZQND65XWUU2RJK74PENNLGQU";
    public string location = "Bratislava";
    //public Text weatherText;

    // variables for canvas
    public GameObject canvasPohyb;
    public GameObject canvasFix;
    public GameObject canvasFixVertical;
    public GameObject canvasPohybVertical;
    public GameObject LandingCamera;
    public GameObject LandingCameraPohyb;
    public GameObject LandingCameraVertical;
    public GameObject LandingCameraVerticalPohyb;
    public GameObject Letun;
    public GameObject ScreenWeather;
    public GameObject ScreenMap;
    public GameObject ScreenGear;
    public GameObject ScreenMenu;
    public GameObject AircraftGear;
    public GameObject VertiportPointer;
    public GameObject VertiportPointer2;
    private bool isShowing;
    public bool switchOn;
    public RawImage stupnicaKlopeniaVertical, stupnicaKlopeniaFixVertical, stupnicaKlopeniaFix, stupnicaKlopenia, UkazovatelKurzuFix, UkazovatelKurzu, UkazovatelKurzuFixVertical, RychlomerStupicaVlavoFix, RychlomerStupicaStredFix, RychlomerStupicaVpravoFix,
    RychlomerStupicaVlavo, RychlomerStupicaStred, RychlomerStupicaVpravo, VyskomerStupnicaVpravoFix, VyskomerStupnicaStredFix, VyskomerStupnicaVpravo, VyskomerStupnicaStred, IndikatorKloneniaStredFix, IndikatorKloneniaStred, IndikatorKloneniaStredFixVertical,
    RychlomerStupicaVpravoFixVertical, RychlomerStupicaStredFixVertical, RychlomerStupicaVlavoFixVertical, VyskomerStupnicaVpravoFixVertical, VyskomerStupnicaStredFixVertical, IndikatorKloneniaStredVertical, UkazovatelKurzuVertical,
    RychlomerStupicaVpravoVertical, RychlomerStupicaStredVertical, RychlomerStupicaVlavoVertical, VyskomerStupnicaVpravoVertical, VyskomerStupnicaStredVertical, evtolVertical, evtol;
    public Text HeadingCisloFix, HeadingCislo, HeadingCisloFixVertical, QnhFix, VyskomerCisloFix, VyskaLowTextFix, VyskomerCislo, VyskaLowText, Qnh, QnhFixVertical, VyskomerCisloFixVertical,
    VyskaLowTextFixVertical, HeadingCisloVertical, QnhVertical, VyskomerCisloVertical, VyskaLowTextVertical, Temperature, WindSpeed, Pressure, CloudCover, GearInfo, Visibility, IndicatedSpeed, MapHeading, LocationHeader, TemperatureMax, TemperatureMin,
    BackSpeed, BackSpeedVertical, BackSpeedFix, BackSpeedVerticalFix;
    public Image VystrahaVlavoFix, VystrahaVpravoFix, SipkaVlavoFix, SipkaVpravoFix, VyskaLowFix, VystrahaVlavo, VystrahaVpravo, SipkaVlavo, SipkaVpravo, VyskaLow,
    VystrahaVlavoFixVertical, VystrahaVpravoFixVertical, SipkaVlavoFixVertical, SipkaVpravoFixVertical, VyskaLowFixVertical, VystrahaVlavoVertical, VystrahaVpravoVertical, SipkaVlavoVertical, SipkaVpravoVertical, VyskaLowVertical,
    MotorLavyPredny, MotorLavyPrednyFix, MotorLavyZadny, MotorLavyZadnyFix, MotorPravyPredny, MotorPravyPrednyFix, MotorPravyZadny, MotorPravyZadnyFix, eVTOLLandingLine, eVTOLLandingLineFix,
    RedArrow, RedArrowVertical, RedArrowFix, RedArrowVerticalFix;
    public AudioSource warningSound;
    public float startingAngle = 0.0f, startingRollAngle = 0.0f, startingRollAnglePohyb = 0.0f, startingRollAngleVertical = 0.0f, startingRollAngleVerticalPohyb = 0.0f;
    public float pitch, roll, airspeed, altitude, heading, qnh, aglAltitude, engine1, engine2, engine3, engine4, gear;

    public bool standartHMD, verticalHMD;
    //public Transform headTransform;

    public RenderTexture renderTexture;
    public Camera landingCamera;
    public Camera LandingCameraMap;
    public float rotationSpeed = 5.0f;
    // END TEST

    // variables for VR angles
    public float x1, y1, z1;
    bool running = true;

    public string subor = "ActualData.txt";
    public float priemer1 = 0;
    public float priemer2 = 0;
    public int pocet = 0;

    public float startTime = 0f;
    public float endTime = 0f;
    public bool landingTimeCheck = false;
    public bool landingEndTimeCheck = false;

    void Start()
    {
        // Initialize UDP client
        udpClient = new UdpClient(port);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.Start();

        // hide all warning symbols
        VystrahaVlavoFix.gameObject.SetActive(false);
        VystrahaVpravoFix.gameObject.SetActive(false);
        SipkaVlavoFix.gameObject.SetActive(false);
        SipkaVpravoFix.gameObject.SetActive(false);
        VystrahaVlavo.gameObject.SetActive(false);
        VystrahaVpravo.gameObject.SetActive(false);
        SipkaVlavo.gameObject.SetActive(false);
        SipkaVpravo.gameObject.SetActive(false);
        VystrahaVlavoFixVertical.gameObject.SetActive(false);
        VystrahaVpravoFixVertical.gameObject.SetActive(false);
        SipkaVlavoFixVertical.gameObject.SetActive(false);
        SipkaVpravoFixVertical.gameObject.SetActive(false);
        VystrahaVlavoVertical.gameObject.SetActive(false);
        VystrahaVpravoVertical.gameObject.SetActive(false);
        SipkaVlavoVertical.gameObject.SetActive(false);
        SipkaVpravoVertical.gameObject.SetActive(false);

        // stop sounds
        warningSound.Stop();

        // hide altitude low middle part
        VyskaLowFix.gameObject.SetActive(false);
        VyskaLow.gameObject.SetActive(false);
        VyskaLowFixVertical.gameObject.SetActive(false);
        VyskaLowVertical.gameObject.SetActive(false);

        // hide landing camera at the start
        LandingCamera.gameObject.SetActive(false);
        LandingCameraPohyb.gameObject.SetActive(false);
        LandingCameraVertical.gameObject.SetActive(false);
        LandingCameraVerticalPohyb.gameObject.SetActive(false);

        // hide every canvas at the start
        canvasPohyb.SetActive(false);
        canvasPohybVertical.SetActive(false);
        canvasFix.SetActive(false);
        canvasFixVertical.SetActive(false);

        // variables for showing HMD
        standartHMD = false;
        verticalHMD = false;

        // set power images to 0
        MotorLavyPredny.fillAmount = 0;
        MotorLavyPrednyFix.fillAmount = 0;
        MotorLavyZadny.fillAmount = 0;
        MotorLavyZadnyFix.fillAmount = 0;
        MotorPravyPredny.fillAmount = 0;
        MotorPravyPrednyFix.fillAmount = 0;
        MotorPravyZadny.fillAmount = 0;
        MotorPravyZadnyFix.fillAmount = 0;

        // set landingLine to 0
        eVTOLLandingLineFix.fillAmount = 0;
        eVTOLLandingLine.fillAmount = 0;

        // Initialization of keyword recognizer with wanted words
        keywordRecognizer = new KeywordRecognizer(keywords);
        // do action after word recognition
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        // start recognition
        keywordRecognizer.Start();

        // hide screens
        ScreenWeather.gameObject.SetActive(false);
        ScreenGear.gameObject.SetActive(false);
        ScreenMenu.gameObject.SetActive(false);
        ScreenMap.gameObject.SetActive(false);

        // hide icon and back speed
        BackSpeed.gameObject.SetActive(false);
        BackSpeedFix.gameObject.SetActive(false);
        BackSpeedVertical.gameObject.SetActive(false);
        BackSpeedVerticalFix.gameObject.SetActive(false);
        RedArrow.gameObject.SetActive(false);
        RedArrowFix.gameObject.SetActive(false);
        RedArrowVertical.gameObject.SetActive(false);
        RedArrowVerticalFix.gameObject.SetActive(false);
    }

    void Update()
    {
        // function for getting rotation of VR
        getRotation();
        // set right canvas
        //setCanvas();
        // function for showing Fix canvas
        //runHMDFix();
        // function for landing camera
        LateUpdate();
        // movement of aircraft
        makeMovement();
        // set aircraft gear
        if (inGearScreen)
        {
            setGear();
        }
        // set map info
        if (inMapScreen)
        {
            setMap();
        }
        
        
        // set type of HMD
        if (Input.GetKey(KeyCode.UpArrow))
        {
            standartHMD = true;
            verticalHMD = false;
        } else if (Input.GetKey(KeyCode.DownArrow))
        {
            verticalHMD = true;
            standartHMD = false;
        }
        if (standartHMD)
        {
            canvasPohybVertical.SetActive(false);
            canvasFixVertical.SetActive(false);
            if ((heading > 345) || (heading < 15))
            {
                if (((x1 < 330) && (x1 > 30)) || ((y1 < 330) && (y1 > 30)))
                {
                    canvasFix.SetActive(false);
                    canvasPohyb.SetActive(true);
                    runHMD();
                } else
                {
                    canvasFix.SetActive(true);
                    canvasPohyb.SetActive(false);
                    runHMDFix();
                }
            } else
            {
                if (((x1 <= (330-pitch)) && (x1 >= (30-pitch))) || ((y1) <= (heading-30) || (y1) >= (heading+30)))
                {
                    canvasFix.SetActive(false);
                    canvasPohyb.SetActive(true);
                    runHMD();
                } else
                {
                    canvasFix.SetActive(true);
                    canvasPohyb.SetActive(false);
                    runHMDFix();
                }
            }
            
        } else if (verticalHMD)
        {
            canvasPohyb.SetActive(false);
            canvasFix.SetActive(false);
            if ((heading > 345) || (heading < 15))
            {
                if (((x1 < 330) && (x1 > 30)) || ((y1 < 330) && (y1 > 30)))
                {
                    canvasFixVertical.SetActive(false);
                    canvasPohybVertical.SetActive(true);
                    runHMDVertical();
                } else
                {
                    canvasFixVertical.SetActive(true);
                    canvasPohybVertical.SetActive(false);
                    runHMDVerticalFix();
                }
            } else
            {
                if (((x1 <= (330-pitch)) && (x1 >= (30-pitch))) || ((y1) <= (heading-30) || (y1) >= (heading+30)))
                {
                    canvasFixVertical.SetActive(false);
                    canvasPohybVertical.SetActive(true);
                    runHMDVertical();

                } else
                {
                    canvasFixVertical.SetActive(true);
                    canvasPohybVertical.SetActive(false);
                    runHMDVerticalFix();
                }
            }
        }

        float landingDistanceForTime = Vector3.Distance (VertiportPointer.transform.position, VertiportPointer2.transform.position);
        //Debug.Log("Actual_Distance: " + landingDistanceForTime + "\n");
        
        if (landingDistanceForTime < 300 && landingTimeCheck == false)
        {
            startTime = Time.time;
            Debug.Log("StartTime: " + startTime + "\n");
            landingTimeCheck = true;
        }
        if (((int)aglAltitude == 0) && (startTime != 0) && landingEndTimeCheck == false)
        {
            endTime = Time.time - startTime;
            Debug.Log("LandingTime: " + (float)Math.Round(endTime, 1) + "\n");
            landingEndTimeCheck = true;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveData();
        } else if (Input.GetKeyDown(KeyCode.W))
        {
            WriteResult();
        }
        
    }
    void SaveData()
    {
        // Get distance between aircraft and landing position for landing data result
        float landingDistance = Vector3.Distance (VertiportPointer.transform.position, VertiportPointer2.transform.position);
        Debug.Log("Distance: " + (landingDistance/3) + "\n");
        float landingDirection = Math.Abs(180-heading);
        Debug.Log("Direction: " + landingDirection + "\n");
        
        priemer1 = ((landingDistance/3) + priemer1 * pocet) / (pocet + 1);
        priemer2 = (landingDirection + priemer2 * pocet) / (pocet + 1);

        pocet++;

        // Save values to file
        using (StreamWriter sw = File.AppendText(subor))
        {
            sw.WriteLine((landingDistance/3) + "," + landingDirection); 
            sw.WriteLine(priemer1 + "," + priemer2);
        }
    }
     void WriteResult()
    {
        // Open file for reading
        using (StreamReader sr = new StreamReader(subor))
        {
            // Inicializácia premenných na výpočet celkového priemeru
            float celkovyPriemer1 = 0;
            float celkovyPriemer2 = 0;
            int celkovyPocet = 0;

            // Read from file (line by line)
            string riadok;
            while ((riadok = sr.ReadLine()) != null)
            {
                // Split values with ","
                string[] hodnoty = riadok.Split(',');

                // String to float
                float hodnota1 = float.Parse(hodnoty[0]);
                float hodnota2 = float.Parse(hodnoty[1]);

                celkovyPriemer1 += hodnota1;
                celkovyPriemer2 += hodnota2;
                celkovyPocet++;
            }

            // Calculation of the overall average
            celkovyPriemer1 /= celkovyPocet;
            celkovyPriemer2 /= celkovyPocet;

            // Write final result to console
            Debug.Log("Distance: " + celkovyPriemer1 + ", Direction: " + celkovyPriemer2 + "\n");
        }
    }

    void LateUpdate()
    {
        // Copy camera content to Render Texture
        Graphics.Blit(landingCamera.targetTexture, renderTexture);
    }

    void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        while (running)
        {
            try
            {
                byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
                string receivedData = Encoding.ASCII.GetString(receivedBytes);
                // Pass the received data to the main thread for processing using a thread-safe queue
                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessData), receivedData);
            }
            catch (SocketException e)
            {
                Debug.LogError("Error during data receiving: " + e.Message);
            }
        }
    }

    void ProcessData(object data)
    {
        string receivedData = (string)data;
        // Splitting data
        string[] dataParts = receivedData.Split('|');
        pitch = float.Parse(dataParts[0]);
        roll = float.Parse(dataParts[1]);
        airspeed = float.Parse(dataParts[2]);
        altitude = float.Parse(dataParts[3]);
        heading = float.Parse(dataParts[4]);
        qnh = float.Parse(dataParts[5]);
        aglAltitude = float.Parse(dataParts[6]);
        engine1 = float.Parse(dataParts[7]);
        engine2 = float.Parse(dataParts[8]);
        engine3 = float.Parse(dataParts[9]);
        engine4 = float.Parse(dataParts[10]);
        gear = float.Parse(dataParts[11]);
        // Print data
        //Debug.Log("Received Pitch: " + pitch + " | Roll: " + roll + " | Airspeed: " + airspeed + " | Altitude: " + altitude + " | Heading: " + heading + " | QNH: " + qnh + " | aglAltitude: " + aglAltitude + " | Engine1: " + engine1 + " | Engine2: " + engine2 + " | Engine3: " + engine3 + " | Engine4: " + engine4 + "\n");
    }

    void setCanvas()
    {
        if ((x1 < 330 && x1 > 30) || (y1 < 330 && y1 > 30))
        {
            canvasFix.SetActive(false);
            canvasPohyb.SetActive(true);
        }
        else
        {
            canvasFix.SetActive(true);
            canvasPohyb.SetActive(false);
        }
    }

    void setGear()
    {
        IndicatedSpeed.text = ((int)airspeed).ToString();
        // gear is down
        if ((int)gear == 1)
        {
            AircraftGear.gameObject.SetActive(true);
            GearInfo.text = "DOWN";
        } else // gear is up
        {
            AircraftGear.gameObject.SetActive(false);
            GearInfo.text = "UP";
        }
    }

    void setMap()
    {
        MapHeading.text = ((int)heading).ToString();
    }

    void getRotation()
    {
        // set values for VR rotation
        Quaternion camData = Camera.main.transform.rotation;
        Vector3 eRotation = Camera.main.transform.eulerAngles;

        x1 = eRotation.x;
        y1 = eRotation.y;
        z1 = eRotation.z;

        //Debug.Log("x1: " + x1 + " y1: " + y1 + " z1: " + z1 + "\n");
    }

    void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        // If word is recognized
        if (speech.text == "hey helmet")
        {
            aircraftRecognized = true;
            Debug.Log("Helmet voice system ACTIVE. Available commands: 'weather conditions', 'gear info', 'home', 'map', 'stop helmet'\n");
            // hide special screens
            ScreenWeather.gameObject.SetActive(false);
            ScreenGear.gameObject.SetActive(false);
            ScreenMap.gameObject.SetActive(false);
            ScreenMenu.gameObject.SetActive(true);
        }
        else if (aircraftRecognized)
        {
            switch (speech.text)
            {
                case "weather conditions":
                    Debug.Log("Weather command recognized. Here is BA weather");
                    GetWeatherData();
                    ScreenWeather.gameObject.SetActive(true);
                    ScreenGear.gameObject.SetActive(false);
                    ScreenMenu.gameObject.SetActive(false);
                    ScreenMap.gameObject.SetActive(false);
                    // disable function for zooming in map
                    inMapScreen = false;
                    inGearScreen = false;
                    break;
                case "gear info":
                    inGearScreen = true;
                    Debug.Log("Gear command recognized. Gear value is: " + (int)gear);
                    ScreenWeather.gameObject.SetActive(false);
                    ScreenGear.gameObject.SetActive(true);
                    ScreenMenu.gameObject.SetActive(false);
                    ScreenMap.gameObject.SetActive(false);
                    inMapScreen = false;
                    break;
                case "map":
                    inMapScreen = true;
                    inGearScreen = false;
                    ScreenWeather.gameObject.SetActive(false);
                    ScreenGear.gameObject.SetActive(false);
                    ScreenMenu.gameObject.SetActive(false);
                    ScreenMap.gameObject.SetActive(true);
                    break;
                case "home":
                    ScreenWeather.gameObject.SetActive(false);
                    ScreenGear.gameObject.SetActive(false);
                    ScreenMenu.gameObject.SetActive(true);
                    ScreenMap.gameObject.SetActive(false);
                    inMapScreen = false;
                    inGearScreen = false;
                    break;
                case "stop helmet":
                    inMapScreen = false;
                    inGearScreen = false;
                    aircraftRecognized = false;
                    Debug.Log("Helmet voice system INACTIVE. \n");
                    ScreenWeather.gameObject.SetActive(false);
                    ScreenGear.gameObject.SetActive(false);
                    ScreenMenu.gameObject.SetActive(false);
                    ScreenMap.gameObject.SetActive(false);
                    break;
                default:
                    Debug.Log("Wrong command: " + speech.text + "\n");
                    break;
            }
        }
    }

    async void GetWeatherData()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string url = $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{location}?key={apiKey}";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                LocationHeader.text = location;

                // write all data
                Debug.Log(responseBody);

                // Parsing JSON response
                JObject json = JObject.Parse(responseBody);

                // Getting weather data
                double WeatherTemperatureF = (double)json["days"][0]["temp"];
                double WeatherTemperature = (WeatherTemperatureF - 32) * 5 / 9;
                double WeatherTemperatureMaxF = (double)json["days"][0]["tempmax"];
                double WeatherTemperatureMinF = (double)json["days"][0]["tempmin"];
                double WeatherTemperatureMax = (WeatherTemperatureMaxF - 32) * 5 / 9;
                double WeatherTemperatureMin = (WeatherTemperatureMinF - 32) * 5 / 9;
                double WeatherWindSpeed = (double)json["days"][0]["windspeed"];
                double WeatherPressure = (double)json["days"][0]["pressure"];
                double WeatherCloudCover = (double)json["days"][0]["cloudcover"];
                double WeatherVisibility = (double)json["days"][0]["visibility"];
                
                //Debug.Log("Temp: " + (int)WeatherTemperature + " WindSpeed: " + WeatherWindSpeed + " Pressure:" + (int)WeatherPressure + " CloudCover:" + (int)WeatherCloudCover + " Visibility:" + WeatherVisibility + "\n");
                Temperature.text = ((int)WeatherTemperature).ToString();
                TemperatureMax.text = ((int)WeatherTemperatureMax).ToString();
                TemperatureMin.text = ((int)WeatherTemperatureMin).ToString();
                WindSpeed.text = ((int)WeatherWindSpeed).ToString();
                Pressure.text = ((int)WeatherPressure).ToString();
                CloudCover.text = ((int)WeatherCloudCover).ToString();
                Visibility.text = WeatherVisibility.ToString();
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"Error: {e.Message}");
            }
        }
    }

    void makeMovement()
    {
        // Movement forward
        Letun.transform.position += Letun.transform.forward * airspeed / (float)(2.1) * Time.deltaTime;

        // Movement to side
        float horizontalMovement = Mathf.Sin(roll * Mathf.Deg2Rad) * HORIZONTAL_SPEED * Time.deltaTime;
        Vector3 sidewaysMovement = Letun.transform.right * horizontalMovement;
        Letun.transform.position += sidewaysMovement;

        // Movement up and down
        float newAltitude = Mathf.Lerp(Letun.transform.position.y, aglAltitude, Time.deltaTime * VERTICAL_SPEED);
        Letun.transform.position = new Vector3(Letun.transform.position.x, newAltitude, Letun.transform.position.z);
        
        Quaternion targetRotation = Quaternion.Euler(-pitch, heading, -roll);
        Letun.transform.rotation = Quaternion.Lerp(Letun.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        //Letun.transform.rotation = targetRotation;
        //Letun.transform.rotation = Quaternion.Slerp(Letun.transform.rotation, targetRotation, Time.deltaTime * MAX_ROTATION_SPEED);
        //Letun.transform.rotation = Quaternion.Lerp(Letun.transform.rotation, targetRotation, Time.deltaTime * MAX_ROTATION_SPEED);

        // Rotácia s použitím Quaternion.RotateTowards pre plynulé zatáčanie
        //Letun.transform.rotation = Quaternion.RotateTowards(Letun.transform.rotation, targetRotation, MAX_ROTATION_SPEED * Time.deltaTime);
    }

    void runHMDFix()
    {
        // Check roll angle
        if (startingRollAngle != roll)
        {
            stupnicaKlopeniaFix.transform.Rotate(new Vector3(0, 0, (roll - startingRollAngle)));
            // Set aicraft rotation
            IndikatorKloneniaStredFix.transform.Rotate(new Vector3(0, 0, (roll - startingRollAngle)));
        }

        // Set warning symbols if angle is too steep and play warning sound
        if (startingRollAngle > 60)
        {
            // Play warning effect
            switchOn = true;
            if (switchOn == true && warningSound.isPlaying == false)
            {
                warningSound.Play();   
            } else if (switchOn == false && warningSound.isPlaying == true)
            {
                warningSound.Stop();
            }
            VystrahaVlavoFix.gameObject.SetActive(false);
            VystrahaVpravoFix.gameObject.SetActive(true);
            SipkaVlavoFix.gameObject.SetActive(true);
            SipkaVpravoFix.gameObject.SetActive(false);
        } else if (startingRollAngle < -60)
        {
            // Play warning effect
            switchOn = true;
            if (switchOn == true && warningSound.isPlaying == false)
            {
                warningSound.Play();   
            } else if (switchOn == false && warningSound.isPlaying == true)
            {
                warningSound.Stop();
            }
            VystrahaVlavoFix.gameObject.SetActive(true);
            VystrahaVpravoFix.gameObject.SetActive(false);
            SipkaVlavoFix.gameObject.SetActive(false);
            SipkaVpravoFix.gameObject.SetActive(true);
        } else
        {
            // Stop playing warning sound when angle is OK
            switchOn = false;
            warningSound.Stop();
            VystrahaVlavoFix.gameObject.SetActive(false);
            VystrahaVpravoFix.gameObject.SetActive(false);
            SipkaVlavoFix.gameObject.SetActive(false);
            SipkaVpravoFix.gameObject.SetActive(false);
        }

        // Set new angle
        startingRollAngle = roll;

        // Check pitch angle
        if (pitch > 60)
        {
            pitch = 60;
        }
        if (pitch < -60)
        {
            pitch = -60;
        }
        // Move with middle part up and down
        stupnicaKlopeniaFix.uvRect = new Rect(0, (pitch * 0.0082f) + 0.315f, 1, 0.37f);

        // Heading course & heading number
        UkazovatelKurzuFix.uvRect = new Rect(0.45f + (heading / MAX_HEADING), 0, 0.1f, 0.99f);
        HeadingCisloFix.text = ((int)heading).ToString();

        // Set speed of aircraft
        int speedInt = (int)airspeed;
        if (airspeed > 0)
        {
            RychlomerStupicaVpravoFix.uvRect = new Rect(0, (-0.018f + (airspeed/MIDDLE_SPEED)), 1, 0.13f);
            RychlomerStupicaStredFix.uvRect = new Rect(0, (-0.018f + (((float)((int)(speedInt/MIDDLE_SPEED))/MIDDLE_SPEED))), 1, 0.13f);
            RychlomerStupicaVlavoFix.uvRect = new Rect(0, (-0.018f + (((float)((int)(speedInt/MAX_SPEED))/MIDDLE_SPEED))), 1, 0.13f);
            BackSpeedFix.gameObject.SetActive(false);
            RedArrowFix.gameObject.SetActive(false);
        } else
        {
            RychlomerStupicaVpravoFix.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            RychlomerStupicaStredFix.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            RychlomerStupicaVlavoFix.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            BackSpeedFix.gameObject.SetActive(false);
            RedArrowFix.gameObject.SetActive(false);
            if (airspeed < 0)
            {
                BackSpeedFix.text = ((int)(Math.Abs(airspeed))).ToString();
                BackSpeedFix.gameObject.SetActive(true);
                RedArrowFix.gameObject.SetActive(true);
            }
        }

        // Set QNH number
        QnhFix.text = Math.Round(qnh, 2).ToString();

        // Set altitude of aircraft
        VyskomerStupnicaVpravoFix.uvRect = new Rect(0, -0.577f + (altitude / 100), 1, 0.15f);
        VyskomerStupnicaStredFix.uvRect = new Rect(0, -0.018f + (altitude / 1000), 1, 0.13f);
        VyskomerCisloFix.text = ((int)(altitude / 1000)).ToString();

        // Showing altitude in the middle (under 500)
        if (aglAltitude < MAX_ALTITUDE)
        {
            VyskaLowTextFix.text = ((int)aglAltitude).ToString();
            VyskaLowFix.gameObject.SetActive(true);
            LandingCamera.gameObject.SetActive(true);
        } else
        {
            VyskaLowFix.gameObject.SetActive(false);
            LandingCamera.gameObject.SetActive(false);
        }
    }

    void runHMD()
    {
        // Check roll angle
        if (startingRollAnglePohyb != roll)
        {
            stupnicaKlopenia.transform.Rotate(new Vector3(0, 0, (roll - startingRollAnglePohyb)));
            // Set aicraft rotation
            IndikatorKloneniaStred.transform.Rotate(new Vector3(0, 0, (roll - startingRollAnglePohyb)));
        }

        // Set warning symbols if angle is too steep and play warning sound
        if (startingRollAnglePohyb > 60)
        {
            // Play warning effect
            switchOn = true;
            if (switchOn == true && warningSound.isPlaying == false)
            {
                warningSound.Play();   
            } else if (switchOn == false && warningSound.isPlaying == true)
            {
                warningSound.Stop();
            }
            VystrahaVlavo.gameObject.SetActive(false);
            VystrahaVpravo.gameObject.SetActive(true);
            SipkaVlavo.gameObject.SetActive(true);
            SipkaVpravo.gameObject.SetActive(false);
        } else if (startingRollAnglePohyb < -60)
        {
            // Play warning effect
            switchOn = true;
            if (switchOn == true && warningSound.isPlaying == false)
            {
                warningSound.Play();   
            } else if (switchOn == false && warningSound.isPlaying == true)
            {
                warningSound.Stop();
            }
            VystrahaVlavo.gameObject.SetActive(true);
            VystrahaVpravo.gameObject.SetActive(false);
            SipkaVlavo.gameObject.SetActive(false);
            SipkaVpravo.gameObject.SetActive(true);
        } else
        {
            // Stop playing warning sound when angle is OK
            switchOn = false;
            warningSound.Stop();
            VystrahaVlavo.gameObject.SetActive(false);
            VystrahaVpravo.gameObject.SetActive(false);
            SipkaVlavo.gameObject.SetActive(false);
            SipkaVpravo.gameObject.SetActive(false);
        }

        // Set new angle
        startingRollAnglePohyb = roll;

        // Check pitch angle
        if (pitch > 60)
        {
            pitch = 60;
        }
        if (pitch < -60)
        {
            pitch = -60;
        }
        // Move with middle part up and down
        stupnicaKlopenia.uvRect = new Rect(0, (pitch * 0.0082f) + 0.315f, 1, 0.37f);

        // Heading course & heading number
        UkazovatelKurzu.uvRect = new Rect(0.45f + (heading / MAX_HEADING), 0, 0.1f, 0.99f);
        HeadingCislo.text = ((int)heading).ToString();

        // Set speed of aircraft
        int speedInt = (int)airspeed;
        if (airspeed > 0)
        {
            RychlomerStupicaVpravo.uvRect = new Rect(0, (-0.018f + (airspeed/MIDDLE_SPEED)), 1, 0.13f);
            RychlomerStupicaStred.uvRect = new Rect(0, (-0.018f + (((float)((int)(speedInt/MIDDLE_SPEED))/MIDDLE_SPEED))), 1, 0.13f);
            RychlomerStupicaVlavo.uvRect = new Rect(0, (-0.018f + (((float)((int)(speedInt/MAX_SPEED))/MIDDLE_SPEED))), 1, 0.13f);
            BackSpeed.gameObject.SetActive(false);
            RedArrow.gameObject.SetActive(false);
        } else
        {
            RychlomerStupicaVpravo.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            RychlomerStupicaStred.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            RychlomerStupicaVlavo.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            BackSpeed.gameObject.SetActive(false);
            RedArrow.gameObject.SetActive(false);
            if (airspeed < 0)
            {
                BackSpeed.text = ((int)(Math.Abs(airspeed))).ToString();
                BackSpeed.gameObject.SetActive(true);
                RedArrow.gameObject.SetActive(true);
            }

        }

        // Set QNH number
        Qnh.text = Math.Round(qnh, 2).ToString();

        // Set altitude of aircraft
        VyskomerStupnicaVpravo.uvRect = new Rect(0, -0.577f + (altitude / 100), 1, 0.15f);
        VyskomerStupnicaStred.uvRect = new Rect(0, -0.018f + (altitude / 1000), 1, 0.13f);
        VyskomerCislo.text = ((int)(altitude / 1000)).ToString();

        // Showing altitude in the middle (under 500)
        if (aglAltitude < MAX_ALTITUDE)
        {
            VyskaLowText.text = ((int)aglAltitude).ToString();
            VyskaLow.gameObject.SetActive(true);
            LandingCameraPohyb.gameObject.SetActive(true);
        } else
        {
            VyskaLow.gameObject.SetActive(false);
            LandingCameraPohyb.gameObject.SetActive(false);

        }
    }

    void runHMDVerticalFix()
    {
        // Check roll angle
        if (startingRollAngleVertical != roll)
        {
            stupnicaKlopeniaFixVertical.transform.Rotate(new Vector3(0, 0, (roll - startingRollAngleVertical)));
            // Set aicraft rotation
            IndikatorKloneniaStredFixVertical.transform.Rotate(new Vector3(0, 0, (roll - startingRollAngleVertical)));
        }
        if (startingRollAngleVertical > 60)
        {
            // Play warning effect
            switchOn = true;
            if (switchOn == true && warningSound.isPlaying == false)
            {
                warningSound.Play();   
            } else if (switchOn == false && warningSound.isPlaying == true)
            {
                warningSound.Stop();
            }
            VystrahaVlavoFixVertical.gameObject.SetActive(false);
            VystrahaVpravoFixVertical.gameObject.SetActive(true);
            SipkaVlavoFixVertical.gameObject.SetActive(true);
            SipkaVpravoFixVertical.gameObject.SetActive(false);
        } else if (startingRollAngleVertical < -60)
        {
            // Play warning effect
            switchOn = true;
            if (switchOn == true && warningSound.isPlaying == false)
            {
                warningSound.Play();   
            } else if (switchOn == false && warningSound.isPlaying == true)
            {
                warningSound.Stop();
            }
            VystrahaVlavoFixVertical.gameObject.SetActive(true);
            VystrahaVpravoFixVertical.gameObject.SetActive(false);
            SipkaVlavoFixVertical.gameObject.SetActive(false);
            SipkaVpravoFixVertical.gameObject.SetActive(true);
        } else
        {
            // Stop playing warning sound when angle is OK
            switchOn = false;
            warningSound.Stop();
            VystrahaVlavoFixVertical.gameObject.SetActive(false);
            VystrahaVpravoFixVertical.gameObject.SetActive(false);
            SipkaVlavoFixVertical.gameObject.SetActive(false);
            SipkaVpravoFixVertical.gameObject.SetActive(false);
        }
        
        // Set new angle
        startingRollAngleVertical = roll;
        // Check pitch angle
        if (pitch > 60)
        {
            pitch = 60;
        }
        if (pitch < -60)
        {
            pitch = -60;
        }
        // Move with middle part up and down
        stupnicaKlopeniaFixVertical.uvRect = new Rect(0, (pitch * 0.0082f) + 0.315f, 1, 0.37f);

        
        // Heading course & heading number
        UkazovatelKurzuFixVertical.uvRect = new Rect(0.45f + (heading / MAX_HEADING), 0, 0.1f, 0.99f);
        HeadingCisloFixVertical.text = ((int)heading).ToString();

        // Set speed of aircraft
        int speedInt = (int)airspeed;
        if (airspeed > 0)
        {
            RychlomerStupicaVpravoFixVertical.uvRect = new Rect(0, (-0.018f + (airspeed/MIDDLE_SPEED)), 1, 0.13f);
            RychlomerStupicaStredFixVertical.uvRect = new Rect(0, (-0.018f + (((float)((int)(speedInt/MIDDLE_SPEED))/MIDDLE_SPEED))), 1, 0.13f);
            RychlomerStupicaVlavoFixVertical.uvRect = new Rect(0, (-0.018f + (((float)((int)(speedInt/MAX_SPEED))/MIDDLE_SPEED))), 1, 0.13f);
            BackSpeedVerticalFix.gameObject.SetActive(false);
            RedArrowVerticalFix.gameObject.SetActive(false);
        } else
        {
            RychlomerStupicaVpravoFixVertical.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            RychlomerStupicaStredFixVertical.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            RychlomerStupicaVlavoFixVertical.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            BackSpeedVerticalFix.gameObject.SetActive(false);
            RedArrowVerticalFix.gameObject.SetActive(false);
            if (airspeed < 0)
            {
                BackSpeedVerticalFix.text = ((int)(Math.Abs(airspeed))).ToString();
                BackSpeedVerticalFix.gameObject.SetActive(true);
                RedArrowVerticalFix.gameObject.SetActive(true);
            }
        }
        
        // Set QNH number
        QnhFixVertical.text = Math.Round(qnh, 2).ToString();

        // Set altitude of aircraft
        VyskomerStupnicaVpravoFixVertical.uvRect = new Rect(0, -0.577f + (altitude / 100), 1, 0.15f);
        VyskomerStupnicaStredFixVertical.uvRect = new Rect(0, -0.018f + (altitude / 1000), 1, 0.13f);
        VyskomerCisloFixVertical.text = ((int)(altitude / 1000)).ToString();

        // Showing altitude in the middle (under 500)
        if (aglAltitude < MAX_ALTITUDE)
        {
            VyskaLowTextFixVertical.text = ((int)aglAltitude).ToString();
            VyskaLowFixVertical.gameObject.SetActive(true);
            LandingCameraVertical.gameObject.SetActive(true);
        } else
        {
            VyskaLowFixVertical.gameObject.SetActive(false);
            LandingCameraVertical.gameObject.SetActive(false);

        }
        
        // Move with vtol aircraft in the middle of the HMD
        RectTransform rectTransform = evtolVertical.GetComponent<RectTransform>();

        if (rectTransform != null) {
            // Check maximum evtol movement
            if (aglAltitude > 100) {
                eVTOLLandingLineFix.fillAmount = 1;
                Vector3 newPosition = new Vector3(130, 10, 0);
                rectTransform.localPosition = newPosition;
            } else {
                float newY = (-68 + aglAltitude * 0.78f);
                Vector3 newPosition = new Vector3(130, newY, 0);
                rectTransform.localPosition = newPosition;
                eVTOLLandingLineFix.fillAmount = aglAltitude / 100;
            }
        }

        // Set engine performance for maximum
        if (engine3 > MAX_ENGINE_RPM) {
            engine3 = MAX_ENGINE_RPM;
        }
        if (engine4 > MAX_ENGINE_RPM) {
            engine4 = MAX_ENGINE_RPM;
        }

        // Fill engine objects
        MotorLavyPrednyFix.fillAmount = engine1 / MAX_ENGINE_RPM;
        MotorLavyZadnyFix.fillAmount = engine3 / MAX_ENGINE_RPM;
        MotorPravyPrednyFix.fillAmount = engine2 / MAX_ENGINE_RPM;
        MotorPravyZadnyFix.fillAmount = engine4 / MAX_ENGINE_RPM;
    }

    void runHMDVertical()
    {
        // Check roll angle
        if (startingRollAngleVerticalPohyb != roll)
        {
            stupnicaKlopeniaVertical.transform.Rotate(new Vector3(0, 0, (roll - startingRollAngleVerticalPohyb)));
            // Set aicraft rotation
            IndikatorKloneniaStredVertical.transform.Rotate(new Vector3(0, 0, (roll - startingRollAngleVerticalPohyb)));
        }
        if (startingRollAngleVerticalPohyb > 60)
        {
            // Play warning effect
            switchOn = true;
            if (switchOn == true && warningSound.isPlaying == false)
            {
                warningSound.Play();   
            } else if (switchOn == false && warningSound.isPlaying == true)
            {
                warningSound.Stop();
            }
            VystrahaVlavoVertical.gameObject.SetActive(false);
            VystrahaVpravoVertical.gameObject.SetActive(true);
            SipkaVlavoVertical.gameObject.SetActive(true);
            SipkaVpravoVertical.gameObject.SetActive(false);
        } else if (startingRollAngleVerticalPohyb < -60)
        {
            // Play warning effect
            switchOn = true;
            if (switchOn == true && warningSound.isPlaying == false)
            {
                warningSound.Play();   
            } else if (switchOn == false && warningSound.isPlaying == true)
            {
                warningSound.Stop();
            }
            VystrahaVlavoVertical.gameObject.SetActive(true);
            VystrahaVpravoVertical.gameObject.SetActive(false);
            SipkaVlavoVertical.gameObject.SetActive(false);
            SipkaVpravoVertical.gameObject.SetActive(true);
        } else
        {
            // Stop playing warning sound when angle is OK
            switchOn = false;
            warningSound.Stop();
            VystrahaVlavoVertical.gameObject.SetActive(false);
            VystrahaVpravoVertical.gameObject.SetActive(false);
            SipkaVlavoVertical.gameObject.SetActive(false);
            SipkaVpravoVertical.gameObject.SetActive(false);
        }

        // Set new angle
        startingRollAngleVerticalPohyb = roll;
        // Check pitch angle
        if (pitch > 60)
        {
            pitch = 60;
        }
        if (pitch < -60)
        {
            pitch = -60;
        }
        // Move with middle part up and down
        stupnicaKlopeniaVertical.uvRect = new Rect(0, (pitch * 0.0082f) + 0.315f, 1, 0.37f);

        // Heading course & heading number
        UkazovatelKurzuVertical.uvRect = new Rect(0.45f + (heading / MAX_HEADING), 0, 0.1f, 0.99f);
        HeadingCisloVertical.text = ((int)heading).ToString();

        // Set speed of aircraft
        int speedInt = (int)airspeed;
        if (airspeed > 0)
        {
            RychlomerStupicaVpravoVertical.uvRect = new Rect(0, (-0.018f + (airspeed/MIDDLE_SPEED)), 1, 0.13f);
            RychlomerStupicaStredVertical.uvRect = new Rect(0, (-0.018f + (((float)((int)(speedInt/MIDDLE_SPEED))/MIDDLE_SPEED))), 1, 0.13f);
            RychlomerStupicaVlavoVertical.uvRect = new Rect(0, (-0.018f + (((float)((int)(speedInt/MAX_SPEED))/MIDDLE_SPEED))), 1, 0.13f);
            BackSpeedVertical.gameObject.SetActive(false);
            RedArrowVertical.gameObject.SetActive(false);
        } else
        {
            RychlomerStupicaVpravoVertical.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            RychlomerStupicaStredVertical.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            RychlomerStupicaVlavoVertical.uvRect = new Rect(0, -0.018f, 1, 0.13f);
            BackSpeedVertical.gameObject.SetActive(false);
            RedArrowVertical.gameObject.SetActive(false);
            if (airspeed < 0)
            {
                BackSpeedVertical.text = ((int)(Math.Abs(airspeed))).ToString();
                BackSpeedVertical.gameObject.SetActive(true);
                RedArrowVertical.gameObject.SetActive(true);
            }
        }

        // Set QNH number
        QnhVertical.text = Math.Round(qnh, 2).ToString();

        // Set altitude of aircraft
        VyskomerStupnicaVpravoVertical.uvRect = new Rect(0, -0.577f + (altitude / 100), 1, 0.15f);
        VyskomerStupnicaStredVertical.uvRect = new Rect(0, -0.018f + (altitude / 1000), 1, 0.13f);
        VyskomerCisloVertical.text = ((int)(altitude / 1000)).ToString();

        // Showing altitude in the middle (under 500)
        if (aglAltitude < MAX_ALTITUDE)
        {
            VyskaLowTextVertical.text = ((int)aglAltitude).ToString();
            VyskaLowVertical.gameObject.SetActive(true);
            LandingCameraVerticalPohyb.gameObject.SetActive(true);
        } else
        {
            VyskaLowVertical.gameObject.SetActive(false);
            LandingCameraVerticalPohyb.gameObject.SetActive(false);

        }

        // Move with vtol aircraft in the middle of the HMD
        Transform headTransform = Camera.main.transform;
        if (aglAltitude >= 0)
        {
            // Check maximum evtol movement
            if (aglAltitude > 100) {
                eVTOLLandingLine.fillAmount = 1;
                Vector3 newPosition = new Vector3((headTransform.localPosition.x + 130), (0.875f + 10), 0.49999999f);
                evtol.gameObject.transform.localPosition = newPosition;
            } else {
                Vector3 newPosition = new Vector3(headTransform.localPosition.x + 130, (-68 + 0.875f + (aglAltitude*0.78f)), 0.49999999f);
                evtol.gameObject.transform.localPosition = newPosition;
                eVTOLLandingLine.fillAmount = aglAltitude / 100;
            }
        }
        else
        {
            Vector3 newPosition = new Vector3((headTransform.localPosition.x + 130), (-68 + headTransform.localPosition.y + 0.875f), 0.49999999f);
            evtol.gameObject.transform.localPosition = newPosition;
            eVTOLLandingLine.fillAmount = 0;
        }
        
        // Set engine performance for maximum
        if (engine3 > MAX_ENGINE_RPM) {
            engine3 = MAX_ENGINE_RPM;
        }
        if (engine4 > MAX_ENGINE_RPM) {
            engine4 = MAX_ENGINE_RPM;
        }

        // Fill engine objects
        MotorLavyPredny.fillAmount = engine1 / MAX_ENGINE_RPM;
        MotorLavyZadny.fillAmount = engine3 / MAX_ENGINE_RPM;
        MotorPravyPredny.fillAmount = engine2 / MAX_ENGINE_RPM;
        MotorPravyZadny.fillAmount = engine4 / MAX_ENGINE_RPM;
    }

    void OnDestroy()
    {
        // Stop recignizer
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }

        running = false;
        receiveThread.Join();
        udpClient.Close();
    }
}
