﻿/*--------------------------------------------------------*\
|                                                          |
|                          hprose                          |
|                                                          |
| Official WebSite: https://hprose.com                     |
|                                                          |
|  ClientContext.cs                                        |
|                                                          |
|  ClientContext class for C#.                             |
|                                                          |
|  LastModified: Feb 8, 2019                               |
|  Author: Ma Bingyao <andot@hprose.com>                   |
|                                                          |
\*________________________________________________________*/

using System;
using System.Dynamic;

namespace Hprose.RPC {
    public class ClientContext : Context {
        public Client Client { get; private set; }
        public string Uri { get; set; }
        public Type Type { get; set; }
        public dynamic RequestHeaders { get; } = new ExpandoObject();
        public dynamic ResponseHeaders { get; } = new ExpandoObject();
        public ClientContext(Client client, Type type, Settings settings = null) {
            Client = client;
            Uri = (client.Uris.Count > 0) ? client.Uris[0] : null;
            Type = settings?.Type;
            if (type != null && !type.IsAssignableFrom(Type)) Type = type;
            Copy(client.RequestHeaders, RequestHeaders);
            Copy(settings?.RequestHeaders, RequestHeaders);
            Copy(settings?.Context, Items);
        }
    }
}