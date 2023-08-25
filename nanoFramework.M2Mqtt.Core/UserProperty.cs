using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// A user property is a key value string pair, v5.0 only
    /// </summary>
    public class UserProperty
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <param name="value">Value of the property</param>
        public UserProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; }
    }
}
