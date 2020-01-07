using System.Collections;
using System.Collections.Generic;
using System.IO;
public class FileController
{
    private static FileController _instance=new FileController();
    private object lockd=new object();
    public static FileController GetInstance()
    {
        return _instance;
    }

    public void Write(string _fileName,string _writeData,bool _newLine=true)
    {
        string writeData=_writeData;
        if (_newLine) writeData=_writeData + "\n";
        lock (lockd)
        {
            File.AppendAllText(@_fileName + ".log", writeData);
        }
    }

}
