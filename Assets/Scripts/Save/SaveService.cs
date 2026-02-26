using System;
using System.IO;
using UnityEngine;

namespace KitchenCaravan.Save
{
    public class SaveService
    {
        private const string FileName = "save_v1.json";

        public SaveModel Load()
        {
            string path = GetPath();
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonUtility.FromJson<SaveModel>(json);
        }

        public void Save(SaveModel model)
        {
            if (model == null)
            {
                return;
            }

            model.version = SaveModel.Version;
            model.lastSaveUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string json = JsonUtility.ToJson(model, true);
            File.WriteAllText(GetPath(), json);
        }

        private static string GetPath()
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }
    }
}
