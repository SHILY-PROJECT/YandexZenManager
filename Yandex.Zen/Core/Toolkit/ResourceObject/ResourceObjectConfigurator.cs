using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;

namespace Yandex.Zen.Core.Toolkit.ResourceObject
{
    public class ResourceObjectConfigurator
    {
        private readonly DataManager _manager;
        private readonly TemplateSettingsModel _templateSettings;
        private readonly CaptchaService _captchaService;
        private readonly SmsService _smsService;

        public ResourceObjectConfigurator(DataManager manager, TemplateSettingsModel templateSettings, CaptchaService captchaService, SmsService smsService)
        {
            _manager = manager;
            _templateSettings = templateSettings;
            _captchaService = captchaService;
            _smsService = smsService;
        }

        public IResourceObject Configure()
        {
            throw new NotImplementedException();
        }
    }
}
