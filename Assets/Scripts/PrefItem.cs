using System;
using UnityEngine;

namespace GrandTheftAuto {
    public class PrefItem<T> {
        private T _value;
        private bool loaded;

        public string Key { get; private set; }
        public object DefaultValue { get; private set; }
        public T Value {
            get { return loaded ? _value : (_value = (T)LoadValue(Key, DefaultValue)); }
            set { SetValue(Key, _value = value); }
        }

        public PrefItem(string key) {
            Key = key;
        }

        public PrefItem(string key, T defaultValue) {
            Key = key;
            DefaultValue = defaultValue;
        }

        private void SetValue(string key, object value) {
            if(value is int || value is Enum)
                PlayerPrefs.SetInt(key, (int)value);
            else if(value is float)
                PlayerPrefs.SetFloat(key, (float)value);
            else if(value is bool)
                PlayerPrefs.SetInt(key, (bool)value ? 1 : 0);
            else if(value is string)
                PlayerPrefs.SetString(key, (string)value);
        }

        private object LoadValue(string key, object defaultValue) {
            try {
                if(loaded)
                    return _value;

                var typeOfT = typeof(T);

                if(defaultValue == null) {
                    if(typeOfT == typeof(int) || typeOfT.IsEnum)
                        defaultValue = 0;
                    if(typeOfT == typeof(float))
                        defaultValue = 0f;
                    if(typeOfT == typeof(bool))
                        defaultValue = false;
                    if(typeOfT == typeof(string))
                        defaultValue = string.Empty;
                }

                if(typeOfT == typeof(int) || typeOfT.IsEnum)
                    return PlayerPrefs.GetInt(key, (int)defaultValue);
                if(typeOfT == typeof(float))
                    return PlayerPrefs.GetFloat(key, (float)defaultValue);
                if(typeOfT == typeof(bool))
                    return PlayerPrefs.GetInt(key, (bool)defaultValue ? 1 : 0) != 0;
                if(typeOfT == typeof(string))
                    return PlayerPrefs.GetString(key, (string)defaultValue);

                return null;
            }
            finally {
                loaded = true;
            }
        }

        public static implicit operator T(PrefItem<T> pref) {
            return pref.Value;
        }
    }
}