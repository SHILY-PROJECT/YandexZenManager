using System.IO;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces
{
    public interface IProfile : IResourceObject
    {
        FileInfo File { get; set; }

        void Save();
        void Load(bool createVariables = true);
        void Delete();
    }
}
