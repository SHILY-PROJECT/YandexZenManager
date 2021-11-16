using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Enums.Logger;
using Yandex.Zen.Core.ServicesCommonComponents;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Tools
{
    public class Logger
    {
        private static readonly object _locker = new object();
        private static readonly string _logAccountFileName = @"_logger\account.log";
        private static readonly string _backupAccountDataFileName = @"_logger\backup_account_data.txt";

        [ThreadStatic] public static FileInfo GeneralLog;
        [ThreadStatic] public static string LogResourceText;

        private static DirectoryInfo ResourceDirectory { get => ServiceComponents.ResourceDirectory; }
        private static Instance Instance { get => ServiceComponents.instance; }
        private static 

        /// <summary>
        /// Получение лога режима.
        /// </summary>
        /// <returns></returns>
        public static void SetGeneralLog()
        {
            new Dictionary<ProgramModeEnum, FileInfo>
            {
                { ProgramModeEnum.WalkingProfile, new FileInfo($@"{zenno.Directory}\_logger\walking_profile.log") },
                { ProgramModeEnum.WalkingOnZen, new FileInfo($@"{zenno.Directory}\_logger\walking_on_zen.log") },
                { ProgramModeEnum.InstanceAccountManagement, new FileInfo($@"{zenno.Directory}\_logger\instance_account_management.log") },
                { ProgramModeEnum.YandexAccountRegistration, new FileInfo($@"{zenno.Directory}\_logger\yandex_account_registration.log") },
                { ProgramModeEnum.ZenChannelCreationAndDesign, new FileInfo($@"{zenno.Directory}\_logger\zen_channel_creation_and_design.log") },
                { ProgramModeEnum.ZenArticlePublication, new FileInfo($@"{zenno.Directory}\_logger\zen_posting.log") },
            }
            .TryGetValue(Program.ProgramMode, out GeneralLog);
        }

        /// <summary>
        /// Получение текущий даты.
        /// </summary>
        /// <param name="dateTimeType">Формат даты.</param>
        /// <returns></returns>
        public static string GetDateTime(DateTimeFormat dateTimeType)
        {
            new Dictionary<DateTimeFormat, string>
            {
                { DateTimeFormat.yyyyMMdd, $"{DateTime.Now:yyyy-MM-dd}" },
                { DateTimeFormat.yyyyMMddTabHHmmss, $"{DateTime.Now:yyyy-MM-dd\tHH-mm-ss}" },
                { DateTimeFormat.yyyyMMddSpaceHHmmss, $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}" },
                { DateTimeFormat.yyyyMMddThreeSpaceHHmmss, $"{DateTime.Now:yyyy-MM-dd   HH-mm-ss}" },
                { DateTimeFormat.UnixtimeTotalSeconds, $"{(int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds}" }
            }
            .TryGetValue(dateTimeType, out string result);

            return result;
        }

        /// <summary>
        /// Получение даты в формате Unixtime.
        /// </summary>
        /// <returns></returns>
        public static int GetUnixtime() =>
            (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        /// <summary>
        /// Получение лога аккаунта/донора.
        /// </summary>
        /// <param name="fullNameFolderAccountOrDonor">Путь к папке с аккаунтом/донором.</param>
        /// <returns></returns>
        public static FileInfo GetLogAccountFileInfo(string fullNameFolderAccountOrDonor) =>
            new FileInfo(Path.Combine(fullNameFolderAccountOrDonor, _logAccountFileName));

        /// <summary>
        /// Получение лога аккаунта/донора.
        /// </summary>
        /// <returns></returns>
        public static FileInfo GetAccountFileLogInfo() =>
            new FileInfo(Path.Combine(ResourceDirectory.FullName, _logAccountFileName));

        /// <summary>
        /// Получение информации о бэкап файле аккаунта.
        /// </summary>
        /// <param name="CreateLoggerDirectoryIfNotExist"></param>
        /// <returns></returns>
        public static FileInfo GetInfoBackupAccount(bool CreateLoggerDirectoryIfNotExist = true)
        {
            var fileInfo = new FileInfo(Path.Combine(ResourceDirectory.FullName, _backupAccountDataFileName));

            if (CreateLoggerDirectoryIfNotExist && !fileInfo.Exists) fileInfo.Directory.Create();

            return fileInfo;
        }

        /// <summary>
        /// Бэкап данных аккаунта.
        /// </summary>
        /// <param name="dataList"></param>
        public static void MakeBackupData(List<string> dataList, bool addDateTimeInBeginningOfLine)
        {
            var fileBackup = GetInfoBackupAccount(true);

            if (addDateTimeInBeginningOfLine)
            {
                var datetime = $"[{DateTime.Now:yyyy-MM-dd   HH-mm-ss}]   ";

                for (int i = 0; i < dataList.Count; i++)
                    if (!string.IsNullOrWhiteSpace(dataList[i]))
                        dataList[i] = $"{datetime}{dataList[i]}";
            }

            File.AppendAllLines(fileBackup.FullName, dataList, Encoding.UTF8);

            Write($"[Backup: {fileBackup.FullName}]\tСделан бэкап данных", LoggerType.Info, true, false, true);
        }

        /// <summary>
        /// Получить лог аккаунта.
        /// </summary>
        /// <param name="backupFilter"></param>
        /// <returns></returns>
        public static List<string> GetAccountBackup(BackupFilter backupFilter)
        {
            // Получение информации о бэкап файле
            var backupAccount = GetInfoBackupAccount(true);

            // Проверка наличия бэкап файла
            if (!backupAccount.Exists) return new List<string>();

            switch (backupFilter)
            {
                case BackupFilter.AllLinesWithoutFilter:
                    return File.ReadAllLines(backupAccount.FullName, Encoding.UTF8).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Анализ данных и сохранение результата в отдельную папку в папке логгера аккаунта.
        /// </summary>
        /// <param name="saveDomText"></param>
        /// <param name="takeScreenshot"></param>
        /// <param name="otherInfoList"></param>
        public static void ErrorAnalysis(bool saveDomText, bool saveSourceText, bool takeScreenshot, List<string> otherInfoList = null)
            => ErrorAnalysis(ResourceDirectory, saveDomText, saveSourceText, takeScreenshot, otherInfoList);

        /// <summary>
        /// Анализ данных и сохранение результата в отдельную папку в папке логгера аккаунта.
        /// </summary>
        /// <param name="saveDomText"></param>
        /// <param name="takeScreenshot"></param>
        /// <param name="otherInfoList"></param>
        public static void ErrorAnalysis(DirectoryInfo resourceDirectory, bool saveDomText, bool saveSourceText, bool takeScreenshot, List<string> otherInfoList = null)
        {
            var datetime = $"{DateTime.Now:yyyy-MM-dd   HH-mm-ss}";
            var errorFolder = new DirectoryInfo(Path.Combine(resourceDirectory.FullName, "_logger", $"error - {datetime}"));
            var titleActiveTab = $"{new string(Path.GetInvalidFileNameChars())}{new string(Path.GetInvalidPathChars())}".Aggregate(Instance.ActiveTab.Title, (t, c) => t.Replace(c.ToString(), ""));

            try
            {
                // Создать папку для результата анализа ошибки
                if (!errorFolder.Exists) errorFolder.Create();

                // Сохранение DomText
                if (saveDomText)
                    File.WriteAllText(Path.Combine(errorFolder.FullName, $"[{datetime}] - [DomText] - {titleActiveTab}.html"), Instance.ActiveTab.DomText, Encoding.UTF8);

                // Сохранение SourceText
                if (saveSourceText)
                    File.WriteAllText(Path.Combine(errorFolder.FullName, $"[{datetime}] - [SourceText] - {titleActiveTab}.html"), Instance.ActiveTab.GetSourceText("utf-8"), Encoding.UTF8);

                // Сохранение дополнительной информации об ошибке
                if (otherInfoList != null && otherInfoList.Count != 0)
                    File.WriteAllLines(Path.Combine(errorFolder.FullName, $"[{datetime}] - other info error.txt"), otherInfoList, Encoding.UTF8);

                // Сохранение скриншота страницы
                if (takeScreenshot)
                {
                    using (var ms = new MemoryStream())
                    {
                        var btScreenshot = Convert.FromBase64String(Instance.ActiveTab.GetPagePreview());
                        ms.Write(btScreenshot, 0, btScreenshot.Length);

                        using (var bm = new Bitmap(ms))
                        {
                            bm.Save(Path.Combine(errorFolder.FullName, $"[{datetime}] - {titleActiveTab}.png"), System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }

                if (saveDomText || saveSourceText || takeScreenshot || otherInfoList != null && otherInfoList.Count != 0)
                    Write($"[{errorFolder.FullName}]\tАнализ ошибки успешно выполнен", LoggerType.Info, true, true, true, LogColor.Violet);
            }
            catch (Exception ex)
            {
                Write($"[{errorFolder.FullName}]\t[Exception message: {ex.Message}]\tНе вышло выполнить анализ ошибки...", LoggerType.Warning, true, true, true, LogColor.Yellow);
            }
        }

        /// <summary>
        /// Получение паттерна фильтрации лога.
        /// </summary>
        /// <param name="logPattern"></param>
        /// <returns></returns>
        public static Regex GetRegexPatternForAccountLog(LogFilter logPattern)
        {
            new Dictionary<LogFilter, Regex>
            {
                { LogFilter.WalkingUnixtime, new Regex(@"(?<=\[Walking\ zen\ unixtime:\ ).*?(?=])")},
                { LogFilter.ArticlePublicationUnixtime, new Regex(@"(?<=\[Publication\ unixtime:\ ).*?(?=])") },
                { LogFilter.SkipAccount, new Regex(@"(?<=\[)Skip\ account.*?(?=])") },
                { LogFilter.SkipBindingPhone, new Regex(@"\[Skip\ binding\ phone].*?") },
                { LogFilter.AllLines, null }
            }
            .TryGetValue(logPattern, out Regex pattern);

            return pattern;
        }

        /// <summary>
        /// Получение данных лога по паттерну.
        /// </summary>
        /// <param name="logFilter"></param>
        /// <returns></returns>
        public static List<string> GetAccountLog(LogFilter logFilter)
        {
            // Получение паттерна для фильтрации списка
            var pattern = GetRegexPatternForAccountLog(logFilter);

            // Получаем информацию о файле лога аккаунта
            var fileLog = new FileInfo(Path.Combine(ResourceDirectory.FullName, _logAccountFileName));

            // Проверка файла лога аккаунта на существование
            if (!fileLog.Exists) return new List<string>();

            // Считываем файл лога аккаунта в список
            var logList = File.ReadAllLines(fileLog.FullName, Encoding.UTF8).ToList();

            // Проверка на пустоту
            if (logList.Count == 0) return new List<string>();

            // Возвращаем весь лог, если нужно
            if (logFilter == LogFilter.AllLines) return logList;

            // Возвращаем результат
            return logList.Where(x => pattern.IsMatch(x)).ToList();
        }

        /// <summary>
        /// Записать в лог донора, основной, отправить в лог zp.
        /// </summary>
        /// <param name="textToLog"></param>
        public static void Write(string textToLog, LoggerType loggerType, bool writeToResourceLog = true, bool writeToGeneralLog = true, bool sendToZennoPosterLog = true, LogColor logColor = LogColor.Default) =>
            Write(ResourceDirectory, textToLog, loggerType, writeToResourceLog, writeToGeneralLog, sendToZennoPosterLog, logColor);

        /// <summary>
        /// Записать в лог донора, основной, отправить в лог zp.
        /// </summary>
        /// <param name="textToLog"></param>
        public static void Write(DirectoryInfo resourceDirectory, string textToLog, LoggerType loggerType, bool writeToResourceLog = true, bool writeToGeneralLog = true, bool sendToZennoPosterLog = true, LogColor logColor = LogColor.Default)
        {
            var dateTime = $"{DateTime.Now:yyyy-MM-dd   HH-mm-ss}";

            // Запись в лог аккаунта
            if (writeToResourceLog && resourceDirectory != null)
            {
                new Dictionary<ProgramModeEnum, string>
                {
                    { ProgramModeEnum.WalkingProfile, $"[{Program.ProgramMode}]                  " },
                    { ProgramModeEnum.WalkingOnZen, $"[{Program.ProgramMode}]                    " },
                    { ProgramModeEnum.InstanceAccountManagement, $"[{Program.ProgramMode}]       " },
                    { ProgramModeEnum.YandexAccountRegistration, $"[{Program.ProgramMode}]       " },
                    { ProgramModeEnum.ZenChannelCreationAndDesign, $"[{Program.ProgramMode}]     " },
                    { ProgramModeEnum.ZenArticlePublication, $"[{Program.ProgramMode}]           " },
                }
                .TryGetValue(Program.ProgramMode, out string modeForAccountLog);

                WriteToResourceLog(resourceDirectory, $"{modeForAccountLog}{LogResourceText}{textToLog}", loggerType, dateTime);
            }

            // Запись в основной лог режима
            if (writeToGeneralLog)
                WriteToGeneralLog($"{LogResourceText}{textToLog}", loggerType, dateTime);
            
            // Отправка сообщения в zp/pm
            if (zenno != null)
                zenno.SendToLog($"[{ProgramModeEnum.WalkingProfile}]\t{LogResourceText}{textToLog}", (LogType)Enum.Parse(typeof(LogType), ((int)loggerType).ToString()), sendToZennoPosterLog, logColor);
        }

        /// <summary>
        /// Записать в общий лог (IZennoPosterProjectModel).
        /// </summary>
        /// <param name="textToLog"></param>
        /// <param name="zenno"></param>
        /// <param name="loggerType"></param>
        /// <param name="dateTime"></param>
        private static void WriteToGeneralLog(string textToLog, LoggerType loggerType, string dateTime = null) =>
            WriteGeneralLogToFile(textToLog, loggerType, dateTime);

        /// <summary>
        /// Записать общий лог в файл.
        /// </summary>
        /// <param name="textToLog"></param>
        /// <param name="zenno"></param>
        /// <param name="loggerType"></param>
        /// <param name="loggerWriteSettings"></param>
        /// <param name="dateTime"></param>
        private static void WriteGeneralLogToFile(string textToLog, LoggerType loggerType, string dateTime = null)
        {
            if (GeneralLog == null)
                throw new Exception("Logger.DirectoryGeneralLog  -  Директория проекта не установлена");

            if (!GeneralLog.Directory.Exists) GeneralLog.Directory.Create();

            dateTime = !string.IsNullOrWhiteSpace(dateTime) ? dateTime : $"{DateTime.Now:yyyy-MM-dd   HH-mm-ss}";
            textToLog = GetTextLogByType(dateTime, loggerType, textToLog);

            lock (_locker)
                File.AppendAllLines(GeneralLog.FullName, new[] { $"{textToLog}" }, Encoding.UTF8);
        }

        /// <summary>
        /// Записать в лог аккаунта/донора (настройка типов лога).
        /// </summary>
        /// <param name="textToLog"></param>
        /// <param name="folderAccountLog"></param>
        /// <param name="loggerWriteSettings"></param>
        /// <param name="loggerType"></param>
        /// <param name="dateTime"></param>
        public static void WriteToResourceLog(DirectoryInfo folderAccountLog, string textToLog, LoggerType loggerType, string dateTime = null) =>
            WriteResourceLogToFile(textToLog, folderAccountLog.FullName, loggerType, dateTime);

        /// <summary>
        /// Сделать запись в лог аккаунта.
        /// </summary>
        /// <param name="textToLog"></param>
        /// <param name="pathFolderAccountLog"></param>
        /// <param name="loggerType"></param>
        /// <param name="datetime"></param>
        private static void WriteResourceLogToFile(string textToLog, string pathFolderAccountLog, LoggerType loggerType, string datetime = null)
        {
            var pathAccountLog = new FileInfo(Path.Combine(pathFolderAccountLog, _logAccountFileName));

            if (!pathAccountLog.Directory.Exists) pathAccountLog.Directory.Create();

            datetime = !string.IsNullOrWhiteSpace(datetime) ? datetime : $"{DateTime.Now:yyyy-MM-dd   HH-mm-ss}";
            textToLog = GetTextLogByType(datetime, loggerType, textToLog);

            File.AppendAllLines(pathAccountLog.FullName, new[] { $"{textToLog}" }, Encoding.UTF8);
        }

        /// <summary>
        /// Сформировать строку для записи в лог.
        /// </summary>
        /// <param name="textToLog"></param>
        /// <param name="datetime"></param>
        /// <param name="loggerType"></param>
        /// <param name="loggerWriteSettings"></param>
        /// <returns></returns>
        private static string GetTextLogByType(string datetime, LoggerType loggerType, string textToLog)
        {
            new Dictionary<LoggerType, string>
            {
                { LoggerType.Info, $"[{datetime}]      |INFO|      {textToLog}"},
                { LoggerType.Warning, $"[{datetime}]      |WARNING|   {textToLog}" },
                { LoggerType.Error, $"[{datetime}]      |ERROR|     {textToLog}" }
            }
            .TryGetValue(loggerType, out string logData);

            return logData ?? string.Empty;
        }

    }
}