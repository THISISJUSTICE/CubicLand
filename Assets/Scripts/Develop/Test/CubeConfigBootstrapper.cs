#if TEST_DEVELOP && UNITY_STANDALONE_WIN && !UNITY_EDITOR
using Commar.CubicLand.Cube;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace Commar
{
    public static class CubeConfigBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "CubeConfig.json");
            try
            {
                bool isFileLoaded = File.Exists(filePath);
                XElement root = new XElement("root", new XAttribute("type", "object"));

                if (isFileLoaded)
                {
                    using FileStream stream = File.OpenRead(filePath);
                    using XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max);
                    root = XElement.Load(reader, LoadOptions.PreserveWhitespace);

                    if ((string)root.Attribute("type") != "object")
                        throw new InvalidDataException("Expected a JSON object.");
                }

                foreach (PropertyInfo property in typeof(CubeConfig).GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    if (property.GetSetMethod(true)?.IsPrivate != true)
                        continue;

                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(property.PropertyType,
                        new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
                    if (isFileLoaded)
                    {
                        XElement element = root.Element(property.Name);
                        if (element == null)
                            continue;

                        using XmlReader reader = element.CreateReader();
                        property.SetValue(null, serializer.ReadObject(reader, false));
                    }
                    else
                    {
                        using XmlWriter writer = root.CreateWriter();
                        writer.WriteStartElement(property.Name);
                        serializer.WriteObjectContent(writer, property.GetValue(null));
                        writer.WriteEndElement();
                    }
                }

                if (!isFileLoaded)
                {
                    using FileStream stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
                    using XmlDictionaryWriter writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, false, true, "  ");
                    root.WriteTo(writer);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to initialize CubeConfig ({filePath}): {exception.Message}");
            }
        }
    }
}
#endif