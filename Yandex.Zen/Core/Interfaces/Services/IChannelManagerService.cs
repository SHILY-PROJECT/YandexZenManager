namespace Yandex.Zen.Core.Interfaces.Services
{
    public interface IChannelManagerService : IService
    {
        IAuthorizationModule Authorization { get; set; }
    }
}