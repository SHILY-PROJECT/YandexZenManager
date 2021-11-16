using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Enums.InstanceAccountManagement
{
	/// <summary>
	/// Режим обработки аккаунтов.
	/// </summary>
	public enum AccountProcessingModeEnum
	{
		FirstLoginOnly = 0,
		AllLoginsInOrder = 1
	}
}
