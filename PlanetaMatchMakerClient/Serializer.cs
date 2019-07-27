﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using PlanetaMatchMakerClient;

namespace PlanetaGameLabo {
    public static class Serializer {
        public static int GetSerializedSize<T>() {
            return GetSerializedSizeImpl(typeof(T));
        }

        public static int GetSerializedSize(Type type) {
            return GetSerializedSizeImpl(type);
        }

        public static byte[] Serialize(object obj) {
            var size = GetSerializedSize(obj.GetType());
            var data = new byte[size];
            var pos = 0;
            SerializeImpl(obj, data, ref pos);
            return data;
        }

        public static T Deserialize<T>(byte[] source) {
            var size = GetSerializedSize(typeof(T));
            if (size != source.Length) {
                throw new InvalidSerializationException(
                    $"The size of source ({source.Length}) does not match the size of serialize target type ({typeof(T)}: {size})");
            }

            var pos = 0;
            DeserializeImpl(typeof(T), source, ref pos, out var obj);
            return (T) obj;
        }

        private static readonly HashSet<Type> _directSerializableTypeSet = new HashSet<Type>() {
            typeof(bool), typeof(char), typeof(byte), typeof(sbyte),
            typeof(double), typeof(short), typeof(int), typeof(long),
            typeof(float), typeof(ushort), typeof(uint), typeof(ulong)
        };

        private static readonly Dictionary<Type, Func<byte[], int, object>> _bytesToDirectSerializableTypeConverterDict
            = new Dictionary<Type, Func<byte[], int, object>>() {
                {typeof(bool), (bytes, start_idx) => BitConverter.ToBoolean(bytes, start_idx)},
                {typeof(char), (bytes, start_idx) => BitConverter.ToChar(bytes, start_idx)},
                {typeof(byte), (bytes, start_idx) => bytes[0]},
                {typeof(sbyte), (bytes, start_idx) => (sbyte) bytes[0]},
                {typeof(double), (bytes, start_idx) => BitConverter.ToDouble(bytes, start_idx)},
                {typeof(short), (bytes, start_idx) => BitConverter.ToInt16(bytes, start_idx)},
                {typeof(int), (bytes, start_idx) => BitConverter.ToInt32(bytes, start_idx)},
                {typeof(long), (bytes, start_idx) => BitConverter.ToInt64(bytes, start_idx)},
                {typeof(float), (bytes, start_idx) => BitConverter.ToSingle(bytes, start_idx)},
                {typeof(ushort), (bytes, start_idx) => BitConverter.ToUInt16(bytes, start_idx)},
                {typeof(uint), (bytes, start_idx) => BitConverter.ToUInt32(bytes, start_idx)},
                {typeof(ulong), (bytes, start_idx) => BitConverter.ToUInt64(bytes, start_idx)},
            };

        private static readonly Dictionary<Type, int> _serializedSizeCache = new Dictionary<Type, int>();

        private static int GetSerializedSizeImpl(Type type) {
            if (_serializedSizeCache.ContainsKey(type)) {
                return _serializedSizeCache[type];
            }

            var size = 0;
            if (IsDirectSerializableType(type)) {
                size = GetSerializedSizeOfDirectSerializableType(type);
            }
            else if (IsFieldSerializableType(type)) {
                throw new InvalidSerializationException(
                    $"The type ({type}) is serializable only when it is declared as a field");
            }
            else if (IsComplexSerializableType(type)) {
                size = GetSerializedSizeOfComplexSerializableType(type);
            }
            else {
                throw new InvalidSerializationException(
                    $"The type ({type}) is not serializable. Primitive types, fixed string and struct and class which is sequential are available.");
            }

            _serializedSizeCache.Add(type, size);
            return size;
        }

        private static int GetSerializedSizeOfDirectSerializableType(Type type) {
            return Marshal.SizeOf(type);
        }

        private static int GetSerializedSizeOfFieldSerializableType(FieldInfo field, Type type) {
            // check cache here because this method doesn't called by GetSerializableSizeImpl, which checks cache
            if (_serializedSizeCache.ContainsKey(type)) {
                return _serializedSizeCache[type];
            }

            int size;
            if (type == typeof(string)) {
                size = GetLengthOfFixedLengthAttribute(field);
                _serializedSizeCache.Add(type, size);
            }
            else if (field.FieldType.IsArray) {
                var length = GetLengthOfFixedLengthAttribute(field);
                var element_type = field.FieldType.GetGenericTypeDefinition().GetGenericArguments()[0];
                size = GetSerializedSizeImpl(element_type) * length;
                // Not add to cache because the size is not fixed for same array time.
            }
            else {
                throw new InvalidSerializationException("Invalid type.");
            }

            return size;
        }

        private static int GetSerializedSizeOfComplexSerializableType(Type type) {
            var sum = 0;
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                 BindingFlags.Instance)) {
                if (IsFieldSerializableType(field.FieldType)) {
                    sum += GetSerializedSizeOfFieldSerializableType(field, field.FieldType);
                }
                else {
                    sum += GetSerializedSizeImpl(field.FieldType);
                }
            }

            return sum;
        }

        private static void SerializeImpl(object obj, byte[] destination, ref int pos) {
            var type = obj.GetType();
            if (IsDirectSerializableType(type)) {
                SerializeDirectSerializableType(obj, destination, ref pos);
            }
            else if (IsFieldSerializableType(type)) {
                throw new InvalidSerializationException(
                    $"The type ({type}) is serializable only when it is declared as a field");
            }
            else if (IsComplexSerializableType(type)) {
                SerializeComplexSerializableType(obj, destination, ref pos);
            }
            else {
                throw new InvalidSerializationException(
                    $"The type ({type}) is not serializable. Primitive types, fixed string and struct and class which is sequential are available.");
            }
        }

        private static void SerializeDirectSerializableType(object obj, byte[] destination, ref int pos) {
            var type = obj.GetType();
            byte[] data;
            switch (obj) {
                case bool value:
                    data = BitConverter.GetBytes(value);
                    break;
                case char value:
                    data = BitConverter.GetBytes(value);
                    break;
                case byte value:
                    data = new[] {value};
                    break;
                case sbyte value:
                    data = new[] {(byte) value};
                    break;
                case double value:
                    data = BitConverter.GetBytes(value);
                    break;
                case short value:
                    data = BitConverter.GetBytes(value);
                    break;
                case int value:
                    data = BitConverter.GetBytes(value);
                    break;
                case long value:
                    data = BitConverter.GetBytes(value);
                    break;
                case float value:
                    data = BitConverter.GetBytes(value);
                    break;
                case ushort value:
                    data = BitConverter.GetBytes(value);
                    break;
                case uint value:
                    data = BitConverter.GetBytes(value);
                    break;
                case ulong value:
                    data = BitConverter.GetBytes(value);
                    break;
                default:
                    throw new InvalidSerializationException("Invalid type for serialization.");
            }

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(data);
            }

            data.CopyTo(destination, pos);
            pos += GetSerializedSize(type);
        }

        private static void
            SerializeFieldSerializableType(FieldInfo field, object obj, byte[] destination, ref int pos) {
            if (field.FieldType == typeof(string)) {
                var max_length = GetLengthOfFixedLengthAttribute(field);
                var data = Encoding.UTF8.GetBytes((string) obj);
                // Check length except '\0' of end
                if (data.Length - 1 > max_length) {
                    throw new InvalidSerializationException(
                        $"The length of string ({data.Length - 1}) exceeds max length indicated by attribute ({max_length}).");
                }

                for (var i = 0; i < max_length; ++i) {
                    destination[pos + i] = data.Length - 1 < i ? data[i] : (byte) '\0';
                }

                pos += max_length;
            }
            else if (field.FieldType.IsArray) {
                var length = GetLengthOfFixedLengthAttribute(field);
                var array = (object[]) obj;
                if (array.Length != length) {
                    throw new InvalidSerializationException(
                        $"The size of array ({array.Length}) does not match the size indicated by attribute ({length}).");
                }

                for (var i = 0; i < length; ++i) {
                    SerializeImpl(array[i], destination, ref pos);
                }
            }
            else {
                throw new InvalidSerializationException("Invalid type.");
            }
        }

        private static void SerializeComplexSerializableType(object obj, byte[] destination, ref int pos) {
            var type = obj.GetType();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                 BindingFlags.Instance)
            ) {
                if (IsFieldSerializableType(field.FieldType)) {
                    SerializeFieldSerializableType(field, obj, destination, ref pos);
                }
                else {
                    SerializeImpl(field.GetValue(obj), destination, ref pos);
                }
            }
        }

        private static void DeserializeImpl(Type type, byte[] source, ref int pos, out object obj) {
            if (IsDirectSerializableType(type)) {
                DeserializeDirectSerializableType(type, source, ref pos, out obj);
            }
            else if (IsFieldSerializableType(type)) {
                throw new InvalidSerializationException(
                    $"The type ({type}) is serializable only when it is declared as a field");
            }
            else if (IsComplexSerializableType(type)) {
                DeserializeComplexSerializableType(type, source, ref pos, out obj);
            }
            else {
                throw new InvalidSerializationException(
                    $"The type ({type}) is not serializable. Primitive types, fixed string and struct and class which is sequential are available.");
            }
        }

        private static void DeserializeDirectSerializableType(Type type, byte[] source, ref int pos,
            out object obj) {
            var data = source.ToArray();
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(data);
            }

            if (_bytesToDirectSerializableTypeConverterDict.ContainsKey(type)) {
                obj = _bytesToDirectSerializableTypeConverterDict[type](source, pos);
            }
            else {
                throw new InvalidSerializationException("Invalid type for serialization.");
            }

            pos += GetSerializedSize(type);
        }

        private static void DeserializeFieldSerializableType(FieldInfo field, byte[] source, ref int pos,
            out object obj) {
            if (field.FieldType == typeof(string)) {
                var max_length = GetLengthOfFixedLengthAttribute(field);
                var real_length = Array.IndexOf(source, '\0', pos, max_length);
                if (real_length < 0) {
                    real_length = max_length;
                }

                obj = Encoding.UTF8.GetString(source, pos, real_length);
                pos += max_length;
            }
            else if (field.FieldType.IsArray) {
                var length = GetLengthOfFixedLengthAttribute(field);
                obj = Activator.CreateInstance(field.FieldType, length);
                var array = (object[]) obj;
                var element_type = field.FieldType.GetGenericTypeDefinition().GetGenericArguments()[0];
                for (var i = 0; i < length; ++i) {
                    DeserializeImpl(element_type, source, ref pos, out array[i]);
                }
            }
            else {
                throw new InvalidSerializationException("Invalid type.");
            }
        }

        private static void
            DeserializeComplexSerializableType(Type type, byte[] source, ref int pos, out object obj) {
            obj = Activator.CreateInstance(type);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                 BindingFlags.Instance)
            ) {
                if (IsFieldSerializableType(field.FieldType)) {
                    DeserializeFieldSerializableType(field, source, ref pos, out obj);
                }
                else {
                    DeserializeImpl(field.FieldType, source, ref pos, out obj);
                }
            }
        }

        private static int GetLengthOfFixedLengthAttribute(FieldInfo field) {
            var fixed_length_attribute = field.GetCustomAttribute<FixedLengthAttribute>();
            if (fixed_length_attribute == null) {
                throw new InvalidSerializationException("There is no FixedLengthAttribute set to the field.");
            }

            return fixed_length_attribute.length;
        }

        private static bool IsDirectSerializableType(Type type) {
            return _directSerializableTypeSet.Contains(type);
        }

        private static bool IsFieldSerializableType(Type type) {
            return type == typeof(string) || type.IsArray;
        }

        private static bool IsComplexSerializableType(Type type) {
            return type.GetFields().Length > 0 && type.IsLayoutSequential &&
                   (type.Attributes & TypeAttributes.Serializable) != 0;
        }
    }
}