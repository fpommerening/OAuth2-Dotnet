using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FP.OAuth.ResourceServer.Business
{
    public class ValueRepository : IValueRepository
    {
        private readonly ConcurrentDictionary<string, ValueItem> _items = new ConcurrentDictionary<string, ValueItem>();

        public ValueItem Add(string userName, int value)
        {
            var item = new ValueItem
            {
                Timestamp = DateTime.Now,
                UserName = userName,
                Value = value
            };
            _items.AddOrUpdate(userName, item, (s, vi) => item);

            return item;
        }

        public List<ValueItem> GetValue()
        {
            return _items.Values.ToList();
        }
    }
}
