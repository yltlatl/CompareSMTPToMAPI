using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace CompareSMTPToMAPI
{
    internal class SmtpHeader
    {
        #region Constructors

        public SmtpHeader(string header, string newline)
        {
            StandardFields = new List<Tuple<string, string>>();
            CustomFields = new List<Tuple<string, string>>();
            var headerFields = ParseHeader(header, newline);
            CustomFields = AssignStandardFields(headerFields);
        }

        #endregion

        #region Properties
        public List<Tuple<string, string>> StandardFields { get; private set; } 

        public List<Tuple<string, string>> CustomFields { get; private set; }

        public readonly string[] Rfc822Fields =
        {
            "Return-path:",
            "Received:",
            "From:",
            "To:",
            "CC:",
            "In-Reply-To:",
            "Sender:",
            "Reply-To:",
            "BCC:",
            "Message-Id:",
            "Subject:",
            "Date:"
        };



        #endregion

        #region Methods

        private List<Tuple<string, string>> ParseHeader(string header, string newline)
        {
            string[] nlArray = {newline};
            var explodedHeader = header.Split(nlArray, StringSplitOptions.RemoveEmptyEntries);
            var headerFields = new List<string>();
            foreach (var t in explodedHeader)
            {
                var str = new StringBuilder();
                str.Append(t);
                var initialCharacter = t[0];
                if (char.IsWhiteSpace(initialCharacter))
                {
                    if (headerFields.Count > 0)
                    {
                        str.Insert(0, headerFields.Last());
                        headerFields.RemoveAt(headerFields.Count - 1);
                    }
                    headerFields.Add(str.ToString());
                }
                else
                {
                    headerFields.Add(str.ToString());
                }
            }
            var retVal = new List<Tuple<string, string>>();
            var components = headerFields.Select(headerField => headerField.Split(new[] {": "}, 2, StringSplitOptions.None)).ToList();
            foreach (var component in components)
            {
                if (component.Length == 1)
                {
                    var field = Tuple.Create(string.Format("{0}:", component[0]), string.Empty);
                    retVal.Add(field);
                }
                else if (component.Length == 2)
                {
                    var field = Tuple.Create(string.Format("{0}:", component[0]), component[1]);
                    retVal.Add(field);
                }
                else
                {
                    throw new InvalidDataException("Failed to parse field into appropriate parts.");
                }
            }
            return retVal;
        }

        private List<Tuple<string, string>> AssignStandardFields(List<Tuple<string, string>> retVal)
        {
            foreach (var field in Rfc822Fields)
            {
                var lField = field.ToLowerInvariant();
                var matches = retVal.Where(t => t.Item1.ToLowerInvariant().Equals(lField)).ToList();
                if (matches.Count == 0) continue;
                foreach (var match in matches)
                {
                    StandardFields.Add(match);
                }
                retVal.RemoveAll(t => t.Item1.ToLowerInvariant().Equals(lField));
            }
            return retVal;
        }

        #endregion
    }
}
