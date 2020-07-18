﻿using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace FishingScheduler
{
    class Fish
        : IGameDataObject
    {
        private class FishingRequirement
        {
            public FishingRequirement()
            {
                TranslatedRequirement = null;
                RequiredFishName = null;
            }
            public string TranslatedRequirement { get; set; }
            public string RequiredFishName { get; set; }
        }

        private static Regex _直釣り省略パターン = new Regex(@"^(\((?<requires>(要|天候不問|時間帯不問)[^\)]*)\)\s*)?(\((?<hooking>(スト|プレ))\))?$", RegexOptions.Compiled);
        private static Regex _直釣りパターン = new Regex(@"^(\((?<requires>(要|天候不問|時間帯不問)[^\)]*)\)\s*)?(?<bait>[^\(\)⇒]+)⇒(\((?<hooking>(スト|プレ))\))?$", RegexOptions.Compiled);
        private static Regex _泳がせ釣りパターン = new Regex(@"^(\((?<requires>(要|天候不問|時間帯不問)[^\)]*)\)\s*)?(?<bait1>[^\(\)⇒]+)⇒(\((?<hooking1>(スト|プレ))\))?(?<bait2>[^\(\)⇒]+)⇒(\((?<hooking2>(スト|プレ))\))?$", RegexOptions.Compiled);
        private static Regex _2段泳がせ釣りパターン = new Regex(@"^(\((?<requires>(要|天候不問|時間帯不問)[^\)]*)\)\s*)?(?<bait1>[^\(\)⇒]+)⇒(\((?<hooking1>(スト|プレ))\))?(?<bait2>[^\(\)⇒]+)⇒(\((?<hooking2>(スト|プレ))\))?(?<bait3>[^\(\)⇒]+)⇒(\((?<hooking3>(スト|プレ))\))?$", RegexOptions.Compiled);
        private static Regex _3段泳がせ釣りパターン = new Regex(@"^(\((?<requires>(要|天候不問|時間帯不問)[^\)]*)\)\s*)?(?<bait1>[^\(\)⇒]+)⇒(\((?<hooking1>(スト|プレ))\))?(?<bait2>[^\(\)⇒]+)⇒(\((?<hooking2>(スト|プレ))\))?(?<bait3>[^\(\)⇒]+)⇒(\((?<hooking3>(スト|プレ))\))?(?<bait4>[^\(\)⇒]+)⇒(\((?<hooking4>(スト|プレ))\))?$", RegexOptions.Compiled);
        private static Regex _トレードリリース推奨パターン = new Regex(@"^※(?<fish>.*)が釣れたらトレードリリース$", RegexOptions.Compiled);
        private static Regex _漁師の直感条件パターン = new Regex(@"^要\s*(?<fish>[^× ]+)×(?<count>[0-9]+)$", RegexOptions.Compiled);
        private static Regex _ET時間帯パターン = new Regex(@"^ET ?(?<fromhour>[0-9]+):(?<fromminute>[0-9]+) ?[-～] ?(?<tohour>[0-9]+):(?<tominute>[0-9]+)$", RegexOptions.Compiled);
        private static Regex _天候移ろいパターン = new Regex(@"^(?<before>[^/⇒]+(/[^/⇒]+)*)⇒(?<after>[^/⇒]+(/[^/⇒]+)*)$", RegexOptions.Compiled);
        private TranslationTextId _nameId;
        private TranslationTextId _memoId;
        private string _memoSource;

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, WeatherType weather, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot, weather) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, int hourOfStart, int hourOfEnd, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weather, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weather) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, WeatherType weather, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, weather)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, weatherBefore, weatherAfter)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, int hourOfStart, int hourOfEnd, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weather, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weather)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weatherBefore, weatherAfter)))
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, string memo = "")
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, WeatherType weather, string memo = "")
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot, weather) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, string memo = "")
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weather, string memo = "")
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weather) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, string memo = "")
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, WeatherType weather, string memo = "")
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, weather)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, weatherBefore, weatherAfter)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, string memo = "")
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weather, string memo = "")
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weather)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weatherBefore, weatherAfter)))
        {
        }

        private Fish(string fishId, IEnumerable<FishingBait> fishingBaits, string memo, IEnumerable<FishingCondition> conditions)
        {
            if (fishId.Trim() != fishId)
                throw new ArgumentException();
            Id = new GameDataObjectId(GameDataObjectCategory.Fish, fishId);
            _nameId = new TranslationTextId(TranslationCategory.Fish, fishId);
            _memoId = new TranslationTextId(TranslationCategory.FishMemo, fishId);
            FishingSpots = conditions.Select(c => c.FishingSpot).ToArray();
            FishingBaits = fishingBaits.ToArray();
            _memoSource = memo;
            FishingConditions = conditions.ToArray();
            DifficultyValue = conditions.Min(item => item.DifficultyValue);
            DifficultySymbol = DifficultySymbol.None;
            foreach (var lang in Translate.SupportedLanguages)
                TranslateMemo(lang);
        }

        public GameDataObjectId Id { get; }
        public string Name => Translate.Instance[_nameId];
        public IEnumerable<FishingSpot> FishingSpots { get; }
        public IEnumerable<FishingBait> FishingBaits { get; }
        public double DifficultyValue { get; }
        public DifficultySymbol DifficultySymbol { get; private set; }
        public IEnumerable<FishingCondition> FishingConditions { get; }

        public string TranslatedMemo => Translate.Instance[_memoId];

        public FishChanceTimeRegions GetFishingChance(EorzeaDateTimeHourRegions wholeRegion, EorzeaDateTime now)
        {
            EorzeaDateTimeHourRegions foundRegion = null;
            FishingCondition foundCondition = null;
            foreach (var condition in FishingConditions)
            {
                var temp = condition.GetFishingChance(wholeRegion);
                if (foundRegion == null || foundRegion.Begin > temp.Begin)
                {
                    foundRegion = temp;
                    foundCondition = condition;
                }
            }
            if (foundRegion == null || foundRegion.IsEmpty || foundCondition == null)
                return null;
            return new FishChanceTimeRegions(this, foundCondition, foundRegion, now);
        }

        public static void RandDifficulty(IEnumerable<Fish> fishes)
        {
            var maxDifficultyOfFish = fishes.Max(fish => fish.DifficultyValue);
            var minDifficultyOfFish = fishes.Min(fish => fish.DifficultyValue);
            var log_maxDifficultyOfFish = Math.Log(maxDifficultyOfFish);
            var log_minDifficultyOfFish = Math.Log(minDifficultyOfFish);
            var width = (log_maxDifficultyOfFish - log_minDifficultyOfFish) / 6;
            foreach (var fish in fishes)
            {
                if (Math.Log(fish.DifficultyValue) < log_minDifficultyOfFish + width)
                    fish.DifficultySymbol = DifficultySymbol.E;
                else if (Math.Log(fish.DifficultyValue) < log_minDifficultyOfFish + width * 2)
                    fish.DifficultySymbol = DifficultySymbol.D;
                else if (Math.Log(fish.DifficultyValue) < log_minDifficultyOfFish + width * 3)
                    fish.DifficultySymbol = DifficultySymbol.C;
                else if (Math.Log(fish.DifficultyValue) < log_minDifficultyOfFish + width * 4)
                    fish.DifficultySymbol = DifficultySymbol.B;
                else if (Math.Log(fish.DifficultyValue) < log_minDifficultyOfFish + width * 5)
                    fish.DifficultySymbol = DifficultySymbol.A;
                else
                    fish.DifficultySymbol = DifficultySymbol.S;
            }
        }

        public IEnumerable<string> CheckTranslation()
        {
            return Translate.Instance.CheckTranslation(_nameId);
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return Id.Equals(((Fish)o).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        private void TranslateMemo(string lang)
        {
            var translatedMemo =
                string.Join(
                    "\n",
                    _memoSource.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => TranslateMemoLine(s, lang)));
            Translate.Instance.Add(_memoId, lang, translatedMemo);
        }

        private string TranslateMemoLine(string text, string lang)
        {
            Match m;
            if ((m = _直釣り省略パターン.Match(text)).Success)
            {
                var requires = m.Groups["requires"].Success ? TranslateRequirement(m.Groups["requires"].Value, lang) : null;
                var hooking = m.Groups["hooking"].Success ? TranslateHooking(m.Groups["hooking"].Value, lang) : null;
                return string.Format(
                    "{0}{1}",
                    requires != null ? string.Format("({0})", requires.TranslatedRequirement) : "",
                    hooking != null ? string.Format("({0})", hooking) : "");
            }
            else if ((m = _直釣りパターン.Match(text)).Success)
            {
                var requires = m.Groups["requires"].Success ? TranslateRequirement(m.Groups["requires"].Value, lang) : null;
                var baitName = Translate.Instance[new TranslationTextId(TranslationCategory.FishingBait, m.Groups["bait"].Value), lang];
                var hooking = m.Groups["hooking"].Success ? TranslateHooking(m.Groups["hooking"].Value, lang) : null;
                return string.Format(
                    "{0}{1}⇒{2}{3}",
                    requires != null ? string.Format("({0})", requires.TranslatedRequirement) : "",
                    baitName,
                    hooking != null ? string.Format("({0})", hooking) : "",
                    requires != null && requires.RequiredFishName != null ? requires.RequiredFishName : Translate.Instance[_nameId, lang]);
            }
            else if ((m = _泳がせ釣りパターン.Match(text)).Success)
            {
                var requires = m.Groups["requires"].Success ? TranslateRequirement(m.Groups["requires"].Value, lang) : null;
                var bait1Name = Translate.Instance[new TranslationTextId(TranslationCategory.FishingBait, m.Groups["bait1"].Value), lang];
                var hooking1 = m.Groups["hooking1"].Success ? TranslateHooking(m.Groups["hooking1"].Value, lang) : null;
                var bait2Name =
                    string.Join(
                        "/",
                        m.Groups["bait2"].Value.Split('/')
                        .Select(s => s.EndsWith("HQ") ? s.Substring(0, s.Length - 2) : s)
                        .Select(s =>
                            string.Format(
                                Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "HighQualityItem"), lang],
                                Translate.Instance[new TranslationTextId(TranslationCategory.Fish, s), lang])));
                var hooking2 = m.Groups["hooking2"].Success ? TranslateHooking(m.Groups["hooking2"].Value, lang) : null;
                return string.Format(
                    "{0}{1}⇒{2}{3}⇒{4}{5}",
                    requires != null ? string.Format("({0})", requires.TranslatedRequirement) : "",
                    bait1Name,
                    hooking1 != null ? string.Format("({0})", hooking1) : "",
                    bait2Name,
                    hooking2 != null ? string.Format("({0})", hooking2) : "",
                    requires != null && requires.RequiredFishName != null ? requires.RequiredFishName : Translate.Instance[_nameId, lang]);
            }
            else if ((m = _2段泳がせ釣りパターン.Match(text)).Success)
            {
                var requires = m.Groups["requires"].Success ? TranslateRequirement(m.Groups["requires"].Value, lang) : null;
                var bait1Name = Translate.Instance[new TranslationTextId(TranslationCategory.FishingBait, m.Groups["bait1"].Value), lang];
                var hooking1 = m.Groups["hooking1"].Success ? TranslateHooking(m.Groups["hooking1"].Value, lang) : null;
                var bait2Name =
                    string.Join(
                        "/",
                        m.Groups["bait2"].Value.Split('/')
                        .Select(s => s.EndsWith("HQ") ? s.Substring(0, s.Length - 2) : s)
                        .Select(s => Translate.Instance[new TranslationTextId(TranslationCategory.Fish, s), lang] + "HQ"));
                var hooking2 = m.Groups["hooking2"].Success ? TranslateHooking(m.Groups["hooking2"].Value, lang) : null;
                var bait3Name =
                    string.Join(
                        "/",
                        m.Groups["bait3"].Value.Split('/')
                        .Select(s => s.EndsWith("HQ") ? s.Substring(0, s.Length - 2) : s)
                        .Select(s => Translate.Instance[new TranslationTextId(TranslationCategory.Fish, s), lang] + "HQ"));
                var hooking3 = m.Groups["hooking3"].Success ? TranslateHooking(m.Groups["hooking3"].Value, lang) : null;
                return string.Format(
                    "{0}{1}⇒{2}{3}⇒{4}{5}⇒{6}{7}",
                    requires != null ? string.Format("({0})", requires.TranslatedRequirement) : "",
                    bait1Name,
                    hooking1 != null ? string.Format("({0})", hooking1) : "",
                    bait2Name,
                    hooking2 != null ? string.Format("({0})", hooking2) : "",
                    bait3Name,
                    hooking3 != null ? string.Format("({0})", hooking3) : "",
                    requires != null && requires.RequiredFishName != null ? requires.RequiredFishName : Translate.Instance[_nameId, lang]);
            }
            else if ((m = _3段泳がせ釣りパターン.Match(text)).Success)
            {
                var requires = m.Groups["requires"].Success ? TranslateRequirement(m.Groups["requires"].Value, lang) : null;
                var bait1Name = Translate.Instance[new TranslationTextId(TranslationCategory.FishingBait, m.Groups["bait1"].Value), lang];
                var hooking1 = m.Groups["hooking1"].Success ? TranslateHooking(m.Groups["hooking1"].Value, lang) : null;
                var bait2Name =
                    string.Join(
                        "/",
                        m.Groups["bait2"].Value.Split('/')
                        .Select(s => s.EndsWith("HQ") ? s.Substring(0, s.Length - 2) : s)
                        .Select(s => Translate.Instance[new TranslationTextId(TranslationCategory.Fish, s), lang] + "HQ"));
                var hooking2 = m.Groups["hooking2"].Success ? TranslateHooking(m.Groups["hooking2"].Value, lang) : null;
                var bait3Name =
                    string.Join(
                        "/",
                        m.Groups["bait3"].Value.Split('/')
                        .Select(s => s.EndsWith("HQ") ? s.Substring(0, s.Length - 2) : s)
                        .Select(s => Translate.Instance[new TranslationTextId(TranslationCategory.Fish, s), lang] + "HQ"));
                var hooking3 = m.Groups["hooking3"].Success ? TranslateHooking(m.Groups["hooking3"].Value, lang) : null;
                var bait4Name =
                    string.Join(
                        "/",
                        m.Groups["bait4"].Value.Split('/')
                        .Select(s => s.EndsWith("HQ") ? s.Substring(0, s.Length - 2) : s)
                        .Select(s => Translate.Instance[new TranslationTextId(TranslationCategory.Fish, s), lang] + "HQ"));
                var hooking4 = m.Groups["hooking4"].Success ? TranslateHooking(m.Groups["hooking4"].Value, lang) : null;
                return string.Format(
                    "{0}{1}⇒{2}{3}⇒{4}{5}⇒{6}{7}⇒{8}{9}",
                    requires != null ? string.Format("({0})", requires.TranslatedRequirement) : "",
                    bait1Name,
                    hooking1 != null ? string.Format("({0})", hooking1) : "",
                    bait2Name,
                    hooking2 != null ? string.Format("({0})", hooking2) : "",
                    bait3Name,
                    hooking3 != null ? string.Format("({0})", hooking3) : "",
                    bait4Name,
                    hooking4 != null ? string.Format("({0})", hooking4) : "",
                    requires != null && requires.RequiredFishName != null ? requires.RequiredFishName : Translate.Instance[_nameId, lang]);
            }
            else if ((m = _トレードリリース推奨パターン.Match(text)).Success)
            {
                var fishName = Translate.Instance[new TranslationTextId(TranslationCategory.Fish, m.Groups["fish"].Value), lang];
                return string.Format(Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "UseTradeReleaseIf"), lang], fishName);
            }
            else
                throw new Exception();
        }

        private static string TranslateHooking(string source, string lang)
        {
            switch (source)
            {
                case "スト":
                    return Translate.Instance[new TranslationTextId(TranslationCategory.Action, "ストロングフッキング"), lang];
                case "プレ":
                    return Translate.Instance[new TranslationTextId(TranslationCategory.Action, "プレシジョンフッキング"), lang];
                default:
                    throw new Exception();
            }
        }

        private static FishingRequirement TranslateRequirement(string source, string lang)
        {
            var requirements =
                source.Split(',')
                .Select(s => s.Trim())
                .Select(s =>
                {
                    Match m;
                    string weather1;
                    string weather2;
                    if ((m = _漁師の直感条件パターン.Match(s)).Success)
                    {
                        var fishName = Translate.Instance[new TranslationTextId(TranslationCategory.Fish, m.Groups["fish"].Value), lang];
                        var count = m.Groups["count"].Value;
                        return new FishingRequirement
                        {
                            TranslatedRequirement = string.Format(Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "RequiresFishes"), lang], fishName, count),
                            RequiredFishName = fishName,
                        };
                    }
                    if ((m = _ET時間帯パターン.Match(s)).Success)
                    {
                        var fromHour = int.Parse(m.Groups["fromhour"].Value);
                        var fromMinute = int.Parse(m.Groups["fromminute"].Value);
                        var toHour = int.Parse(m.Groups["tohour"].Value);
                        var toMinute = int.Parse(m.Groups["tominute"].Value);
                        return new FishingRequirement
                        {
                            TranslatedRequirement = string.Format("ET {0:D2}:{1:D2} - {2:D2}:{3:D2}", fromHour, fromMinute, toHour, toMinute)
                        };
                    }
                    if ((weather1 = TranslateWeathers(s, lang)) != null)
                        return new FishingRequirement
                        {
                            TranslatedRequirement = weather1
                        };
                    if ((m = _天候移ろいパターン.Match(s)).Success &&
                        (weather1 = TranslateWeathers(m.Groups["before"].Value, lang)) != null &&
                        (weather2 = TranslateWeathers(m.Groups["after"].Value, lang)) != null)
                    {
                        return new FishingRequirement
                        {
                            TranslatedRequirement = string.Format("{0}⇒{1}", weather1, weather2)
                        };
                    }
                    else if (s == "天候不問")
                        return new FishingRequirement
                        {
                            TranslatedRequirement = Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "AnyWeather"), lang]
                        };
                    else if (s == "時間帯不問")
                        return new FishingRequirement
                        {
                            TranslatedRequirement = Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "AnyTime"), lang]
                        };
                    else if (s == "要フィッシュアイ")
                        return new FishingRequirement
                        {
                            TranslatedRequirement =
                                string.Format(
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "RequiresAction"), lang],
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Action, "フィッシュアイ"), lang])
                        };
                    else if (s == "要引っ掛け釣り")
                        return new FishingRequirement
                        {
                            TranslatedRequirement =
                                string.Format(
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "RequiresAction"), lang],
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Action, "引っ掛け釣り"), lang])
                        };
                    else if (s == "フィッシュアイ不要")
                        return new FishingRequirement
                        {
                            TranslatedRequirement =
                                string.Format(
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "NotRequiresAction"), lang],
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Action, "フィッシュアイ"), lang])
                        };
                    else
                        throw new Exception();
                });
            if (requirements.Where(requirement => requirement.RequiredFishName != null).Take(2).Count() > 1)
                throw new Exception();
            return new FishingRequirement
            {
                TranslatedRequirement =
                    string.Join(
                        ", ",
                        requirements
                            .Select(requirement => requirement.TranslatedRequirement)),
                RequiredFishName =
                    requirements
                    .Select(requirement => requirement.RequiredFishName)
                    .Where(fishName => fishName != null)
                    .FirstOrDefault()
            };
        }

        private static string TranslateWeathers(string text, string lang)
        {
            var weathers =
                text.Split('/')
                .Select(w => TryParseAsWeather(w))
                .ToArray();
            if (weathers.Any(w => w == WeatherType.None))
                return null;
            return string.Join("/", weathers.Select(w => Translate.Instance[new TranslationTextId(TranslationCategory.Weather, w.ToString()), lang]));
        }

        private static WeatherType TryParseAsWeather(string text)
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
    }
}

