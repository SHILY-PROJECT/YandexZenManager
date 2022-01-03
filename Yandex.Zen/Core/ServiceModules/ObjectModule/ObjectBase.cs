using System;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.ServiceModules.ObjectModule.Models;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.Macros;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.ServiceModules.ObjectModule
{
    public class ObjectBase : ObjectBaseModel
    {
        private static readonly object _locker = new object();

        private DataManager DataManager { get; set; }

        public ObjectBase(DataManager manager)
        {
            DataManager = manager;
        }

        /// <summary>
        /// Сохранение профиля.
        /// </summary>
        public void SaveProfile() => ProfileData.SaveProfile();

        /// <summary>
        /// Загрузка профиля.
        /// </summary>
        /// <param name="createVariables"></param>
        public void Load(bool createVariables = true) => ProfileData.Load(createVariables);

        /// <summary>
        /// Генерация нового пароля (автоматически вставляется в свойство 'Password')
        /// </summary>
        public void GenerateNewPassword()
        {
            Password = TextMacros.GenerateString(12, 16, "abcd");
            Logger.Write($"[{nameof(Password)}:{Password}]\tСгенерирован новый пароль (на момент записи в лог, этот пароль не установлен)", LoggerType.Info, true, false, false);
        }

        /// <summary>
        /// Установка ресурса.
        /// </summary>
        public void SetObject(ProgramModeEnum mode)
        {
            var tb = DataManager.Table;

            if (tb.Instance.RowCount == 0)
                throw new Exception($"Таблица пуста: {tb.FileName}");

            lock (_locker)
            {
                for (int row = 0; row < tb.Instance.RowCount; row++)
                {
                    try
                    {
                        switch (mode)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(ex.Message, LoggerType.Warning, this.Directory.Exists, true, true, LogColor.Yellow);
                    }
                }
            }

            return;

            throw new Exception($"Отсутствуют свободные/подходящие аккаунты: {tb.FileName}");
        }
    }
}
