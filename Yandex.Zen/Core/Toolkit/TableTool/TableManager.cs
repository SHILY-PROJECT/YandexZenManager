﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.TableTool.Enums;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Toolkit.TableTool
{
    public class TableManager
    {
        private static TableModel MainTable { get => DataManager.Data.MainTable; }
        private static TableModel ModeTable { get => DataManager.Data.ModeTable; }

        public static void SaveToTable<T>(int column, string value, bool saveToMainTable, bool saveTo)
        {
            

        }


        private static void TrySaveToTable(string searchByValue, string setValue, int columnForSetValue)
        {
            for (int row = 0; row < MainTable.Table.RowCount; row++)
            {
                if (MainTable.Table.GetRow(row).Any(x => x.Equals(searchByValue, StringComparison.OrdinalIgnoreCase)))
                {
                    MainTable.Table.SetCell(columnForSetValue, row, setValue);

                    
                    break;
                }
            }
        }

        private static int RowView(int row) => row + 2;

    }
}