// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

using System;
using System.Text;

namespace HtmlAgilityPack
{
    internal class EncodingFoundException : Exception
    {
        #region Fields

        #endregion

        #region Constructors

        internal EncodingFoundException(Encoding encoding)
        {
            Encoding = encoding;
        }

        #endregion

        #region Properties

        internal Encoding Encoding { get; }

        #endregion
    }
}