#nullable enable
using System.Collections.Generic;

namespace GolemUI
{
    public class SentryContext
    {
        public delegate void MemberChangedEventHandler();
        public event MemberChangedEventHandler? MemberChanged;

        public readonly Dictionary<string, string> Items = new Dictionary<string, string>();

        public void AddItem(string key, string value)
        {
            if (Items.ContainsKey(key))
            {
                if (Items[key] == value)
                    return;
                Items[key] = value;
            }
            else
            {
                Items.Add(key, value);
            }
            MemberChanged?.Invoke();
        }

    }
}
