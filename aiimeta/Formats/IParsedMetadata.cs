using aiimeta.Reader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aiimeta.Formats
{
    /// <summary>Storage of parsed metadata.</summary>
    public interface IParsedMetadata
    {
        /// <summary>Positive prompt text.</summary>
        string? PositivePromptText { get; set; }

        /// <summary>Negative prompt text.</summary>
        string? NegativePromptText { get; set; }

        /// <summary>Gets all properties in this object.</summary>
        /// <remarks>Positive and negative prompt texts are not automatically included.</remarks>
        IEnumerable<KeyValuePair<string, string>> Properties { get; }

        /// <summary>Gets all property values for a specified key.</summary>
        /// <param name="key">Property key.</param>
        /// <returns>Series of property values.</returns>
        /// <remarks>
        /// If no property of the specified key is present, 
        /// this method returns an empty enumerable.
        /// </remarks>
        IEnumerable<string> Get(string key);

        /// <summary>Adds a single property.</summary>
        /// <param name="key">Property key.</param>
        /// <param name="value">Property value.</param>
        /// <returns>True if the specified property is added successfully. False, otherwise.</returns>
        /// <remarks>
        /// IParsedMetadata instance <i>may</i> drop identical property.
        /// Such instance returns false if a duplicate property is already included.
        /// </remarks>
        bool Add(string key, string value);

        /// <summary>Adds multiple property values for a single property key.</summary>
        /// <param name="key">Property key.</param>
        /// <param name="values">Series of property values.</param>
        /// <returns>True if all property values are added successfully. Flase, otherwise.</returns>
        bool AddRange(string key, IEnumerable<string> values);

        /// <summary>Adds multiple property values.</summary>
        /// <param name="pairs">Series of pairs of property key and property value.</param>
        /// <returns>True if all property values are added successfully. Flase, otherwise.</returns>
        bool AddRange(IEnumerable<KeyValuePair<string, string>> pairs);

        /// <summary>Removes a single property.</summary>
        /// <param name="key">Property key.</param>
        /// <param name="value">Property value.</param>
        /// <returns>True if the property is removed successfully. False otherwise.</returns>
        /// <remarks>
        /// If the specified <paramref name="key"/> is not included,
        /// or if the specified key is found but the specified <paramref name="value"/>
        /// is not associated with the key,
        /// this method does nothing and returns a false.
        /// </remarks>
        bool Remove(string key, string value);
        
        /// <summary>Removes all properties with the specified key.</summary>
        /// <param name="key">Property key.</param>
        /// <returns>True if one or more property is removed. False otherwise.</returns>
        bool RemoveAll(string key);
    }
}
