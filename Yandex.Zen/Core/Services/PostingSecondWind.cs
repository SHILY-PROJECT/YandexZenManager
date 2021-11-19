﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Models.AccountOrDonorModels;

namespace Yandex.Zen.Core.Services
{
    public class PostingSecondWind
    {
        private static readonly object _locker = new object();

        private AccountOrDonorBaseModel Account { get => ProjectComponents.ResourceObject is null ? null : ProjectComponents.ResourceObject; }

        public PostingSecondWind()
        {
            
        }

        public void Start()
        {
            if (Account is null)
                throw new Exception($"'{nameof(Account)}' - равен null");
        }
    }
}
