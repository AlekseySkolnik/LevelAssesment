namespace Reliability.Extensions;

public enum ClientIdentifierTypes
{
    DeviceId,
    ClientIp,
    ClientUUId,
    WorkflowId
}
public readonly record struct ClientIdentifierInfo(ClientIdentifierTypes Type, string Value);