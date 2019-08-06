﻿using Aquality.Selenium.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.IO;

namespace Aquality.Selenium.Utilities
{
    /// <summary>
    /// Provides methods to get info from JSON files.
    /// </summary>
    public sealed class JsonFile
    {
        private readonly string fileName;
        private readonly string fileContent;

        private JObject JsonObject => JsonConvert.DeserializeObject<JObject>(fileContent);

        /// <summary>
        /// Inistantiates class using desired JSON fileinfo.
        /// </summary>
        /// <param name="fileInfo">JSON fileinfo.</param>
        public JsonFile(FileInfo fileInfo)
        {
            fileContent = FileReader.GetTextFromFile(fileInfo);
            fileName = fileInfo.Name;
        }

        /// <summary>
        /// Inistantiates class using desired resource file info.
        /// </summary>
        /// <param name="resourceFileName"></param>
        public JsonFile(string resourceFileName)
        {
            fileContent = FileReader.GetTextFromResource(resourceFileName);
            fileName = resourceFileName;
        }

        /// <summary>
        /// Gets object from JSON.
        /// </summary>
        /// <param name="jsonPath">Relative JsonPath to the object.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Object from JSON by JsonPath.</returns>
        public T GetObject<T>(string jsonPath)
        {
            var envValue = GetEnvironmentValue(jsonPath);
            if (envValue != null)
            {
                return (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(envValue);
            }

            return GetJsonNode(jsonPath).ToObject<T>();
        }

        /// <summary>
        /// Gets value of object from JSON.
        /// </summary>
        /// <param name="jsonPath">Relative JsonPath to the object.</param>
        /// <returns>Value of JSON object.</returns>
        public object GetValue(string jsonPath)
        {
            return GetEnvironmentValueOrDefault(jsonPath);
        }

        /// <summary>
        /// Checks whether value present on JSON by JsonPath or not.
        /// </summary>
        /// <param name="jsonPath">Relative JsonPath to the object.</param>
        /// <returns>True if present and false otherwise.</returns>
        public bool IsValuePresent(string jsonPath)
        {
            return GetEnvironmentValue(jsonPath) != null || GetJsonNode(jsonPath) != null;
        }

        private object GetEnvironmentValueOrDefault(string jsonPath)
        {
            var envValue = GetEnvironmentValue(jsonPath);
            if(envValue == null)
            {
                var node = GetJsonNode(jsonPath);
                if(node == null)
                {
                    throw new InvalidDataException($"Failed to get value by JPath {jsonPath} from json file {fileName}");
                }
                if (node.Type == JTokenType.Boolean)
                {
                    return node.ToObject<bool>();
                }
                else if (node.Type == JTokenType.Integer)
                {
                    return node.ToObject<int>();
                }
                else if (node.Type == JTokenType.Float)
                {
                    return node.ToObject<float>();
                }
                else if (node.Type == JTokenType.String)
                {
                    return node.ToObject<string>();
                }
                
                return node.ToObject<object>();
            }
            else
            {
                Logger.Instance.Debug($"***** Using variable passed from environment {jsonPath.Substring(1)}={envValue}");
                return envValue;
            }
        }

        private string GetEnvironmentValue(string jsonPath)
        {
            var key = jsonPath.Substring(1);
            return EnvironmentConfiguration.GetVariable(key);
        }

        private JToken GetJsonNode(string jsonPath)
        {
            return JsonObject.SelectToken(jsonPath);
        }
    }
}
