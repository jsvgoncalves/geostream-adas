using UnityEngine;
using System.Collections;
using System.Xml;
using System.Globalization;

public class GoogleGeoRef {


	public float latitude;
	public float longitude;


	public void SyncGetAddress( string address)
	{
		address = WWW.EscapeURL(address);
		string req = "http://maps.googleapis.com/maps/api/geocode/xml?address="+address+"&sensor=false";
		
		WWW request = new WWW(req);

		while (!request.isDone)
		{

		}
		XmlDocument XMLFile = new XmlDocument();
		XMLFile.LoadXml(request.text);
		Vector2 latlon = ParseXML(XMLFile);
		latitude = latlon[0];
		longitude = latlon[1];

	}

	public IEnumerator GetAddress (string address)
	{
		address = WWW.EscapeURL(address);
		string req = "http://maps.googleapis.com/maps/api/geocode/xml?address="+address+"&sensor=false";

		WWW request = new WWW(req);

		yield return request;

		XmlDocument XMLFile = new XmlDocument();
		XMLFile.LoadXml(request.text);
		Vector2 latlon = ParseXML(XMLFile);
		latitude = latlon[0];
		longitude = latlon[1];

	}

	Vector2 ParseXML(XmlDocument d)
	{
		Vector2 result = new Vector2();
		XmlNode googleNode = d["GeocodeResponse"];

		foreach(XmlElement ele in googleNode.ChildNodes)
		{
			if(ele.LocalName == "result")
			{
				XmlNode resultNode = ele as XmlNode;

				foreach(XmlElement rele in resultNode.ChildNodes)
				{
					if(rele.LocalName == "geometry")
					{
						XmlNode geometryNode = rele as XmlNode;

						foreach(XmlElement gele in geometryNode.ChildNodes)
						{
							if(gele.LocalName == "location")
							{
								XmlNode locationNode = gele as XmlNode;
								foreach(XmlElement lele in locationNode.ChildNodes)
								{
									if(lele.LocalName == "lat")
									{
										result[0] = float.Parse(lele.InnerText ,CultureInfo.CurrentCulture);
									}
									if(lele.LocalName == "lng")
									{
										result[1] = float.Parse(lele.InnerText, CultureInfo.CurrentCulture);
									}
								}
							
							}
						}
					}
				}
			}
		}
		return result;
	}

}
