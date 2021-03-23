using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonCodecs : Codecs
{

    public override byte[] Encode<T>(T message)
    {
        string json = JsonSerializer.Serialize(message);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    public override T Decode<T>(byte[] data)
    {
        string json = System.Text.Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<T>(json);
    }
}