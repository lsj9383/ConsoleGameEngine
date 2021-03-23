using System;
using System.Collections;
using System.Collections.Generic;

public class NetworkPackage
{
    public byte[] data;
    public int type = 0;

    public byte[] Encode(bool ignoreLength = false) {
        // 输出的 payload 格式为 <length> <type> <data>
        // 其中 <length> = <type>.length + <data>.length
        int payloadSize = sizeof(int) + sizeof(int) + data.Length;

        // dataSize = data_type.length + data.length
        byte[] dataSizeBytes = BitConverter.GetBytes(sizeof(int) + data.Length);
        if (BitConverter.IsLittleEndian) {
            // converter big-endian
            Array.Reverse(dataSizeBytes);
        }

        byte[] typeBytes = BitConverter.GetBytes(type);
        if (BitConverter.IsLittleEndian) {
            // converter big-endian
            Array.Reverse(typeBytes);
        }

        int offset = 0;
        byte[] payload = new byte[payloadSize];

        if (!ignoreLength) {
            Array.Copy(dataSizeBytes, 0, payload, offset, dataSizeBytes.Length);
            offset += dataSizeBytes.Length;
        }

        Array.Copy(typeBytes, 0, payload, offset, typeBytes.Length);
        offset += typeBytes.Length;

        Array.Copy(data, 0, payload, offset, data.Length);

        return payload;
    }

    public static NetworkPackage Decode(byte[] payload) {
        // 传入的 payload 格式为 <type> <data>
        byte[] typeBytes = new byte[sizeof(int)];
        Array.Copy(payload, 0, typeBytes, 0, sizeof(int));
        if (BitConverter.IsLittleEndian) {
            Array.Reverse(typeBytes);
        }
        int type = BitConverter.ToInt32(typeBytes, 0);

        // 将 payload 中 type 字段后的信息提取出
        byte[] data = new byte[payload.Length - sizeof(int)];
        Array.Copy(payload, typeBytes.Length, data, 0, payload.Length - typeBytes.Length);

        NetworkPackage p = new NetworkPackage();
        p.type = type;
        p.data = data;
        return p;
    }
}
