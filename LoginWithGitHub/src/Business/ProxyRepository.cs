using System;
using System.Collections.Generic;

namespace FP.OAuth.LoginWithGitHub.Business
{
    public class ProxyRepository : IProxyRepository
    {
        public ProxyRepository()
        {
            LocalId = Guid.NewGuid();
        }

        private List<ProxyItem> _items = new List<ProxyItem>();

        public ProxyItem Add(string url)
        {
            var item = new ProxyItem
            {
                Url = url,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
            _items.Add(item);

            return item;
        }

        public IEnumerable<ProxyItem> GetItems()
        {
            return _items;
        }

        public Guid LocalId { get; }
    }
}
