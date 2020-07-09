using System;
using System.Linq;

namespace FishingScheduler
{
    static class WeatherTypeExtensions
    {
        public static string GetText(this WeatherType weather)
        {
            return string.Join(
                "/",
                Enum.GetValues(typeof(WeatherType))
                    .Cast<WeatherType>()
                    .Where(w => (weather & w) != WeatherType.None)
                    .Select(w =>
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
                                return string.Format("???({0})", w);
                        }
                    }));
        }
    }
}
