using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;

public enum WeatherType { Rain, Fog, Clear, Snow, Thunder, Wind, Clouds};

public class LocalWeather
{
    public float lat;
    public float lon;
    public WeatherType weather;
    public string city;
    public string country;
    public float temperature;
    public float humidity;
    public float pressure;
    public float windSpeed;
    public float windDirection;
    public float cloudIntensity;
    public bool precipitation;
    public string lastUpdate;


    public void SyncGetWeather()
    {
        string request = "";

        request = WWW.EscapeURL(request);
        string req = "http://api.openweathermap.org/data/2.5/weather?lat=" + lat + "&lon=" + lon + "&mode=xml";
        Debug.Log(req);
        WWW weatherReq = new WWW(req);
        request = req;
        while (!weatherReq.isDone)
        {

        }
       
        XmlDocument XMLFile = new XmlDocument();
        XMLFile.LoadXml(weatherReq.text);
        ParseXML(XMLFile);
           
        
    }

    private void ParseXML(XmlDocument d)
	{
		List<float> result = new List<float>();
		XmlNode currentWeatherNode = d["current"];

        foreach (XmlElement currentChild in currentWeatherNode.ChildNodes)
		{
            if (currentChild.LocalName == "city")
			{
                XmlNode cityNode = currentChild as XmlNode;
                city = currentChild.GetAttribute("name");
                foreach (XmlElement cityChild in cityNode.ChildNodes)
                {
                    if (cityChild.LocalName == "coord")
                    {
                        //Do nothing, we already have the lat lon
                    }
                    if (cityChild.LocalName == "country")
                    {
                        //Do nothing, we already have the lat lon
                        country = cityChild.InnerText;
                    }
                }
		
            }
            if (currentChild.LocalName == "temperature")
            {
                temperature = float.Parse(currentChild.GetAttribute("value"));
            }
            if (currentChild.LocalName == "humidity")
            {
                humidity = float.Parse(currentChild.GetAttribute("value"));
            }
            if (currentChild.LocalName == "pressure")
            {
                pressure = float.Parse(currentChild.GetAttribute("value"));
            }

            if (currentChild.LocalName == "wind")
            {
                XmlNode windNode = currentChild as XmlNode;
                foreach (XmlElement windChild in windNode.ChildNodes)
                {
                    if (windChild.LocalName == "speed")
                    {
                        windSpeed = float.Parse(windChild.GetAttribute("value"));
                        //Do nothing, we already have the lat lon
                    }
                    if (windChild.LocalName == "direction")
                    {
                        //Do nothing, we already have the lat lon
                        windDirection = float.Parse(windChild.GetAttribute("value"));
                    }
                }
            }
            if (currentChild.LocalName == "clouds")
            {
                cloudIntensity = float.Parse(currentChild.GetAttribute("value"));
            }
            if (currentChild.LocalName == "precipitation")
            {
                string value = currentChild.GetAttribute("mode");
                if(value.Contains("no"))
                    precipitation = false;
                else
                    precipitation = true;
            }
            if (currentChild.LocalName == "weather")
            {
                int weatherCode = int.Parse(currentChild.GetAttribute("number"));
                int mainWeather = weatherCode/100;
                if (mainWeather == 200)
                    weather = WeatherType.Thunder;
                else if (mainWeather == 3)
                    weather = WeatherType.Rain;
                else if (mainWeather == 5)
                    weather = WeatherType.Rain;
                else if (mainWeather == 6)
                    weather = WeatherType.Snow;
                else if (mainWeather == 7)
                    weather = WeatherType.Fog;
                else if (mainWeather == 8)
                    weather = WeatherType.Clouds;
                else weather = WeatherType.Clear;
            }
            if (currentChild.LocalName == "lastupdate")
            {
                lastUpdate = currentChild.GetAttribute("value");
            }
        }
    }

}