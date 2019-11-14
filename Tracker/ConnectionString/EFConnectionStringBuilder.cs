using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DSM.Controller.Tracker.ConnectionString
{

    /// <summary>
    /// Class representing a connection string builder for the entity client provider
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "EntityConnectionStringBuilder follows the naming convention of DbConnectionStringBuilder.")]
    [SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers", Justification = "There is no applicable strongly-typed implementation of CopyTo.")]
    public sealed class EFConnectionStringBuilder : DbConnectionStringBuilder
    {
        internal const string NameParameterName = "name";

        internal const string MetadataParameterName = "metadata";

        internal const string ProviderParameterName = "provider";

        internal const string ProviderConnectionStringParameterName = "provider connection string";

        internal static readonly string[] ValidKeywords = new string[4]
        {
        "name",
        "metadata",
        "provider",
        "provider connection string"
        };

        private string _namedConnectionName;

        private string _providerName;

        private string _metadataLocations;

        private string _storeProviderConnectionString;

        /// <summary>Gets or sets the name of a section as defined in a configuration file.</summary>
        /// <returns>The name of a section in a configuration file.</returns>
        [DisplayName("Name")]
        [RefreshProperties(RefreshProperties.All)]
        public string Name
        {
            get => _namedConnectionName ?? "";
            set
            {
                _namedConnectionName = value;
                base["name"] = value;
            }
        }

        /// <summary>Gets or sets the name of the underlying .NET Framework data provider in the connection string.</summary>
        /// <returns>The invariant name of the underlying .NET Framework data provider.</returns>
        [DisplayName("Provider")]
        [RefreshProperties(RefreshProperties.All)]
        public string Provider
        {
            get => _providerName ?? "";
            set
            {
                _providerName = value;
                base["provider"] = value;
            }
        }

        /// <summary>Gets or sets the metadata locations in the connection string.</summary>
        /// <returns>Gets or sets the metadata locations in the connection string.</returns>
        [DisplayName("Metadata")]
        [RefreshProperties(RefreshProperties.All)]
        public string Metadata
        {
            get => _metadataLocations ?? "";
            set
            {
                _metadataLocations = value;
                base["metadata"] = value;
            }
        }

        /// <summary>Gets or sets the inner, provider-specific connection string.</summary>
        /// <returns>The inner, provider-specific connection string.</returns>
        [DisplayName("Provider Connection String")]
        [RefreshProperties(RefreshProperties.All)]
        public string ProviderConnectionString
        {
            get => _storeProviderConnectionString ?? "";
            set
            {
                _storeProviderConnectionString = value;
                base["provider connection string"] = value;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the
        /// <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// has a fixed size.
        /// </summary>
        /// <returns>
        /// Returns true in every case, because the
        /// <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// supplies a fixed-size collection of keyword/value pairs.
        /// </returns>
        public override bool IsFixedSize => true;

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection" /> that contains the keys in the
        /// <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// .
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.ICollection" /> that contains the keys in the
        /// <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// .
        /// </returns>
        public override ICollection Keys => new ReadOnlyCollection<string>(ValidKeywords);

        /// <summary>Gets or sets the value associated with the specified key. In C#, this property is the indexer.</summary>
        /// <returns>The value associated with the specified key. </returns>
        /// <param name="keyword">The key of the item to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException"> keyword  is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Tried to add a key that does not exist in the available keys.</exception>
        /// <exception cref="T:System.FormatException">Invalid value in the connection string (specifically, a Boolean or numeric value was expected but not supplied).</exception>
        public override object this[string keyword]
        {
            get
            {
                if (string.Compare(keyword, "metadata", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Metadata;
                }
                if (string.Compare(keyword, "provider connection string", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return ProviderConnectionString;
                }
                if (string.Compare(keyword, "name", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Name;
                }
                if (string.Compare(keyword, "provider", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Provider;
                }
                throw new ArgumentException("");
            }
            set
            {
                //Check.NotNull(keyword, "keyword");
                if (value == null)
                {
                    Remove(keyword);
                    return;
                }
                string text = value as string;
                if (text == null)
                {
                    throw new ArgumentException("");
                }
                if (string.Compare(keyword, "metadata", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Metadata = text;
                    return;
                }
                if (string.Compare(keyword, "provider connection string", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ProviderConnectionString = text;
                    return;
                }
                if (string.Compare(keyword, "name", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Name = text;
                    return;
                }
                if (string.Compare(keyword, "provider", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Provider = text;
                    return;
                }
                throw new ArgumentException("");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" /> class.
        /// </summary>
        public EFConnectionStringBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" /> class using the supplied connection string.
        /// </summary>
        /// <param name="connectionString">A provider-specific connection string to the underlying data source.</param>
        public EFConnectionStringBuilder(string connectionString)
        {
            base.ConnectionString = connectionString;
        }

        /// <summary>
        /// Clears the contents of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" /> instance.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _namedConnectionName = null;
            _providerName = null;
            _metadataLocations = null;
            _storeProviderConnectionString = null;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" /> contains a specific key.
        /// </summary>
        /// <returns>
        /// Returns true if the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" /> contains an element that has the specified key; otherwise, false.
        /// </returns>
        /// <param name="keyword">
        /// The key to locate in the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />.
        /// </param>
        public override bool ContainsKey(string keyword)
        {
            //Check.NotNull(keyword, "keyword");
            string[] validKeywords = ValidKeywords;
            foreach (string text in validKeywords)
            {
                if (text.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieves a value corresponding to the supplied key from this
        /// <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// .
        /// </summary>
        /// <returns>Returns true if  keyword  was found in the connection string; otherwise, false.</returns>
        /// <param name="keyword">The key of the item to retrieve.</param>
        /// <param name="value">The value corresponding to  keyword. </param>
        /// <exception cref="T:System.ArgumentNullException"> keyword  contains a null value (Nothing in Visual Basic).</exception>
        public override bool TryGetValue(string keyword, out object value)
        {
            //Check.NotNull(keyword, "keyword");
            if (ContainsKey(keyword))
            {
                value = this[keyword];
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Removes the entry with the specified key from the
        /// <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// instance.
        /// </summary>
        /// <returns>Returns true if the key existed in the connection string and was removed; false if the key did not exist.</returns>
        /// <param name="keyword">
        /// The key of the keyword/value pair to be removed from the connection string in this
        /// <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// .
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> keyword  is null (Nothing in Visual Basic)</exception>
        public override bool Remove(string keyword)
        {
            if (string.Compare(keyword, "metadata", StringComparison.OrdinalIgnoreCase) == 0)
            {
                _metadataLocations = null;
            }
            else if (string.Compare(keyword, "provider connection string", StringComparison.OrdinalIgnoreCase) == 0)
            {
                _storeProviderConnectionString = null;
            }
            else if (string.Compare(keyword, "name", StringComparison.OrdinalIgnoreCase) == 0)
            {
                _namedConnectionName = null;
            }
            else if (string.Compare(keyword, "provider", StringComparison.OrdinalIgnoreCase) == 0)
            {
                _providerName = null;
            }
            return base.Remove(keyword);
        }
    }

}
