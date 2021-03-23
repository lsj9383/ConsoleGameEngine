public abstract class Codecs
{

    public abstract byte[] Encode<T>(T message);
    public abstract T Decode<T>(byte[] data);
}