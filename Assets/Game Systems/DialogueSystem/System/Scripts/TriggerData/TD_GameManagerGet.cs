using System;
using UnityEngine;

namespace DialogueSystem.TriggerData
{
    public enum GameManagerDataType
    {
        String,
        Int,
        Float,
        Bool
    }

    [Serializable]
    public class TD_GameManagerGetData
    {
        public string FieldName;
        public GameManagerDataType DataType;
    }

    public class TD_GameManagerGet : TriggerDataBase<TD_GameManagerGetData>
    {
        public override object GetOutputValue(string portName, DialogueSequence sequence = null)
        {
            if (TypedData == null || string.IsNullOrEmpty(TypedData.FieldName))
                return null;

            if (portName != "Value")
                return null;

            object value = GameManager.Get(TypedData.FieldName);
            if (value == null)
                return null;

            // Convert to the requested type
            try
            {
                switch (TypedData.DataType)
                {
                    case GameManagerDataType.String:
                        return value.ToString();
                    case GameManagerDataType.Int:
                        return Convert.ToInt32(value);
                    case GameManagerDataType.Float:
                        return Convert.ToSingle(value);
                    case GameManagerDataType.Bool:
                        return Convert.ToBoolean(value);
                    default:
                        return value;
                }
            }
            catch
            {
                Debug.LogWarning($"TD_GameManagerGet: Failed to convert value to {TypedData.DataType}");
                return null;
            }
        }

        public override string[] GetOutputPortNames()
        {
            return new[] { "Value" };
        }

        public override Type GetOutputPortType(string portName)
        {
            if (portName == "Value")
            {
                // If TypedData is available, use its DataType
                if (TypedData != null)
                {
                    switch (TypedData.DataType)
                    {
                        case GameManagerDataType.String:
                            return typeof(string);
                        case GameManagerDataType.Int:
                            return typeof(int);
                        case GameManagerDataType.Float:
                            return typeof(float);
                        case GameManagerDataType.Bool:
                            return typeof(bool);
                    }
                }
                // Default to object if data not set yet
                return typeof(object);
            }
            return null;
        }
    }
}

