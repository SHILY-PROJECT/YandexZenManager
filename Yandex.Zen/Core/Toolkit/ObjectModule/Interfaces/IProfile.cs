using System.IO;
using Yandex.Zen.Core.Toolkit.ObjectModule.Models;

namespace Yandex.Zen.Core.Toolkit.ObjectModule.Interfaces
{
    public interface IProfile : IObject
    {
        /// <summary>
        /// Файл профиля.
        /// </summary>
        FileInfo File { get; }

        /// <summary>
        /// Имя файла профиля.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Сохранить профиль.
        /// </summary>
        void Save();

        /// <summary>
        /// Загрузить профиль.
        /// </summary>
        /// <param name="createVariables"></param>
        void Load(bool createVariables = true);

        /// <summary>
        /// Удаление профиля.
        /// </summary>
        void Delete();
    }
}
