namespace Yandex.Zen.Core.Interfaces
{
    public interface IAuthorizationModule
    {
        bool IsSuccess { get; }
        void Authorization();
        void Authorization(out bool isSuccessful);
    }
}
