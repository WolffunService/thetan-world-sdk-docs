using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Wolffun.Log;
using Wolffun.RestAPI;

namespace Wolffun.RestAPI
{
    public struct LocalDataLoadSaver<T> where T : struct, ICustomDefaultable<T>
    {
        public bool IsDoneLoad { get; private set; }

        private static string GetFilePath(string fileName)
        {
            var builder = Utils.StringBuilderPool.Get();
            builder.Append(Application.persistentDataPath);
            builder.Append('/');
            builder.Append(fileName);
            builder.Append(".dat");
            var filePath = builder.ToString();
            Utils.StringBuilderPool.Release(builder);
            return filePath;
        }

        public T LoadDataLocal(string fileName)
        {
            var filePath = GetFilePath(fileName);
#if UNITY_WEBGL
        var isHasFile = PlayerPrefs.HasKey(filePath);
#else
            var isHasFile = File.Exists(filePath);
#endif

#if THETAN_SIMULATION
        isHasFile = false;
#endif
            if (isHasFile)
            {
                try
                {
#if UNITY_WEBGL
                var savedData = PlayerPrefs.GetString(filePath);
#else
                    var savedData = File.ReadAllText(filePath);
#endif
                    var loadedData = JsonConvert.DeserializeObject<T>(savedData);
                    CommonLog.Log("LoadData complete " + filePath + "\n" + savedData);
                    IsDoneLoad = true;
                    return loadedData;
                }
                catch (Exception ex)
                {
                    AdminLog.LogError("Load data --" + filePath + "-- is error: " + ex.GetBaseException() + "\n" +
                                      ex.StackTrace);
                    IsDoneLoad = true;
                    return new T().SetDefault();
                }
            }

            IsDoneLoad = true;
            T result = new T();
            result.SetDefault();
            return result;
        }

        public void SaveDataLocal(T data, string fileName)
        {
            var filePath = GetFilePath(fileName);

#if !THETAN_SIMULATION
            try
            {
                var saveData = JsonConvert.SerializeObject(data);
#if UNITY_WEBGL
            PlayerPrefs.SetString(_dataLink, saveData);
            PlayerPrefs.Save();
#else
                File.WriteAllText(filePath, saveData);
#endif

                CommonLog.Log("SaveData complete" + filePath + "\n" + saveData);
            }
            catch (Exception ex)
            {
                AdminLog.LogError("Save data --" + data.GetType() + "-- is error: " + ex.GetBaseException() + "\n" +
                                  ex.StackTrace);
            }
#endif
        }
    }
}