﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;
using System.Threading;
using System.Net;
using System.IO;

public class GoogleElevation {

    public List<float> coordinates = new List<float>();
    public List<float> heights = new List<float>();
	public string request;
	// Use this for initialization
	
	
	
	public void SyncGetHeights()
	{
       string request = "";
       for(int i = 0; i < coordinates.Count-1; i+=2)
       {
            request+=coordinates[i]+","+coordinates[i+1];
            if(i != coordinates.Count -2)
            request+="|";
            //request += coordinates[coordinates.Count - 2] + "," + coordinates[coordinates.Count - 1];
            
        }
        
		request = WWW.EscapeURL(request);
		string req = "http://maps.googleapis.com/maps/api/elevation/xml?locations=" + request + "&sensor=false";
        Debug.Log(req);
		WWW googleRequest = new WWW(req);
		request = req;
		while (!googleRequest.isDone)
		{

		}
		if(googleRequest.text.Contains("OVER_QUERY_LIMIT"))
		{
			this.heights = new List<float>();
			for (int i = 0; i < coordinates.Count; i+=2)
			{
				float l = Mathf.Abs(coordinates[i]*100-(int)(coordinates[i]*100));
				float r = Mathf.Abs(coordinates[i+1]*100-(int)(coordinates[i+1]*100));
                float height = 2;//(l+r)*100;
				this.heights.Add(height);
			}
		}
		else
		{
			XmlDocument XMLFile = new XmlDocument();
			XMLFile.LoadXml(googleRequest.text);
			List<float> heightsResult = ParseXML(XMLFile);
			this.heights = heightsResult;
		}
	}

    private void AsyncGetHeightsAux()
    {
        string request = "";
        for (int i = 0; i < coordinates.Count - 1; i += 2)
        {
            request += coordinates[i] + "," + coordinates[i + 1];
            if (i != coordinates.Count - 2)
                request += "|";
            //request += coordinates[coordinates.Count - 2] + "," + coordinates[coordinates.Count - 1];

        }

        string req = "http://maps.googleapis.com/maps/api/elevation/xml?locations=" + request + "&sensor=false";
        Debug.Log(req);

        WebResponse webResponse = null;
        HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(req);
        webResponse = httpreq.GetResponse();
        Stream dataStream = webResponse.GetResponseStream();
        // Open the stream using a StreamReader for easy access.
        StreamReader reader = new StreamReader(dataStream);
        // Read the content.
        string responseFromServer = reader.ReadToEnd();

        if (responseFromServer.Contains("OVER_QUERY_LIMIT"))
        {
            this.heights = new List<float>();
            for (int i = 0; i < coordinates.Count; i += 2)
            {
                float l = Mathf.Abs(coordinates[i] * 100 - (int)(coordinates[i] * 100));
                float r = Mathf.Abs(coordinates[i + 1] * 100 - (int)(coordinates[i + 1] * 100));
                float height = (l + r) * 100;
                this.heights.Add(height);
            }
        }
        else
        {
            XmlDocument XMLFile = new XmlDocument();
            XMLFile.LoadXml(responseFromServer);
            List<float> heightsResult = ParseXML(XMLFile);
            this.heights = heightsResult;
        }
    }

	public void AsyncGetHeights()
	{
        Thread workerThread = new Thread(() => AsyncGetHeightsAux());
        workerThread.Start();

	}

	List<float> ParseXML(XmlDocument d)
	{
		List<float> result = new List<float>();
		XmlNode googleNode = d["ElevationResponse"];

		foreach(XmlElement ele in googleNode.ChildNodes)
		{
			if(ele.LocalName == "result")
			{
				XmlNode resultNode = ele as XmlNode;

				foreach(XmlElement rele in resultNode.ChildNodes)
				{
					if(rele.LocalName == "location")
					{
						XmlNode locationNode = rele as XmlNode;
						foreach(XmlElement lele in locationNode.ChildNodes)
						{
							if(lele.LocalName == "lat")
							{
								//Do nothing
							}
							if(lele.LocalName == "lng")
							{
								//Do nothing
							}
						}
						
					}
                    if(rele.LocalName == "elevation")
                    {
                        result.Add(float.Parse(rele.InnerText, CultureInfo.CurrentCulture));
                    }
                    if(rele.LocalName == "resolution")
                    {
                        // Do nothing
                    }
				}
			}
		}
		return result;
	}

}


