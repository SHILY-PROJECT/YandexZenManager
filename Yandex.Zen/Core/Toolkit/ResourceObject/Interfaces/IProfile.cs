using System.IO;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;

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
