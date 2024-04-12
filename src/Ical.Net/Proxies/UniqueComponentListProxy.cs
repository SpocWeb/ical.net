﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.Collections;

namespace Ical.Net.Proxies
{
    /// <summary> Typed Proxy/Filter for a List of generic Instances </summary>
    /// <typeparam name="TComponentType">the Type to filter for</typeparam>
    public class UniqueComponentListProxy<TComponentType> :
        CalendarObjectListProxy<TComponentType>,
        IUniqueComponentList<TComponentType>
        where TComponentType : class, IUniqueComponent
    {
        readonly Dictionary<string, TComponentType> _lookup;

        public UniqueComponentListProxy(IGroupedCollection<string, ICalendarObject> children) : base(children)
        {
            _lookup = new Dictionary<string, TComponentType>();
        }

        TComponentType Search(string uid)
        {
            if (_lookup.TryGetValue(uid, out var componentType))
            {
                return componentType;
            }

            var item = this.FirstOrDefault(c => string.Equals(c.Uid, uid, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                return default;
            }

            _lookup[uid] = item;
            return item;
        }

        public TComponentType this[string uid]
        {
            get => Search(uid);
            set
            {
                // Find the item matching the UID
                var item = Search(uid);

                if (item != null)
                {
                    Remove(item);
                }

                if (value != null)
                {
                    Add(value);
                }
            }
        }

        public void AddRange(IEnumerable<TComponentType> collection)
        {
            foreach (var element in collection)
            {
                Add(element);
            }
        }
    }
}