//-----------------------------------------------------------------------------
// FILE:        ArgCollection.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright (c) 2005-2015 by Jeffrey Lill.  All rights reserved.
// DESCRIPTION: Implements simple way to encode name/value pairs into a string.

using System;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace LillTek.Common
{
    /// <summary>
    /// Implements simple way to encode name/value pairs into a string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class encodes one or more name/value pairs into a string
    /// as [name0]=[value0];name[1]=value[1],...
    /// </para>
    /// <para>
    /// The name lookup is case insensitive and only one instance of
    /// a given name is allowed.
    /// </para>
    /// <para>
    /// The class implements the <see cref="IEnumerable" /> interface
    /// to enumerate over the property keys.
    /// </para>
    /// </remarks>
    public class ArgCollection : IEnumerable<string>, IEnumerable
    {
        //---------------------------------------------------------------------
        // Static members

        private const int Magic = 0x771A;

        /// <summary>
        /// Parses and returns an <see cref="ArgCollection" /> using the default assignment
        /// "=" and separator ";" charcters.
        /// </summary>
        /// <param name="input">The input string (or <c>null</c>).</param>
        /// <returns>The parsed collection.</returns>
        /// <remarks>
        /// If <paramref name="input" /> is <c>null</c> then an empty collection will be created.
        /// </remarks>
        public static ArgCollection Parse(string input)
        {
            return new ArgCollection(input);
        }

        /// <summary>
        /// Parses and returns a <see cref="ArgCollection" /> using an alternate
        /// assignment character.
        /// </summary>
        /// <param name="input">The input string (or <c>null</c>).</param>
        /// <param name="assignChar">The alternate assignment character.</param>
        /// <param name="separatorChar">The separator character.</param>
        /// <returns>The parsed collection.</returns>
        /// <remarks>
        /// If <paramref name="input" /> is <c>null</c> then an empty collection will be created.
        /// </remarks>
        public static ArgCollection Parse(string input, char assignChar, char separatorChar)
        {
            return new ArgCollection(input, assignChar, separatorChar);
        }

        /// <summary>
        /// Casts the string parameter passed into to a <see cref="ArgCollection" /> instance
        /// by parsing it with the default "=" assignment and ";" separator characters.
        /// </summary>
        /// <param name="input">The string to be converted.</param>
        /// <returns>The parameter string instance.</returns>
        /// <remarks>
        /// If <paramref name="input" /> is <c>null</c> then an empty collection will be created.
        /// </remarks>
        public static implicit operator ArgCollection(string input)
        {
            return Parse(input);
        }

        /// <summary>
        /// Deserializes a <see cref="ArgCollection" /> instance from a byte array previously
        /// generated by a call to <see cref="ToBytes" />.
        /// </summary>
        /// <param name="input">The input array.</param>
        /// <returns>The deserialized <see cref="ArgCollection" /> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="input" /> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Thrown if the input data is not valid.</exception>
        public static ArgCollection FromBytes(byte[] input)
        {
            using (var ms = new EnhancedMemoryStream(input))
            {

                return new ArgCollection(ms);
            }
        }

        /// <summary>
        /// Serializes a <see cref="ArgCollection" /> instance to a byte array.
        /// </summary>
        /// <param name="input">The input <see cref="ArgCollection" />.</param>
        /// <returns>The serialized byte array.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="input" /> is <c>null</c>.</exception>
        public static byte[] ToBytes(ArgCollection input)
        {
            using (var ms = new EnhancedMemoryStream())
            {

                input.Write(ms);
                return ms.ToArray();
            }
        }

        //---------------------------------------------------------------------
        // Instance members

        private IDictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private char assignChar;     // The assignment character
        private char sepChar;        // Separator character

        /// <summary>
        /// Constructs an empty collection using the default assignment character (=).
        /// </summary>
        public ArgCollection()
        {
            this.assignChar = '=';
            this.sepChar    = ';';
        }

        /// <summary>
        /// Constructs a specialized collection with unusable assignment and separator characters.
        /// </summary>
        /// <param name="type">Describes the type of collection to be created.</param>
        /// <remarks>
        /// <para>
        /// This constructor is typically used by passing <see cref="ArgCollectionType.Unconstrained"/> for 
        /// situations where the collection will not be parsed or rendered into a string form and the 
        /// application wishes there to be no contraints on the name/value strings stored in the collection.
        /// </para>
        /// <para>
        /// Passing <see cref="ArgCollectionType.Normal" /> will create a collection that uses the
        /// default assignment (<b>=</b>) and separator (<b>;</b>) characters. This is equivalent to
        /// using the default constructor <see cref="ArgCollection()" />.
        /// </para>
        /// </remarks>
        public ArgCollection(ArgCollectionType type)
        {
            if (type == ArgCollectionType.Unconstrained)
            {

                this.assignChar = (char)0;
                this.sepChar = (char)0;
            }
            else
            {

                this.assignChar = '=';
                this.sepChar    = ';';
            }
        }

        /// <summary>
        /// Constructs an empty collection using an alternate assignment character.
        /// </summary>
        /// <param name="assignChar">The alternate character.</param>
        /// <param name="separatorChar">The separator character.</param>
        public ArgCollection(char assignChar, char separatorChar)
        {
            if ((assignChar != 0 || sepChar != 0) && assignChar == separatorChar)
                throw new ArgumentException("Assignment and separaror characters cannot be the same.");

            this.assignChar = assignChar;
            this.sepChar    = separatorChar;
        }

        /// <summary>
        /// Constructs a collection by parsing the input using the default assignment
        /// character (=).
        /// </summary>
        /// <param name="input">The formatted input string (or <c>null</c>).</param>
        /// <remarks>
        /// If <paramref name="input" /> is <c>null</c> then an empty collection will be created.
        /// </remarks>
        public ArgCollection(string input)
            : this(input, '=', ';')
        {
        }

        /// <summary>
        /// Constructs a collection by parsing the input using an alternate assignment
        /// character.
        /// </summary>
        /// <param name="input">The formatted input string (or <c>null</c>).</param>
        /// <param name="assignChar">The alternate assignment character.</param>
        /// <param name="separatorChar">The separator character.</param>
        /// <remarks>
        /// If <paramref name="input" /> is <c>null</c> then an empty collection will be created.
        /// </remarks>
        public ArgCollection(string input, char assignChar, char separatorChar)
        {
            this.assignChar = assignChar;
            this.sepChar    = separatorChar;

            Load(input);
        }

        /// <summary>
        /// Constructs an instance by deseralizing from a stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        public ArgCollection(EnhancedStream input)
        {
            this.assignChar = '=';
            this.sepChar    = (char)0;

            if (input == null)
                throw new ArgumentException("input");

            if (input.ReadInt16() != Magic)
                throw new FormatException("ArgCollection: Invalid magic number.");

            var count = input.ReadInt16();

            for (int i = 0; i < count; i++)
            {
                var name = input.ReadString16();
                var value = input.ReadString32();

                this[name] = value;
            }
        }

        /// <summary>
        /// Indicates whether the collection is <b>read-only</b>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when an attempt is made to make a read-only collection writable.</exception>
        /// <remarks>
        /// Setting this to <c>true</c> will make the collection read-only.  Note that
        /// once a collection is set to read only, it cannot be reset to be writable.
        /// </remarks>
        public bool IsReadOnly
        {
            get { return dictionary is ReadOnlyDictionary<string, string>; }

            set
            {
                if (value)
                {
                    if (!IsReadOnly)
                        dictionary = dictionary.ToReadOnly();
                }
                else if (IsReadOnly)
                    throw new InvalidOperationException("ArgCollection cannot be made writable after it has been made read-only/");
            }
        }

        /// <summary>
        /// Serializes the set values to a stream.
        /// </summary>
        /// <param name="output">The output stream.</param>
        public void Write(EnhancedStream output)
        {
            output.WriteInt16(Magic);
            output.WriteInt16(this.Count);

            foreach (var key in this)
            {
                output.WriteString16(key);
                output.WriteString32(this[key]);
            }
        }

        /// <summary>
        /// Clears the collection and then loads properties from a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        public void Load(string input)
        {

            int         p, pEnd;
            string      name;
            string      value;

            if (assignChar == 0 && sepChar == 0)
                throw new InvalidOperationException("ArgCollection cannot parse a string collection because is was created using the ArgCollection(Param.Stub) constructor.");

            Clear();
            if (input == null)
                return;

            p = 0;
            while (true)
            {
                // Skip over separator characters and whitespace

                while (p < input.Length && (input[p] == sepChar || Char.IsWhiteSpace(input[p])))
                    p++;

                // Parse the key name

                pEnd = input.IndexOf(assignChar, p);
                if (pEnd == -1)
                    break;

                name = input.Substring(p, pEnd - p).Trim();
                p = pEnd + 1;

                // Parse the key value

                pEnd = input.IndexOf(sepChar, p);
                if (pEnd == -1)
                    value = input.Substring(p).Trim();
                else
                    value = input.Substring(p, pEnd - p).Trim();

                Set(name, value);

                if (pEnd == -1)
                    break;
                else
                    p = pEnd + 1;
            }
        }

        /// <summary>
        /// Removes all items from the instance.
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();
        }

        /// <summary>
        /// Returns a shallow clone of this instance.
        /// </summary>
        public ArgCollection Clone()
        {
            ArgCollection clone = new ArgCollection(assignChar, sepChar);

            foreach (KeyValuePair<string, string> entry in dictionary)
                clone.dictionary.Add(entry.Key, entry.Value);

            return clone;
        }

        /// <summary>
        /// Returns the number of items in the instance.
        /// </summary>
        public int Count
        {
            get { return dictionary.Count; }
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, string value)
        {
            if (assignChar != 0 && name.IndexOf(assignChar) != -1)
                throw new ArgumentException(string.Format("Name cannot contain a '{0}'.", assignChar));

            if (value == null)
            {
                if (dictionary.ContainsKey(name))
                    dictionary.Remove(name);

                return;
            }

            if (sepChar != 0 && value.IndexOf(sepChar) != -1)
                throw new ArgumentException("Value cannot contain the separator character.");

            this[name] = value;
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, bool value)
        {
            Set(name, Serialize.ToString(value));
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, TimeSpan value)
        {
            Set(name, Serialize.ToString(value));
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, DateTime value)
        {
            Set(name, Serialize.ToString(value));
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, int value)
        {
            Set(name, Serialize.ToString(value));
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, long value)
        {
            Set(name, Serialize.ToString(value));
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, double value)
        {
            Set(name, Serialize.ToString(value));
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, IPAddress value)
        {
            Set(name, Serialize.ToString(value));
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, NetworkBinding value)
        {
            Set(name, Serialize.ToString(value));
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, Guid value)
        {
            Set(name, Serialize.ToString(value));
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, Uri value)
        {
            Set(name, Serialize.ToString(value));
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, object value)
        {

            Set(name, value != null ? value.ToString() : (string)null);
        }

        /// <summary>
        /// Sets a name/value pair in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, byte[] value)
        {
            if (value == null)
            {
                Set(name, (string)null);
                return;
            }

            Set(name, Convert.ToBase64String(value));
        }

        /// <summary>
        /// Accesses the named value, returning <c>null</c> if the value is not present.
        /// </summary>
        public string this[string name]
        {
            get
            {
                string value;

                if (dictionary.TryGetValue(name, out value))
                    return value;

                return null;
            }

            set
            {
                if (assignChar != 0 && name.IndexOf(assignChar) != -1)
                    throw new ArgumentException(string.Format("Name cannot contain a '{0}'.", assignChar));

                if (sepChar != 0 && value.IndexOf(sepChar) != -1)
                    throw new ArgumentException("Value cannot contain the separator character.");

                dictionary[name] = value;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the collection contains a specific key.
        /// </summary>
        /// <param name="name">The key.</param>
        /// <returns><c>true</c> if the key exists.</returns>
        public bool ContainsKey(string name)
        {
            return dictionary.ContainsKey(name);
        }

        /// <summary>
        /// Looks for a named value in the collection and returns
        /// if it is present.
        /// </summary>
        /// <param name="name">The key.</param>
        /// <param name="value">Returns as the value if found.</param>
        /// <returns><c>true</c> if the named value is found.</returns>
        public bool TryGetValue(string name, out string value)
        {
            return dictionary.TryGetValue(name, out value);
        }

        /// <summary>
        /// Returns the value of the named string if it's present
        /// null otherwise.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The value or <c>null</c>.</returns>
        public string Get(string name)
        {
            return this[name];
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public string Get(string name, string def)
        {
            var value = this[name];

            return value != null ? value : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public bool Get(string name, bool def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, def) : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public TimeSpan Get(string name, TimeSpan def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, def) : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public DateTime Get(string name, DateTime def)
        {
            var value = this[name];

            return value != null ? Serialize.Parse(value, def) : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        /// <remarks>
        /// <note>
        /// <para>
        /// This method supports "K", "M", and "G" suffixes
        /// where "K" multiples the value parsed by 1024, "M multiplies
        /// the value parsed by 1048576, and "G" multiples the value
        /// parsed by 1073741824.  The "T" suffix is not supported
        /// by this method because it exceeds the capacity of a
        /// 32-bit integer.
        /// </para>
        /// <para>
        /// The following constant values are also supported:
        /// </para>
        /// <list type="table">
        ///     <item><term><b>short.min</b></term><description>-32768</description></item>
        ///     <item><term><b>short.max</b></term><description>32767</description></item>
        ///     <item><term><b>ushort.max</b></term><description>65533</description></item>
        ///     <item><term><b>int.min</b></term><description>-2147483648</description></item>
        ///     <item><term><b>int.max</b></term><description>2147483647</description></item>
        /// </list>
        /// </note>
        /// </remarks>
        public int Get(string name, int def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, def) : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        /// <remarks>
        /// <note>
        /// <para>
        /// This method supports "K", "M", "G", and "T" suffixes
        /// where "K" multiples the value parsed by 1024, "M multiplies
        /// the value parsed by 1048576, "G" multiples the value
        /// parsed by 1073741824, and "T" multiplies the value by 
        /// 1099511627776.
        /// </para>
        /// <para>
        /// The following constant values are also supported:
        /// </para>
        /// <list type="table">
        ///     <item><term><b>short.min</b></term><description>-32768</description></item>
        ///     <item><term><b>short.max</b></term><description>32767</description></item>
        ///     <item><term><b>ushort.max</b></term><description>65533</description></item>
        ///     <item><term><b>int.min</b></term><description>-2147483648</description></item>
        ///     <item><term><b>int.max</b></term><description>2147483647</description></item>
        ///     <item><term><b>uint.max</b></term><description>4294967295</description></item>
        ///     <item><term><b>long.max</b></term><description>2^63 - 1</description></item>
        /// </list>
        /// </note>
        /// </remarks>
        public long Get(string name, long def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, def) : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        /// <remarks>
        /// <note>
        /// <para>
        /// This method supports "K", "M", "G", and "T" suffixes
        /// where "K" multiples the value parsed by 1024, "M multiplies
        /// the value parsed by 1048576, "G" multiples the value
        /// parsed by 1073741824, and "T" multiplies the value by 
        /// 1099511627776.
        /// </para>
        /// <para>
        /// The following constant values are also supported:
        /// </para>
        /// <list type="table">
        ///     <item><term><b>short.min</b></term><description>-32768</description></item>
        ///     <item><term><b>short.max</b></term><description>32767</description></item>
        ///     <item><term><b>ushort.max</b></term><description>65533</description></item>
        ///     <item><term><b>int.min</b></term><description>-2147483648</description></item>
        ///     <item><term><b>int.max</b></term><description>2147483647</description></item>
        ///     <item><term><b>uint.max</b></term><description>4294967295</description></item>
        ///     <item><term><b>long.max</b></term><description>2^63 - 1</description></item>
        /// </list>
        /// </note>
        /// </remarks>
        public double Get(string name, double def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, def) : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public IPAddress Get(string name, IPAddress def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, def) : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public NetworkBinding Get(string name, NetworkBinding def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, def) : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public Guid Get(string name, Guid def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, def) : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public Uri Get(string name, Uri def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, def) : def;
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public byte[] Get(string name, byte[] def)
        {
            var value = this[name];

            try
            {
                return value != null ? Convert.FromBase64String(value) : def;
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        /// Returns the named value if it exists in the parameter string otherwise
        /// returns a specified default value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public System.Type Get(string name, System.Type def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, def) : def;
        }

        /// <summary>
        /// Parses the named enumeration argument where the value is case insenstive.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="enumType">The enumeration type.</param>
        /// <param name="def">The default enumeration value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public object Get(string name, System.Type enumType, object def)
        {
            var value = this[name];

            return value != null ? Config.ParseValue(value, enumType, def) : def;
        }

        /// <summary>
        /// Parses the named enumeration argument where the value is case insenstive.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="def">The default enumeration value.</param>
        /// <returns>The named value from the parameter string if it is present, the default value otherwise.</returns>
        public TEnum Get<TEnum>(string name, object def)
        {
            var value = this[name];

            return Config.ParseValue<TEnum>(value, def);
        }

        /// <summary>
        /// Removes an argument from the list if it exists.
        /// </summary>
        /// <param name="name">The argument name.</param>
        public void Remove(string name)
        {
            if (dictionary.ContainsKey(name))
                dictionary.Remove(name);
        }

        /// <summary>
        /// Returns a formatted set of parameters.
        /// </summary>
        /// <returns>The formatted string.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var entry in dictionary)
                sb.AppendFormat("{0}{1}{2}{3}", entry.Key, assignChar, entry.Value, sepChar);

            return sb.ToString();
        }

        /// <summary>
        /// Returns an enumerator for the set of property keys.
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator for the set of property keys.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }
    }
}