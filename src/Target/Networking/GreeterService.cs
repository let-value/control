using System.Threading.Tasks;
using Grpc.Core;
using GrpcGreeter;
using Microsoft.Extensions.Logging;

namespace Target.Networking
{
    public class GreeterService : Greeter.GreeterBase
    {
        public GreeterService(ILogger<GreeterService> logger)
        {
        }

        public override Task<HelloReply> SayHello(
            HelloRequest request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            var clientCertificate = httpContext.Connection.ClientCertificate;

            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name + " from " + clientCertificate.Issuer
            });
        }
    }
}
