using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums.Extensions;
using Yandex.Zen.Core.Toolkit.Extensions;

namespace Yandex.Zen.Core.Toolkit.Macros
{
    public class HttpMacros
    {
        /// <summary>
        /// Получить случайную реферальную ссылку
        /// </summary>
        /// <returns></returns>
        public static string GetRandomReferenceLink()
        {
            return new List<string>
            {
                "https://www.youtube.com/",
                "https://www.figma.com/files/recent",
                "https://yandex.ru/news/",
                "https://www.petshop.ru/catalog/cats/syxkor/",
                "https://nutram.spb.ru/advantages_nutram.html",
                "https://www.wildberries.ru/catalog/tovary-dlya-zhivotnyh/dlya-koshek/korm-i-lakomstva",
                "https://zen.yandex.ru/",
                "https://vk.com/",
                "https://www.avito.ru/rossiya/avtomobili",
                "https://www.drive2.ru/",
                "https://www.wildberries.ru/",
                "https://mail.ru/",
                "https://my.mail.ru/",
                "https://www.rambler.ru/",
                "https://music.yandex.ru/",
                "https://ruv.hotmo.org/",
                "https://zaycev.net/",
                "https://catalog-aktsiy.ru/tovary-dlia-doma-katalog/kartiny-katalog.html",
                "https://www.spotify.com/",
                "https://twitter.com/",
                "https://www.dns-shop.ru/catalog/",
                "https://www.citilink.ru/catalog/",
                "https://www.ozon.ru/",
                "https://en.wikipedia.org/wiki/Main_Page",
                "https://aliexpress.ru/",
                "https://price.ru/",
                "https://www.e-katalog.ru/"
            }
            .GetLine(LineOptions.Random);
        }
    }
}
