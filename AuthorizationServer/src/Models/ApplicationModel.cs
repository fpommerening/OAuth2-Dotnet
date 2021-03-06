﻿using System;

namespace FP.OAuth.AuthorizationServer.Models
{
    public class ApplicationModel
    {
        private string _clientId;

        public ApplicationModel()
        {
            ClientSecret = Guid.NewGuid().ToString("N");
            UserPasssword = $"PW-{Guid.NewGuid().ToString("N").Substring(5, 15)}#";
        }

        public string ClientSecret { get; set; }

        public string UserName { get; set; }

        public string UserPasssword { get; set; }

        public string ClientId
        {
            get => _clientId;
            set
            {
                _clientId = value;
                UserName = $"user-{_clientId}";
            } 
        }
    }
}
