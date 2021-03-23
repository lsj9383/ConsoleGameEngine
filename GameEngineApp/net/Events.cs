using System;
using System.Collections;
using System.Collections.Generic;

public class Events
{
    public delegate void Callback(byte[] data);
    public Dictionary<int, Callback> callbacks = new Dictionary<int, Callback>();

    public void AddCallback(int type, Callback cb) {
        if (!callbacks.ContainsKey(type)) {
            callbacks[type] = null;
        }

        callbacks[type] += cb;
    }

    public void Invoke(int type, byte[] data)
    {
        if (callbacks.ContainsKey(type)) {
            callbacks[type](data);
        } else {
            Console.WriteLine($"Event type invalid: {type}");
        }
    }
}
