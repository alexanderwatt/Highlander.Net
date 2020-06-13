using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Highlander.GrpcService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

//syntax = "proto3"
//import "google/protobuf/duration.proto";  
//import "google/protobuf/timestamp.proto";

//option csharp_namespace = "Highlander.GrpcService";

//package comms;

//// The Highlander communications service definition.
//service ITransferV341
//{
//// Sends a greeting
//rpc SayHello (HelloRequest) returns (HelloReply);
//}

//message V341Source
//{
//Guid SourceNodeId = 1;
//}

//message V341TransportItem
//{
//Guid ItemId = 1;

//}

//message V341CloseSession
//{
//int32 Reserved = 1;
//}

//message V341BeginSession
//{
//Guid Token = 1;
//V341ItemKind ItemKind = 2;
//bool Transient = 3;
//string ItemName = 4;
//string AppProps = 5;
//string DataType = 6;
//string AppScope = 7;
//google.protobuf.Timestamp Created = 8;
//google.protobuf.Timestamp Expires = 9;
//string SysProps = 10;
//string NetScope = 11;
//bytes YData = 12;
//bytes YSign = 13;
//int64 SourceUSN = 14;
//}