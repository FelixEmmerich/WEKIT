using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Ionic.Zip;
using UnityEngine;

/// <summary>
/// Class with static methods for saving and loading compressed data.
/// </summary>
public class Compression
{
    /// <summary>
    /// Add the contents of a memorystream as an entry in a .zip archive
    /// </summary>
    /// <param name="fullpath">the full path, including the .zip file extension</param>
    /// <param name="entryName">the full entry name, including possible file extensions</param>
    public static void AddItemToCompoundArchive(MemoryStream mStream, string fullpath, string entryName)
    {
        bool fileExists = File.Exists(fullpath);
        using (ZipFile zipFile = fileExists?new ZipFile(fullpath):new ZipFile())
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

    /// <summary>
    /// Add an object of type T as an entry in a .zip archive
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullpath">the full path, including the .zip file extension</param>
    /// <param name="entryName">the full entry name, including possible file extensions</param>
    /// <param name="item"></param>
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

    /// <summary>
    /// Add an object of type T as an entry in a .zip archive
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullpath">the full path, including the .zip file extension</param>
    /// <param name="entryName">the full entry name, including possible file extensions</param>
    /// <param name="item"></param>
    public static void AddItemToCompoundArchive<T>(string fullpath, string entryName, T item)
    {
        AddItemToCompoundArchive(new MemoryStream(), fullpath,entryName,item);
    }

    //Despite appearing similar to the above method, they can not be combined/simplified in a sensible way since XmlSerializer does not actually implement IFormatter

    /// <summary>
    /// Adds an object of type T to a .zip archive as an XML-formatted file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullpath">the full path, including the .zip file extension</param>
    /// <param name="entryName">the full entry name, including possible file extensions</param>
    /// <param name="item"></param>
    /// <param name="formatter"></param>
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

    /// <summary>
    /// Returns an object of type T from an entry of a .zip archive.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullpath">the full path, including the .zip file extension</param>
    /// <param name="entryName">the full entry name, including possible file extensions</param>
    /// <returns></returns>
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

    /// <summary>
    /// Returns an entry from a .zip archive in the form of a byte array.
    /// </summary>
    /// <param name="fullpath">the full path, including the .zip file extension</param>
    /// <param name="entryName">the full entry name, including possible file extensions</param>
    /// <returns></returns>
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

    /// <summary>
    /// Returns an object of type T from an XML-formatted  text file in a .zip archive
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullpath">the full path, including the .zip file extension</param>
    /// <param name="entryName">the full entry name, including possible file extensions</param>
    /// <param name="formatter"></param>
    /// <returns></returns>
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
    /// Removes a single entry from a .zip archive
    /// </summary>
    /// <param name="fullpath">the full path, including the .zip file extension</param>
    /// <param name="entryName">the full entry name, including possible file extensions</param>
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

    /// <summary>
    /// Returns a byte array from an object of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static byte[] ConvertToBytes<T>(T source)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream m = new MemoryStream();
        bf.Serialize(m, source);
        Debug.Log(m.Length);
        return m.ToArray();
    }

    /// <summary>
    /// Returns an object of type T from a byte array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static T GetFromBytes<T>(byte[] source)
    {
        MemoryStream m = new MemoryStream(source);
        IFormatter formatter = new BinaryFormatter();
        T item = (T)formatter.Deserialize(m);
        return item;
    }

}