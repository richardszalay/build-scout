using System;
using System.Collections.Generic;
using System.IO;

namespace RichardSzalay.PocketCiTray.Services
{
    public class SettingsDictionarySerializer : ISettingsDictionarySerializer
    {
        private static readonly Dictionary<ValueType, Func<BinaryReader, object>> readers = new Dictionary<ValueType, Func<BinaryReader, object>>
        {
            { ValueType.String, r => r.ReadString() },
            { ValueType.Int32, r => r.ReadInt32() },
            { ValueType.Int64, r => r.ReadInt64() },
            { ValueType.DateTimeOffset, r => new DateTimeOffset(r.ReadInt64(), new TimeSpan(r.ReadInt64())) },
        };

        private static readonly Dictionary<ValueType, Action<BinaryWriter, object>> writers = new Dictionary<ValueType, Action<BinaryWriter, object>>
        {
            { ValueType.String, (w,v) => w.Write((string)v) },
            { ValueType.Int32, (w,v) => w.Write((Int32)v) },
            { ValueType.Int64, (w,v) => w.Write((Int64)v) },
            { ValueType.DateTimeOffset, (w,v) =>
            {
                w.Write(((DateTimeOffset) v).Ticks);
                w.Write(((DateTimeOffset) v).Offset.Ticks);
            } }
        };

        public void Serialize(IDictionary<string, object> settings, Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)settings.Count);

                foreach (var kvp in settings)
                {
                    var valueType = GetValueType(kvp.Value);
                    
                    writer.Write(kvp.Key);
                    writer.Write((byte)valueType);
                    writers[valueType](writer, kvp.Value);
                }
            }
        }

        public IDictionary<string, object> Deserialize(Stream stream)
        {
            var settings = new Dictionary<string, object>();

            using (var reader = new BinaryReader(stream))
            {
                int entryCount = reader.ReadInt16();

                for (int i=0; i<entryCount; i++)
                {
                    string key = reader.ReadString();

                    var type = (ValueType)reader.ReadByte();

                    settings[key] = readers[type](reader);
                }
            }

            return settings;
        }

        private static ValueType GetValueType(object value)
        {
            if (value is String) return ValueType.String;
            if (value is Int32) return ValueType.Int32;
            if (value is Int64) return ValueType.Int64;
            if (value is DateTimeOffset) return ValueType.DateTimeOffset;
            
            throw new NotSupportedException(value == null ? "null" : value.GetType().FullName);
        }

        private enum ValueType : byte
        {
            String = 0,
            Int32 = 1,
            Int64 = 2,
            DateTimeOffset = 3
        }
    }
}