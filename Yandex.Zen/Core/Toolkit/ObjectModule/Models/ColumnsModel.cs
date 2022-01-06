using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Toolkit.ObjectModule.Models
{
    public class ColumnsModel
    {
        int Login { get; set; }
        int Password { get; set; }
        int AnswerQuestion { get; set; }
        int AccountNumberPhone { get; set; }
        int ChannelNumberPhone { get; set; }
        int Proxy { get; set; }
        int ChannelUrl { get; set; }
        int IndexationAndBanStatus { get; set; }
        int ProfileAndIPCountry { get; set; }
        int MessageFromSetting { get; set; }

        public ColumnsModel()
        {

        }
    }
}
