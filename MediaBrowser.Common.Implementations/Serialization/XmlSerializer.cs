﻿using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml;
using CommonIO;
using MediaBrowser.Common.IO;

namespace MediaBrowser.Common.Implementations.Serialization
{
    /// <summary>
    /// Provides a wrapper around third party xml serialization.
    /// </summary>
    public class XmlSerializer : IXmlSerializer
    {
		private IFileSystem _fileSystem;

		public XmlSerializer(IFileSystem fileSystem) 
		{
			_fileSystem = fileSystem;
		}

        // Need to cache these
        // http://dotnetcodebox.blogspot.com/2013/01/xmlserializer-class-may-result-in.html
        private readonly ConcurrentDictionary<string, System.Xml.Serialization.XmlSerializer> _serializers =
            new ConcurrentDictionary<string, System.Xml.Serialization.XmlSerializer>();

        private System.Xml.Serialization.XmlSerializer GetSerializer(Type type)
        {
            var key = type.FullName;
            return _serializers.GetOrAdd(key, k => new System.Xml.Serialization.XmlSerializer(type));
        }

        /// <summary>
        /// Serializes to writer.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="writer">The writer.</param>
        private void SerializeToWriter(object obj, XmlTextWriter writer)
        {
            writer.Formatting = Formatting.Indented;
            var netSerializer = GetSerializer(obj.GetType());
            netSerializer.Serialize(writer, obj);
        }

        /// <summary>
        /// Deserializes from stream.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>System.Object.</returns>
        public object DeserializeFromStream(Type type, Stream stream)
        {
            using (var reader = new XmlTextReader(stream))
            {
                var netSerializer = GetSerializer(type);
                return netSerializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Serializes to stream.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="stream">The stream.</param>
        public void SerializeToStream(object obj, Stream stream)
        {
            using (var writer = new XmlTextWriter(stream, null))
            {
                SerializeToWriter(obj, writer);
            }
        }

        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="file">The file.</param>
        public void SerializeToFile(object obj, string file)
        {
            using (var stream = new FileStream(file, FileMode.Create))
            {
                SerializeToStream(obj, stream);
            }
        }

        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="file">The file.</param>
        /// <returns>System.Object.</returns>
        public object DeserializeFromFile(Type type, string file)
        {
            using (var stream = _fileSystem.OpenRead(file))
            {
                return DeserializeFromStream(type, stream);
            }
        }

        /// <summary>
        /// Deserializes from bytes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>System.Object.</returns>
        public object DeserializeFromBytes(Type type, byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return DeserializeFromStream(type, stream);
            }
        }
    }
}
