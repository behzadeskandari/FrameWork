using Gateway.Framework.Shared.Constants;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Gateway.Framework.Gateway.Transforms;

/// <summary>
/// YARP transform that injects correlation ID into proxied requests.
/// </summary>
public class CorrelationIdTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context) { }

    public void ValidateCluster(TransformClusterValidationContext context) { }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            var correlationId = transformContext.HttpContext.Items["CorrelationId"]?.ToString()
                ?? Guid.NewGuid().ToString("D");

            transformContext.ProxyRequest.Headers.Remove(GatewayConstants.CorrelationIdHeader);
            transformContext.ProxyRequest.Headers.Add(GatewayConstants.CorrelationIdHeader, correlationId);

            return ValueTask.CompletedTask;
        });
    }
}
