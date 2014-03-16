using UnityEngine;
using System.Threading;
using System.Collections;
using System.IO;
using System.Net;


public class MapTexture {
    string mq_request = "http://open.mapquestapi.com/staticmap/v4/getmap?key=";
    string mq_fit = "&bestfit="; //40.073246,-76.66658,39.90089,-76.31697";
    string mq_size = "&size=";
    string mq_sizeValue = "1024,1024";
    string mq_mapType = "&type=hyb";
    string mq_margin = "&margin=0";
    string mq_imageFormat = "&imagetype=png";
    public string mq_key = "Fmjtd%7Cluur2l62nd%2C7g%3Do5-90yahz";
    public Stream mq_dataStream;
    public Texture2D texture;
    public int mq_defaultSize = 1024;
    int mq_computedSize = 1024;

    // Bing maps

    string bm_request = " http://dev.virtualearth.net/REST/V1/Imagery/Map/";
    string bm_keyValue = "AhtGytZkN0NkqJJ8VJFF8fgtcbGn0mgljx2fexZIENd2qPFDCk69I9w4yYZtcA6U";
    string bm_mapType = "aerial?";
    string bm_mapArea = "mapArea=";
    string bm_size = "&ms=";
    string bm_key = "&key=";

    public void getTexture(string minLon, string minLat, string maxLon, string maxLat, string persistentPath, Material m)
    {
        Thread workerThread = new Thread(() => getTextureWorkerThreadBM(minLon, minLat, maxLon, maxLat, persistentPath,m));
        workerThread.Start();
   
    }

    public void readTextureFromCache() { }

    public void saveTextureToFile(Texture2D tx, string filename)
    {

        byte[] bytes = tx.EncodeToPNG();
     
        FileStream file = File.Open(filename, FileMode.Create);
        var binary = new BinaryWriter(file);
        binary.Write(bytes);
        file.Close();
    }

    private void computeCorrectedSize(string minLon, string minLat, string maxLon, string maxLat)
    {
        GeoUTMConverter bl = new GeoUTMConverter();
        bl.ToUTM(double.Parse(minLat),double.Parse(minLon));
        
        GeoUTMConverter tr = new GeoUTMConverter();
        tr.ToUTM(double.Parse(maxLat), double.Parse(maxLon));

        double width = tr.X - bl.X;
        double heigth = tr.Y - bl.Y;

        Debug.Log("W " + width + " H " + heigth );

        mq_computedSize = (int)(mq_defaultSize * (width / heigth));
    }

    
    void getTextureWorkerThreadBM(string minLon, string minLat, string maxLon, string maxLat, string persistentPath, Material material)
    {
        computeCorrectedSize( minLon,  minLat,  maxLon,  maxLat);
        Directory.CreateDirectory(Path.GetDirectoryName(persistentPath + "Assets/Resources/MapTextures/"));
        string cacheFile = persistentPath + "Assets/Resources/MapTextures/" + minLat + "," + minLon + "," + maxLat + "," + maxLon + ".png";

        if (!File.Exists(cacheFile)) // if we haven't cache it we download it and save it.
        {
            string url = bm_request + bm_mapType + bm_mapArea + minLat + "," + minLon + "," + maxLat + "," + maxLon + bm_size + mq_computedSize + "," + mq_defaultSize + bm_key + bm_keyValue;
            Debug.Log(url);
            WebResponse webResponse = null;
            HttpWebRequest r = (HttpWebRequest)WebRequest.Create(url);
            webResponse = r.GetResponse();
            mq_dataStream = webResponse.GetResponseStream();

            Loom.QueueOnMainThread(() =>
                {
                    texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
                    texture.LoadImage(ReadFully(mq_dataStream));
                    material.SetTexture("_MainTex", texture);
                    //material.SetTexture("_MainTex", mt.texture);
                    //char[] imageData = new char[dataStream.Length];
                    //reader.Read(imageData, 0, (int)dataStream.Length);
                    saveTextureToFile(texture, cacheFile);
                });
        }
        else
        {
            var bytes = System.IO.File.ReadAllBytes(cacheFile);
            Loom.QueueOnMainThread(() =>
               {
                   texture = new Texture2D(1, 1);
                   texture.LoadImage(bytes);
                   material.SetTexture("_MainTex", texture);
               });
        }

    }

    void getTextureWorkerThreadMQ(string minLon, string minLat, string maxLon, string maxLat, string persistentPath)
    {
        computeCorrectedSize( minLon,  minLat,  maxLon,  maxLat);
        Directory.CreateDirectory(Path.GetDirectoryName(persistentPath + "Assets/Resources/MapQuest/"));
        string cacheFile = persistentPath + "Assets/Resources/OSM/" + minLon + "," + minLat + "," + maxLon + "," + maxLat + ".png";
        string xmlString = "";
        //if (!File.Exists(cacheFile)) // if we haven't cache it we download it and save it.
        //{
        string url = mq_request + mq_key + mq_fit + minLat + "," + minLon + "," + maxLat + "," + maxLon + mq_size + mq_computedSize + "," + mq_defaultSize + mq_mapType + mq_imageFormat + mq_margin;
            Debug.Log(url);
            WebResponse webResponse = null;
            HttpWebRequest r = (HttpWebRequest)WebRequest.Create(url);
            webResponse = r.GetResponse();
            mq_dataStream = webResponse.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(mq_dataStream);
            //char[] imageData = new char[dataStream.Length];
            //reader.Read(imageData, 0, (int)dataStream.Length);

            //Loom.QueueOnMainThread(() =>
            //{
            //        map = t;
            //});
            // Read the content.
            //xmlString = reader.ReadToEnd();
            //Debug.Log("Response was : " + xmlString);
           // System.IO.File.WriteAllText(cacheFile, xmlString);
        //}
        //else // we have it cached, lets read it locally
       // {
            //xmlString = System.IO.File.ReadAllText(cacheFile);
        //}      
    }

    public byte[] ReadFully(Stream input)
    {
        byte[] buffer = new byte[1024 * 1024 * 32];
        using (MemoryStream ms = new MemoryStream())
        {
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }
    }

}
