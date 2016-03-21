using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Ionic.Zip;

public class Compression
{
    public static void AddItemToNewArchive<T>(string folder, string fileName, ref T item)
    {
        if (!item.GetType().IsSerializable)
            throw new ArgumentException("item must be serializable");


        using (ZipFile zipFile = new ZipFile())
        {
            //serialize item to memorystream
            using (MemoryStream m = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(m, item);
                m.Position = 0;
                zipFile.AddEntry(fileName, m);
                zipFile.Save(folder + "/" + fileName + ".zip");
            }
        }
    }

    public static void AddItemToCompoundArchive<T>(string fullpath, string entryName, ref T item)
    {
        if (!item.GetType().IsSerializable)
            throw new ArgumentException("item must be serializable");


        using (ZipFile zipFile = new ZipFile(fullpath))
        {
            //serialize item to memorystream
            using (MemoryStream m = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(m, item);
                m.Position = 0;
                zipFile.AddEntry(entryName, m);
                zipFile.Save();
            }
        }
    }

    public static T GetItemFromArchive<T>(string folder, string fileName)
    {
        //get the stream from the archive
        using (MemoryStream m = new MemoryStream())
        {
            using (ZipFile zipFile = new ZipFile(folder+"/"+fileName+".zip"))
            {
                ZipEntry e = zipFile[fileName];
                e.Extract(m);
                m.Position = 0;
                //now serialize it back to the correct type
                IFormatter formatter = new BinaryFormatter();
                T item = (T)formatter.Deserialize(m);
                return item;
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
                ZipEntry e = zipFile[entryName];
                e.Extract(m);
                m.Position = 0;
                //now serialize it back to the correct type
                IFormatter formatter = new BinaryFormatter();
                T item = (T)formatter.Deserialize(m);
                return item;
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
