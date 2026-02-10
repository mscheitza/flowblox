namespace FlowBlox.Core.Attributes
{
    /// <summary>
    /// Marks this property as dependent on one or more other properties.
    /// <para>When one of the specified source properties raises a PropertyChanged event,
    /// the runtime should automatically raise PropertyChanged for this property as well.</para>
    /// This allows UI bindings and other listeners to stay updated when the value of this property is indirectly affected by others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DependsOnPropertyAttribute : Attribute
    {
        /// <summary>
        /// List of property names this property depends on.
        /// </summary>
        public string[] MemberNames { get; set; }

        /// <summary>
        /// A single property name this property depends on (alternative to MemberNames).
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// Returns all dependent property names (combines MemberName and MemberNames).
        /// </summary>
        public IEnumerable<string> GetMemberNames()
        {
            var list = new List<string>();

            if (!string.IsNullOrWhiteSpace(MemberName))
                list.Add(MemberName);

            if (MemberNames != null)
                list.AddRange(MemberNames);

            return list;
        }

        public DependsOnPropertyAttribute()
        {
            MemberNames = Array.Empty<string>();
        }
    }
}