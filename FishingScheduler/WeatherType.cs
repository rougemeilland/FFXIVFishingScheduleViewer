using System;

namespace FishingScheduler
{
    [Flags]
    enum WeatherType
    {
        None = 0,
        雨 = 0x1,
        快晴 = 0x2,
        砂塵 = 0x4,
        灼熱波 = 0x8,
        吹雪 = 0x10,
        晴れ = 0x20,
        雪 = 0x40,
        曇り = 0x80,
        風 = 0x100,
        放電 = 0x200,
        暴雨 = 0x400,
        暴風 = 0x800,
        霧 = 0x1000,
        妖霧 = 0x2000,
        雷 = 0x4000,
        雷雨 = 0x8000,
        霊風 = 0x10000,
    }
}
