using System;
using System.Collections.Generic;

namespace FP.OAuth.LoginWithGitHub.Business
{
    public interface IProxyRepository
    {
        ProxyItem Add(string url);
        IEnumerable<ProxyItem> GetItems();
        Guid LocalId { get; }
    }
}