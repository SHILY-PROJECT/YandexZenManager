using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Models.AccountOrDonorModels;
using Yandex.Zen.Core.Services.PostingSecondWindService.Enums;
using Yandex.Zen.Core.Services.PostingSecondWindService.Models;

namespace Yandex.Zen.Core.Services.PostingSecondWindService
{
    public class PostingSecondWind
    {
        private static readonly object _locker = new object();
        [ThreadStatic] private static PostingSecondWindSettings _settings;

        private AccountOrDonorBaseModel Account { get => ProjectComponents.ResourceObject is null ? null : ProjectComponents.ResourceObject; }
        public static PostingSecondWindSettings Settings { get => _settings; }
        public static void SetSettings(PostingSecondWindSettings Settings) => _settings = Settings;

        public void Start()
        {
            if (Account is null)
                throw new Exception($"'{nameof(Account)}' - is null");
            if (Settings is null)
                throw new Exception($"'{nameof(Settings)}' - is null");


        }
    }
}
