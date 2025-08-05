using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests;

internal class ConnectionStrings
{
	public const string PlainConnectionString = "BridgeHost=localhost;ServerName=srv1;Username=test;Password=test;SslMode=None";
	public const string SslConnectionString = "BridgeHost=localhost;ServerName=srv1;Username=test;Password=test";
}