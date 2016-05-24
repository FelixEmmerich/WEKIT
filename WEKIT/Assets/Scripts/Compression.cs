using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Ionic.Zip;

public class Compression
{
    public static void AddItemToCompoundArchive(MemoryStream mStream, string fullpath, string entryName)
    {
        bool fileExists = File.Exists(fullpath);
        using (ZipFile zipFile =fileExists?new ZipFile(fullpath):new ZipFile())
        {
            //serialize item to memorystream
            using (mStream)
            {
                if (zipFile.ContainsEntry(entryName))
                {
                    zipFile.RemoveEntry(entryName);
                }
                zipFile.AddEntry(entryName, mStream);
                zipFile.Save(fullpath);
            }
        }
    }

    public static void AddItemToCompoundArchive<T>(MemoryStream mStream,string fullpath, string entryName, T item)
    {
        if (!item.GetType().IsSerializable)
            throw new ArgumentException("item must be serializable");
        //serialize item to memorystream
        using (mStream)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(mStream, item);
            mStream.Position = 0;
            AddItemToCompoundArchive(mStream, fullpath, entryName);
        }
    }

    public static void AddItemToCompoundArchive<T>(string fullpath, string entryName, T item)
    {
        AddItemToCompoundArchive(new MemoryStream(), fullpath,entryName,item);
    }

    //Despite appearing similar to the above method, they can not be combined/simplified in a sensible way since XmlSerializer does not implement IFormatter
    public static void AddItemToCompoundArchive<T>(string fullpath, string entryName, T item, XmlSerializer formatter)
    {
        if (!item.GetType().IsSerializable)
            throw new ArgumentException("item must be serializable");


        using (ZipFile zipFile = new ZipFile(fullpath))
        {
            //serialize item to memorystream
            using (MemoryStream m = new MemoryStream())
            {
                formatter.Serialize(m, item);
                m.Position = 0;
                if (zipFile.ContainsEntry(entryName))
                {
                    zipFile.RemoveEntry(entryName);
                }
                zipFile.AddEntry(entryName, m);
                zipFile.Save();
            }
        }
    }

    public static T GetItemFromCompoundArchive<T>(string fullpath, string entryName)
    {
        //get the stream from the archive
        using (MemoryStream m = new MemoryStream())
        {
            using (ZipFile zipFile = new ZipFile(fullpath))
            {
                if (zipFile.ContainsEntry(entryName))
                {
                    ZipEntry e = zipFile[entryName];
                    e.Extract(m);
                    m.Position = 0;
                    //now serialize it back to the correct type
                    IFormatter formatter = new BinaryFormatter();
                    T item = (T)formatter.Deserialize(m);
                    return item; 
                }
                return default(T);
            }
        }

    }

    public static byte[] GetByteArrayFromCompoundArchive(string fullpath, string entryName)
    {
        //get the stream from the archive
        using (MemoryStream m = new MemoryStream())
        {
            using (ZipFile zipFile = new ZipFile(fullpath))
            {
                if (zipFile.ContainsEntry(entryName))
                {
                    ZipEntry e = zipFile[entryName];
                    e.Extract(m);
                    m.Position = 0;
                    return m.ToArray();
                }
                return new byte[0];
            }
        }
    }

    public static T GetItemFromCompoundArchive<T>(string fullpath, string entryName, XmlSerializer formatter)
    {
        //get the stream from the archive
        using (MemoryStream m = new MemoryStream())
        {
            using (ZipFile zipFile = new ZipFile(fullpath))
            {
                if (zipFile.ContainsEntry(entryName))
                {
                    ZipEntry e = zipFile[entryName];
                    e.Extract(m);
                    m.Position = 0;
                    //now serialize it back to the correct type
                    T item = (T)formatter.Deserialize(m);
                    return item; 
                }
                return default(T);
            }
        }
    }

    /// <summary>
    /// Returns whether deletion was successful
    /// </summary>
    /// <param name="fullpath"></param>
    /// <param name="entryName"></param>
    /// <param name="deleteIfEmpty">Should file be deleted if the last entry was removed?</param>
    /// <returns></returns>
    public static bool RemoveItemFromCompoundArchive(string fullpath, string entryName, bool deleteIfEmpty)
    { 
        using (ZipFile zipFile = ZipFile.Read(fullpath))
        {
            ZipEntry e = zipFile[entryName];
            if (zipFile.ContainsEntry(entryName))
            {
                zipFile.RemoveEntry(e);
                zipFile.Save();

                //Delete the archive if the last entry was removed
                if (zipFile.Count == 0 && deleteIfEmpty)
                {
                    zipFile.Dispose();
                    File.Delete(fullpath);
                }
                return true;
            }
            return false;
        }
    }

}