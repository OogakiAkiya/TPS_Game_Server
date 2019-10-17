using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Convert
{
    public static Vector3 GetVector3(byte[] _data, int _beginPoint = 0, bool _x = true, bool _y = true, bool _z = true)
    {
        Vector3 vect = Vector3.zero;
        if (_data.Length < sizeof(float) * 3) return vect;
        if (_x) vect.x = System.BitConverter.ToSingle(_data, _beginPoint + 0 * sizeof(float));
        if (_y) vect.y = System.BitConverter.ToSingle(_data, _beginPoint + 1 * sizeof(float));
        if (_z) vect.z = System.BitConverter.ToSingle(_data, _beginPoint + 2 * sizeof(float));
        return vect;
        
    }
    public static byte[] GetByteVector3(Vector3 _vector, bool _x = true, bool _y = true, bool _z = true)
    {
        byte[] vect = new byte[sizeof(float) * 3];
        if (_x) System.Buffer.BlockCopy(System.BitConverter.GetBytes(_vector.x), 0, vect, 0 * sizeof(float), sizeof(float));
        if (_y) System.Buffer.BlockCopy(System.BitConverter.GetBytes(_vector.y), 0, vect, 1 * sizeof(float), sizeof(float));
        if (_z) System.Buffer.BlockCopy(System.BitConverter.GetBytes(_vector.z), 0, vect, 2 * sizeof(float), sizeof(float));

        return vect;
    }


    public static byte[] Conversion(int _data)
    {
        return System.BitConverter.GetBytes(_data);
    }

    public static byte[] Conversion(float _data)
    {
        return System.BitConverter.GetBytes(_data);
    }

    public static byte[] Conversion(double _data)
    {
        return System.BitConverter.GetBytes(_data);
    }

}
