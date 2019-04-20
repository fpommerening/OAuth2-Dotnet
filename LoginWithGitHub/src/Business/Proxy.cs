using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FP.OAuth.LoginWithGitHub.Business
{
    public class Proxy
    {
        public List<ProxyItem> Items { get; set; }

        [Required]
        public Uri Url { get; set; }
    }
}
