using System;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer
{
    static class WeatherTypeExtensions
    {
        public static WeatherType TryParseAsWeather(this string text)
        {
            switch (text)
            {
                case "雨":
                    return WeatherType.雨;
                case "快晴":
                    return WeatherType.快晴;
                case "砂塵":
                    return WeatherType.砂塵;
                case "灼熱波":
                    return WeatherType.灼熱波;
                case "吹雪":
                    return WeatherType.吹雪;
                case "晴れ":
                    return WeatherType.晴れ;
                case "雪":
                    return WeatherType.雪;
                case "曇り":
                    return WeatherType.曇り;
                case "風":
                    return WeatherType.風;
                case "放電":
                    return WeatherType.放電;
                case "暴雨":
                    return WeatherType.暴雨;
                case "暴風":
                    return WeatherType.暴風;
                case "霧":
                    return WeatherType.霧;
                case "妖霧":
                    return WeatherType.妖霧;
                case "雷":
                    return WeatherType.雷;
                case "雷雨":
                    return WeatherType.雷雨;
                case "霊風":
                    return WeatherType.霊風;
                default:
                    return WeatherType.None;
            }
        }

        public static string GetText(this WeatherType weather)
        {
            return GetText(weather, id => Translate.Instance[new TranslationTextId(TranslationCategory.Weather, id)]);
        }

        public static IEnumerable<string> CheckTranslation(this WeatherType weather)
        {
            return Translate.Instance.CheckTranslation(new TranslationTextId(TranslationCategory.Weather, GetTranslationId(weather)));
        }

        private static string GetText(WeatherType weather, Func<string, string> translater)
        {
            return string.Join(
                "/",
                Enum.GetValues(typeof(WeatherType))
                    .Cast<WeatherType>()
                    .Where(w => (weather & w) != WeatherType.None)
                    .Select(w => Translate.Instance[new TranslationTextId(TranslationCategory.Weather, GetTranslationId(w))]));
        }

        private static string GetTranslationId(WeatherType w)
        {
            switch (w)
            {
                case WeatherType.雨:
                    return "雨";
                case WeatherType.快晴:
                    return "快晴";
                case WeatherType.砂塵:
                    return "砂塵";
                case WeatherType.灼熱波:
                    return "灼熱波";
                case WeatherType.吹雪:
                    return "吹雪";
                case WeatherType.晴れ:
                    return "晴れ";
                case WeatherType.雪:
                    return "雪";
                case WeatherType.曇り:
                    return "曇り";
                case WeatherType.風:
                    return "風";
                case WeatherType.放電:
                    return "放電";
                case WeatherType.暴雨:
                    return "暴雨";
                case WeatherType.暴風:
                    return "暴風";
                case WeatherType.霧:
                    return "霧";
                case WeatherType.妖霧:
                    return "妖霧";
                case WeatherType.雷:
                    return "雷";
                case WeatherType.雷雨:
                    return "雷雨";
                case WeatherType.霊風:
                    return "霊風";
                default:
                    return w.ToString();
            }
        }
    }
}
