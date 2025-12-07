using System.Text;

namespace RadioDispatch.Records
{
    /// <summary>
    /// Result container for record lookups so UI, voice, and radio systems can render consistent output.
    /// </summary>
    public struct RecordLookupResult
    {
        public bool Found;
        public string Query;
        public RecordEntry Entry;

        public static RecordLookupResult MissingQuery()
        {
            return new RecordLookupResult { Found = false, Query = string.Empty, Entry = null };
        }

        public static RecordLookupResult NotFound(string query)
        {
            return new RecordLookupResult { Found = false, Query = query, Entry = null };
        }

        public static RecordLookupResult Found(string query, RecordEntry entry)
        {
            return new RecordLookupResult { Found = true, Query = query, Entry = entry };
        }

        /// <summary>
        /// Builds a dispatcher-friendly response string detailing the requested record or a not-found notice.
        /// </summary>
        public string BuildResponse()
        {
            if (!Found || Entry == null)
            {
                return string.IsNullOrWhiteSpace(Query)
                    ? "No search term provided."
                    : $"No record found for {Query}.";
            }

            var builder = new StringBuilder();
            builder.Append(Entry.BuildSummary());
            if (!string.IsNullOrWhiteSpace(Entry.Metadata))
            {
                builder.Append($" Metadata: {Entry.Metadata}");
            }

            return builder.ToString();
        }
    }
}
