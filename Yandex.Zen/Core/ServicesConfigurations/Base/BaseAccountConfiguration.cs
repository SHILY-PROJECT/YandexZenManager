using System;
using System.Collections.Generic;
using Global.ZennoExtensions;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Models;

namespace Yandex.Zen.Core.ServicesConfigurations.Base
{
    public abstract class BaseAccountConfiguration
    {
        public BaseAccountConfiguration(IDataManager manager)
        {
            Manager = manager;
            TableData = manager.TableData;
            Table = manager.TableData.Table;
        }

        protected IDataManager Manager { get; }
        protected IZennoTable Table { get; }
        protected TableData TableData { get; }

        public virtual void Configure()
        {
            lock (SyncObjects.InputSyncer)
            {
                ServiceConfigure();
                ConfigureAccount();
            }
        }

        protected virtual void ConfigureAccount()
        {
            for (int row = 0; row < Table.RowCount; row++)
            {
                try
                {
                    SetUpAccount(Table.GetRow(row));
                }
                catch (InvalidOperationException ex)
                {

                }
                catch
                {
                    throw;
                }
            }
        }

        protected abstract void ServiceConfigure();
        protected abstract void SetUpAccount(IEnumerable<string> columns);
    }
}
