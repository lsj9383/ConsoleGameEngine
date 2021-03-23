using System;
using System.Collections.Generic;


class KCPDatagram {
    public static readonly int CMD_SIZE = 2;
    public static readonly int CONV_SIZE = 4;

    public enum Command : ushort {
        NONE = 0,
        KCP,
        SYN,
        RST,
    }

    public Command cmd;
    public uint conv;

    public static byte[] BuildRST(uint conv) {
        byte[] cmdBytes = BitConverter.GetBytes((ushort)KCPDatagram.Command.RST);
        if (BitConverter.IsLittleEndian) {
            // converter big-endian
            Array.Reverse(cmdBytes);
        }

        byte[] conv_bytes = BitConverter.GetBytes(conv);
        if (BitConverter.IsLittleEndian) {
            // converter big-endian
            Array.Reverse(conv_bytes);
        }

        int offset = 0;
        byte[] p = new byte[cmdBytes.Length + conv_bytes.Length];

        Array.Copy(cmdBytes, 0, p, offset, cmdBytes.Length);
        offset += cmdBytes.Length;

        Array.Copy(conv_bytes, 0, p, offset, conv_bytes.Length);

        return p;
    }

    public static byte[] BuildKCP(byte[] bytes, int size) {
        int dataLength = CMD_SIZE + size;

        byte[] cmdBytes = BitConverter.GetBytes((ushort)KCPDatagram.Command.KCP);
        if (BitConverter.IsLittleEndian) {
            // converter big-endian
            Array.Reverse(cmdBytes);
        }

        int offset = 0;
        byte[] p = new byte[dataLength];

        Array.Copy(cmdBytes, 0, p, offset, cmdBytes.Length);
        offset += cmdBytes.Length;

        Array.Copy(bytes, 0, p, offset, size);
        return p;
    }

    public static Command GetCMD(byte[] bytes) {
        byte[] cmdBytes = new byte[CMD_SIZE];
        Array.Copy(bytes, 0, cmdBytes, 0, CMD_SIZE);
        if (BitConverter.IsLittleEndian) {
            Array.Reverse(cmdBytes);
        }
        return (Command) BitConverter.ToUInt16(cmdBytes, 0);
    }

    public static uint GetConv(byte[] bytes) {
        byte[] convBytes = new byte[CONV_SIZE];
        Array.Copy(bytes, CMD_SIZE, convBytes, 0, CONV_SIZE);
        if (BitConverter.IsLittleEndian) {
            Array.Reverse(convBytes);
        }
        return BitConverter.ToUInt32(convBytes, 0);
    }

    public static byte[] GetData(byte[] bytes, int size) {
        int offset = CMD_SIZE;
        int dataLength = size - CMD_SIZE;

        byte[] dataBytes = new byte[dataLength];
        Array.Copy(bytes, offset, dataBytes, 0, dataBytes.Length);
        return dataBytes;
    }
}
