﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;

namespace Yandex.Zen.Core.Models.TableHandler
{
    /// <summary>
    /// Модель для соханения ячейки.
    /// </summary>
    public class InstDataItem
    {
        public TableColumnEnum.Inst SetToColumn { get; set; }
        public string SetValue { get; set; }

        public InstDataItem(TableColumnEnum.Inst setToColumn, string setValue)
        {
            SetToColumn = setToColumn;
            SetValue = setValue;
        }
    }
}
