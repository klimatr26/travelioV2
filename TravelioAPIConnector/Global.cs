using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace TravelioAPIConnector;

public static class Global
{
    public const bool IsREST = false;

    public static Binding GetBinding(string uri)
    {
        return uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)
            ? new BasicHttpsBinding() { MaxReceivedMessageSize = 10_485_760 }
            : new BasicHttpBinding() { MaxReceivedMessageSize = 10_485_760 };
    }
}
