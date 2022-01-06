using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.TableTool.Enums;

namespace Yandex.Zen.Core.Models.TableHandler
{
    /// <summary>
    /// Модель для соханения ячейки.
    /// </summary>
    public class InstDataItem
    {
        public TableColumnEnum_obsolete.Inst_obsolete SetToColumn { get; set; }
        public string SetValue { get; set; }

        public InstDataItem(TableColumnEnum_obsolete.Inst_obsolete setToColumn, string setValue)
        {
            SetToColumn = setToColumn;
            SetValue = setValue;
        }
    }
}
