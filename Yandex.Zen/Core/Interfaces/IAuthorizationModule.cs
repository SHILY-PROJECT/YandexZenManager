namespace Yandex.Zen.Core.Interfaces
{
    public interface IAuthorizationModule
    {
        bool IsSuccesss { get; }
        void Authorization();
        void Authorization(out bool isSuccessful);
    }
}
