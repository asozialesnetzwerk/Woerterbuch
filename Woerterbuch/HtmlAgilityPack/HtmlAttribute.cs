// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

#region

using System;
using System.Diagnostics;

#endregion

// ReSharper disable InconsistentNaming

namespace HtmlAgilityPack
{
    /// <summary>
    ///     Represents an HTML attribute.
    /// </summary>
    [DebuggerDisplay("Name: {OriginalName}, Value: {Value}")]
    public class HtmlAttribute : IComparable
    {
        #region Constructors

        internal HtmlAttribute(HtmlDocument ownerdocument)
        {
            _ownerdocument = ownerdocument;
        }

        #endregion

        #region IComparable Members

        /// <summary>
        ///     Compares the current instance with another attribute. Comparison is based on attributes' name.
        /// </summary>
        /// <param name="obj">An attribute to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the names comparison.</returns>
        public int CompareTo(object obj)
        {
            var att = obj as HtmlAttribute;
            if (att == null) throw new ArgumentException("obj");
            return Name.CompareTo(att.Name);
        }

        #endregion

        #region Private Methods

        private string GetRelativeXpath()
        {
            if (OwnerNode == null)
                return Name;

            var i = 1;
            foreach (var node in OwnerNode.Attributes)
            {
                if (node.Name != Name) continue;

                if (node == this)
                    break;

                i++;
            }

            return "@" + Name + "[" + i + "]";
        }

        #endregion

        #region Fields

        internal int _lineposition;
        internal string _name;
        internal int _namelength;
        internal int _namestartindex;
        internal HtmlDocument _ownerdocument; // attribute can exists without a node
        internal HtmlNode _ownernode;
        internal int _streamposition;
        internal string _value;
        internal int _valuelength;
        internal int _valuestartindex;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the line number of this attribute in the document.
        /// </summary>
        public int Line { get; internal set; }

        /// <summary>
        ///     Gets the column number of this attribute in the document.
        /// </summary>
        public int LinePosition => _lineposition;

        /// <summary>
        ///     Gets the qualified name of the attribute.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null) _name = _ownerdocument.Text.Substring(_namestartindex, _namelength);
                return _name.ToLower();
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _name = value;
                if (_ownernode != null) _ownernode.SetChanged();
            }
        }

        /// <summary>
        ///     Name of attribute with original case
        /// </summary>
        public string OriginalName => _name;

        /// <summary>
        ///     Gets the HTML document to which this attribute belongs.
        /// </summary>
        public HtmlDocument OwnerDocument => _ownerdocument;

        /// <summary>
        ///     Gets the HTML node to which this attribute belongs.
        /// </summary>
        public HtmlNode OwnerNode => _ownernode;

        /// <summary>
        ///     Specifies what type of quote the data should be wrapped in
        /// </summary>
        public AttributeValueQuote QuoteType { get; set; } = AttributeValueQuote.DoubleQuote;

        /// <summary>
        ///     Gets the stream position of this attribute in the document, relative to the start of the document.
        /// </summary>
        public int StreamPosition => _streamposition;

        /// <summary>
        ///     Gets or sets the value of the attribute.
        /// </summary>
        public string Value
        {
            get
            {
                if (_value == null) _value = _ownerdocument.Text.Substring(_valuestartindex, _valuelength);
                return _value;
            }
            set
            {
                _value = value;
                if (_ownernode != null) _ownernode.SetChanged();
            }
        }

        internal string XmlName => HtmlDocument.GetXmlName(Name);

        internal string XmlValue => Value;

        /// <summary>
        ///     Gets a valid XPath string that points to this Attribute
        /// </summary>
        public string XPath
        {
            get
            {
                var basePath = OwnerNode == null ? "/" : OwnerNode.XPath + "/";
                return basePath + GetRelativeXpath();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates a duplicate of this attribute.
        /// </summary>
        /// <returns>The cloned attribute.</returns>
        public HtmlAttribute Clone()
        {
            var att = new HtmlAttribute(_ownerdocument);
            att.Name = Name;
            att.Value = Value;
            return att;
        }

        /// <summary>
        ///     Removes this attribute from it's parents collection
        /// </summary>
        public void Remove()
        {
            _ownernode.Attributes.Remove(this);
        }

        #endregion
    }

    /// <summary>
    ///     An Enum representing different types of Quotes used for surrounding attribute values
    /// </summary>
    public enum AttributeValueQuote
    {
        /// <summary>
        ///     A single quote mark '
        /// </summary>
        SingleQuote,

        /// <summary>
        ///     A double quote mark "
        /// </summary>
        DoubleQuote
    }
}