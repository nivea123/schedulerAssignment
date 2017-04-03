using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using System.IO;
using System.Text;

/// <summary>
/// Summary description for JSONHelper
/// </summary>
public class JSONHelper
{
    public static string JsonSerializer<T>(T t)
    {
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
        MemoryStream ms = new MemoryStream();
        ser.WriteObject(ms, t);
        string jsonString = Encoding.UTF8.GetString(ms.ToArray());
        ms.Close();
        return jsonString;
    }

    public static T JsonDeserializer<T>(string jsonObject)
    {
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonObject));
        T obj = (T)ser.ReadObject(ms);
        ms.Close();
        return obj;
    }

}