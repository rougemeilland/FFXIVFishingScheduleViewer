using FFXIVFishingScheduleViewer.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FFXIVFishingScheduleViewer.Models
{
    static partial class FishExtensions
    {
        private class FishPageIdElement
        {
            public string fishId { get; set; }
            public string pageId { get; set; }
        };

        private class FishAtariElement
        {
            public string fish { get; set; }
            public string atari { get; set; }
        };

        private class FishBestBaitElement
        {
            public string spot { get; set; }
            public string fish { get; set; }
            public string bestBait { get; set; }
            public string[][] routes { get; set; }
            public double countOfBait { get; set; }
            public double countOfCasting { get; set; }
        }

        private class FishingRequirement
        {
            public FishingRequirement()
            {
                TranslatedRequirementText = null;
                RequiredFishNameId = null;
                RequiredFishRawId = null;
                RequiredFishCount = 0;
            }
            public string TranslatedRequirementText { get; set; }
            public TranslationTextId RequiredFishNameId { get; set; }
            public string RequiredFishRawId { get; set; }
            public int RequiredFishCount { get; set; }
        }

        private class FishingMethod
        {
            public FishingMethod()
            {
                TranslatedMethodText = null;
                FishingBaitNameId = null;
                NeedMooching = null;
            }
            public string TranslatedMethodText { get; set; }
            public TranslationTextId FishingBaitNameId { get; set; }
            public bool? NeedMooching { get; set; }
        }

        private class MemoLine
        {
            public MemoLine()
            {
                Text = null;
                TranslationIdOfBait = null;
                ExpectedCountOBait = 0;
                ExpectedCountOfCasting = 0;
                NeedMooching = null;
            }

            public string Text { get; set; }
            public TranslationTextId TranslationIdOfBait { get; set; }
            public double ExpectedCountOfCasting { get; set; }
            public double ExpectedCountOBait { get; set; }
            public bool? NeedMooching { get; set; }
        }

        private class BestBaitElement
        {
            public string SpotRawId { get; set; }
            public string FishRawId { get; set; }
            public IEnumerable<IEnumerable<string>> Routes { get; set; }
        }

        private class ExpectedCountValueElement
        {
            public double ExpectedCountOfCasting { get; set; }
            public double ExpectedCountOBait { get; set; }
        }

        private const string _釣り方詳細パターンソース = @" *(?<flag>@[!]+)?(?<bait>[^\(\)⇒;\n""]+?)( *⇒ *\( *(?<atari1>!{1,3}) *(?<hooking1>(スト|プレ)) *\) *(?<fish1>[^\(\)⇒;\n""]+?) *HQ( *⇒ *\( *(?<atari2>!{1,3}) *(?<hooking2>(スト|プレ)) *\) *(?<fish2>[^\(\)⇒;\n""]+?) *HQ( *⇒ *\( *(?<atari3>!{1,3}) *(?<hooking3>(スト|プレ)) *\) *(?<fish3>[^\(\)⇒;\n""]+?) *HQ)?)?)? *⇒ *\( *(?<atari4>!{1,3}) *(?<hooking4>(スト|プレ)) *\)";
        private static Regex _釣り方パターン = new Regex(@"^(\((?<requires>(要|天候不問|時間帯不問)[^\)]*)\) *)?(?<method>" + _釣り方詳細パターンソース + "( *; *" + _釣り方詳細パターンソース + ")*)$", RegexOptions.Compiled);
        private static Regex _釣り方詳細パターン = new Regex("^" + _釣り方詳細パターンソース + "( *; *" + _釣り方詳細パターンソース + ")*$", RegexOptions.Compiled);
        private static Regex _トレードリリース推奨パターン = new Regex(@"^※(?<fish>.*)が釣れたらトレードリリース$", RegexOptions.Compiled);
        private static Regex _漁師の直感条件パターン = new Regex(@"^要 *(?<fish>[^× ]+)×(?<count>[0-9]+)$", RegexOptions.Compiled);
        private static Regex _ET時間帯パターン = new Regex(@"^ET ?(?<fromhour>[0-9]+):(?<fromminute>[0-9]+) ?[-～] ?(?<tohour>[0-9]+):(?<tominute>[0-9]+)$", RegexOptions.Compiled);
        private static Regex _天候移ろいパターン = new Regex(@"^(?<before>[^/⇒]+(/[^/⇒]+)*)⇒(?<after>[^/⇒]+(/[^/⇒]+)*)$", RegexOptions.Compiled);
        private static TranslationTextId _unknownBaitNameId = new TranslationTextId(TranslationCategory.FishingBait, "??unknown??");
        private static IDictionary<string, string> _pageIdSourceOfCBH;
        private static IDictionary<string, string> _pageIdSourceOfEDB;
        private static IDictionary<string, IDictionary<string, ExpectedCountValueElement>> _primitiveCountValues;
        private static IDictionary<string, IDictionary<string, ExpectedCountValueElement>> _expectedCountValues;
#if DEBUG
        private static IDictionary<string, IDictionary<string, BestBaitElement>> _bestBaitList;
        private static IDictionary<string, string> _atariList;
        private static bool _isDebugMode = true;
#endif

        static FishExtensions()
        {
            _pageIdSourceOfCBH =
                GetPageIdSourceOfCBH()
                .ToDictionary(item => item.fishId, item => item.pageId);

            _pageIdSourceOfEDB =
                GetPageIdSourceOfEDB()
                .ToDictionary(item => item.fishId, item => item.pageId);

#if DEBUG
            _atariList =
                GetAtariList()
                .ToDictionary(item => item.fish, item => item.atari);
#endif

            // countOfBait: その魚を1匹釣るために消費する釣り餌の期待値
            // countOfCasting: その魚を1匹釣るために実行する「キャスティング」または「泳がせ釣り」の回数の期待値
            // 【注意】
            // countOfBaitとcountOfCastingについては、こういった計算の目的にはあまり適していない(公平性に欠ける)データを使用しているため、
            // 実際に試行した場合とはかけ離れている場合があると思われる。
            // そのため、これらの数値については参考程度にとどめること。
            // 特に以下のような魚については数値が信頼できない傾向が顕著であると思われる。
            // ・釣れる条件(時刻/天候)に制限のある魚
            // ・特殊なスキルが有効な魚(引っ掛け釣り/トレードリリース/サルベージなど)
            // ・特殊な状況が必要な魚(漁師の直感/特定のクエストやリーヴの受注など)
#if DEBUG
            _bestBaitList =
                GetFishBestBaitSource()
                .GroupBy(item => item.spot)
                .ToDictionary(
                    g => g.Key,
                    g =>
                        g
                        .ToDictionary(
                            item => item.fish,
                            item =>
                                new BestBaitElement
                                {
                                    SpotRawId = g.Key,
                                    FishRawId = item.fish,
                                    Routes = item.routes,
                                }) as IDictionary<string, BestBaitElement>);
#endif
            _primitiveCountValues =
                GetFishBestBaitSource()
                .GroupBy(item => item.spot)
                .ToDictionary(
                    g => g.Key,
                    g =>
                        g
                        .ToDictionary(
                            item => item.fish,
                            item =>
                                new ExpectedCountValueElement
                                {
                                    ExpectedCountOBait = item.countOfBait,
                                    ExpectedCountOfCasting = item.countOfCasting,
                                }) as IDictionary<string, ExpectedCountValueElement>);
            _expectedCountValues = new Dictionary<string, IDictionary<string, ExpectedCountValueElement>>();
        }

        public static void TranslateMemo(this Fish fish)
        {
            foreach (var condition in fish.FishingConditions)
                condition.TranslateMemo();
        }

        public static void TranslateMemo(this FishingCondition condition)
        {
            var needMooching = false;
            foreach (var lang in Translate.SupportedLanguages)
            {
#if DEBUG && false
                System.Diagnostics.Debug.Indent();
#endif
                var memoLines =
                    condition.RawMemoText
                    .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Select(text => TranslateMemoLine(condition, text, lang))
                    .ToArray();
#if DEBUG && false
                System.Diagnostics.Debug.Unindent();
#endif
#if DEBUG
                if (_isDebugMode)
                {
                    // 釣り餌情報の整合性を確認する
                    if (memoLines.Any())
                    {
                        // メモに記載してある釣り餌をピックアップする
                        var baitIds = memoLines.Select(line => line.TranslationIdOfBait).Distinct().ToArray();
                        baitIds = baitIds.Where(bait => bait != _unknownBaitNameId).ToArray();
                        if (baitIds.Length != condition.FishingBaits.Count())
                        {
                            throw new Exception(
                                string.Format(
                                    "メモの書式に誤りがあります。釣り餌の種類が一致していません。: fish='{0}', baits=[{1}], baitsOfMemo=[{2}]",
                                    Translate.Instance[condition.Fish.NameId, lang],
                                    string.Join(", ", condition.FishingBaits.Select(bait => string.Format("'{0}'", Translate.Instance[bait.NameId, lang]))),
                                    string.Join(", ", baitIds.Select(id => id.ToString()))));
                        }
                        else if (condition.FishingBaits.Select(bait => bait.NameId).Except(baitIds).Any())
                        {
                            throw new Exception(
                                string.Format(
                                    "メモの書式に誤りがあります。釣り餌の種類が一致していません。: fish='{0}', baits=[{1}], baitsOfMemo=[{2}]",
                                    Translate.Instance[condition.Fish.NameId, lang],
                                    string.Join(", ", condition.FishingBaits.Select(bait => string.Format("'{0}'", Translate.Instance[bait.NameId, lang]))),
                                    string.Join(", ", baitIds.Select(id => id.ToString()))));
                        }
                        else
                        {
                            // OK
                        }
                    }
                }
#endif
                var countOfBait = memoLines.Sum(line => line.ExpectedCountOBait);
                var countOfCasting = memoLines.Sum(line => line.ExpectedCountOfCasting);
                needMooching = needMooching || memoLines.Select(memoLine => memoLine.NeedMooching).Aggregate(false, (p1, p2) => p1 || (p2 == true));
                SetExpectedCountValue(
                    ((IGameDataObject)condition.FishingSpot).InternalId,
                    ((IGameDataObject)condition.Fish).InternalId,
                    memoLines.Sum(line => line.ExpectedCountOBait),
                    memoLines.Sum(line => line.ExpectedCountOfCasting));
#if DEBUG && false
                if (((IGameDataObject)condition.Fish).InternalId == "シャリベネ")
                {
                    System.Diagnostics.Debug.WriteLine(
                    string.Format(
                        "**SetNeedMooching: lang={0}, spot={1}, fish={2}, mooching={3}",
                        lang,
                        ((IGameDataObject)condition.FishingSpot).InternalId,
                        ((IGameDataObject)condition.Fish).InternalId,
                        needMooching));
                }
#endif
#if DEBUG && false
                System.Diagnostics.Debug.WriteLine(
                    string.Format(
                        "**set count: spot={0}, fish={1}, count1={2}, count2={3}",
                        ((IGameDataObject)condition.FishingSpot).InternalId,
                        ((IGameDataObject)condition.Fish).InternalId,
                        memoLines.Sum(line => line.ExpectedCountOBait),
                        memoLines.Sum(line => line.ExpectedCountOfCasting)));
                System.Diagnostics.Debug.WriteLine("----------");
#endif
                var translatedMemo =
                    string.Join("\n\n", memoLines.Select(line => line.Text));
                Translate.Instance.Add(condition.DefaultMemoId, lang, translatedMemo);
            }
#if DEBUG && false
            //if (((IGameDataObject)condition.Fish).InternalId == "シャリベネ")
            {
                System.Diagnostics.Debug.WriteLine(
                string.Format(
                    "**SetNeedMooching: spot={0}, fish={1}, mooching={2}",
                    ((IGameDataObject)condition.FishingSpot).InternalId,
                    ((IGameDataObject)condition.Fish).InternalId,
                    needMooching));
                System.Diagnostics.Debug.WriteLine("----------");
            }
#endif
            condition.SetNeedMooching(needMooching);
        }

        public static string GetCBHLink(this Fish fish)
        {
            string pageId;
            if (!_pageIdSourceOfCBH.TryGetValue(((IGameDataObject)fish).InternalId, out pageId))
                return null;
            var format = Translate.Instance[new TranslationTextId(TranslationCategory.Url, "CBH.FishPage")];
            return string.Format(format, pageId);
        }

        public static string GetEDBLink(this Fish fish)
        {
            string pageId;
            if (!_pageIdSourceOfEDB.TryGetValue(((IGameDataObject)fish).InternalId, out pageId))
                return null;
            var format = Translate.Instance[new TranslationTextId(TranslationCategory.Url, "EDB.FishPage")];
            return string.Format(format, pageId);
        }

        internal static bool IsFishRawId(this string text)
        {
            return _pageIdSourceOfCBH.ContainsKey(text);
        }

        public static double GetExpectedCountOfCasting(this FishingCondition condition)
        {
            IDictionary<string, ExpectedCountValueElement> value1;
            if (!_expectedCountValues.TryGetValue(((IGameDataObject)condition.FishingSpot).InternalId, out value1))
                throw new Exception();
            ExpectedCountValueElement value2;
            if (!value1.TryGetValue(((IGameDataObject)condition.Fish).InternalId, out value2))
                throw new Exception();
            return value2.ExpectedCountOfCasting;
        }

        internal static void SetExpectedCountValue(string fishingSpotRawId, string fishRawId, double expectedCountOBait, double expectedCountOfCasting)
        {
            IDictionary<string, ExpectedCountValueElement> value1;
            if (!_expectedCountValues.TryGetValue(fishingSpotRawId, out value1))
            {
                value1 = new Dictionary<string, ExpectedCountValueElement>();
                _expectedCountValues.Add(fishingSpotRawId, value1);
            }
            ExpectedCountValueElement value2;
            if (!value1.TryGetValue(fishRawId, out value2))
            {
                value2 =
                    new ExpectedCountValueElement
                    {
                        ExpectedCountOBait = expectedCountOBait,
                        ExpectedCountOfCasting = expectedCountOfCasting,
                    };
                value1.Add(fishRawId, value2);
            }
            if (value2.ExpectedCountOBait != expectedCountOBait || value2.ExpectedCountOfCasting != expectedCountOfCasting)
                throw new Exception();
        }

        internal static double[] GetPrimitiveCounts(string fishingSpotRawId, string fishRawId)
        {
            try
            {
                IDictionary<string, ExpectedCountValueElement> value1;
                if (!_primitiveCountValues.TryGetValue(fishingSpotRawId, out value1))
                    throw new Exception();
                ExpectedCountValueElement value2;
                if (!value1.TryGetValue(fishRawId, out value2))
                    throw new Exception();
                return new[] { value2.ExpectedCountOBait, value2.ExpectedCountOfCasting };
            }
            catch (Exception)
            {
#if DEBUG
                if (_isDebugMode)
                    throw;
                return new[] { 0.0, 0.0 };
#else
                throw;
#endif
            }
        }

        private static MemoLine TranslateMemoLine(FishingCondition condition, string text, string lang)
        {
            Match m;
            if ((m = _釣り方パターン.Match(text)).Success)
            {
                var requires = m.Groups["requires"].Success ? TranslateRequirement(m.Groups["requires"].Value, lang) : null;
                var requiredFishCount = requires != null && requires.RequiredFishCount > 0 ? requires.RequiredFishCount : 1;
                var targetFishRawId = requires?.RequiredFishRawId ?? ((IGameDataObject)condition.Fish).InternalId;
                var method =
                    m.Groups["method"].Success
                    ? TranslateFishingMethod(
                        condition.FishingSpot.NameId,
                        requires?.RequiredFishNameId ?? condition.Fish.NameId,
                        ((IGameDataObject)condition.FishingSpot).InternalId,
                        targetFishRawId,
                        m.Groups["method"].Value,
                        lang)
                    : null;
                if (method == null)
                    throw new Exception();
#if DEBUG && false
                if (targetFishRawId == "シャリベネ")
                {
                    System.Diagnostics.Debug.WriteLine(
                    string.Format(
                        "**SetNeedMooching: lang={0}, text='{1}', spot={2}, fish={3}, mooching={4}",
                        lang,
                        text,
                        ((IGameDataObject)condition.FishingSpot).InternalId,
                        ((IGameDataObject)condition.Fish).InternalId,
                        method.NeedMooching));
                }
#endif
                var counts = GetPrimitiveCounts(((IGameDataObject)condition.FishingSpot).InternalId, targetFishRawId);
#if DEBUG && false
                System.Diagnostics.Debug.WriteLine(
                    string.Format(
                        "**get count: spot={0}, fish={1}, count1={2}, count2={3}, m={4}",
                        ((IGameDataObject)condition.FishingSpot).InternalId,
                        targetFishRawId,
                        counts[0],
                        counts[1],
                        requiredFishCount));
#endif
                return new MemoLine
                {
                    Text =
                        string.Format(
                            "{0}{1}",
                            requires != null ? string.Format("({0})\n  ", requires.TranslatedRequirementText) : "",
                            method. TranslatedMethodText),
                    TranslationIdOfBait = method.FishingBaitNameId,
                    ExpectedCountOBait = counts[0] * requiredFishCount,
                    ExpectedCountOfCasting = counts[1] * requiredFishCount,
                    NeedMooching = method.NeedMooching,
                };
            }
            else if ((m = _トレードリリース推奨パターン.Match(text)).Success)
            {
                var fishName = Translate.Instance[new TranslationTextId(TranslationCategory.Fish, m.Groups["fish"].Value), lang];
                return new MemoLine
                {
                    Text = string.Format(Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "UseTradeReleaseIf"), lang], fishName),
                    TranslationIdOfBait = _unknownBaitNameId,
                };
            }
            else
                throw new Exception(string.Format("Bad memo format. memo='{0}'", condition.DefaultMemoId));
        }

        private static string TranslateHooking(string atari, string hooking, string lang)
        {
            switch (atari)
            {
                case "!":
                    switch (hooking)
                    {
                        case "スト":
                            throw new Exception();
                        case "プレ":
                        case "":
                            return "!";
                        default:
                            throw new Exception();
                    }
                case "!!":
                    switch (hooking)
                    {
                        case "スト":
                        case "":
                            return "!!";
                        case "プレ":
                            throw new Exception();
                        default:
                            throw new Exception();
                    }
                case "!!!":
                    switch (hooking)
                    {
                        case "スト":
                        case "":
                            return string.Format("{0}", atari);
                        case "プレ":
                            return string.Format("{0} {1}", atari, Translate.Instance[new TranslationTextId(TranslationCategory.Action, "プレシジョンフッキング"), lang]);
                        default:
                            throw new Exception();
                    }
                case "":
                    throw new Exception();
                default:
                    throw new Exception();
            }
        }

        private static FishingRequirement TranslateRequirement(string sourceText, string lang)
        {
            var requirements =
                sourceText.Split(',')
                .Select(s => s.Trim())
                .Select(s =>
                {
                    Match m;
                    string weather1;
                    string weather2;
                    if ((m = _漁師の直感条件パターン.Match(s)).Success)
                    {
                        var rawFishId = m.Groups["fish"].Value;
                        var fishNameId = new TranslationTextId(TranslationCategory.Fish, rawFishId);
                        var count = int.Parse(m.Groups["count"].Value);
                        return new FishingRequirement
                        {
                            TranslatedRequirementText =
                                string.Format(
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "RequiresFishes"), lang],
                                    Translate.Instance[fishNameId, lang],
                                    count),
                            RequiredFishNameId = fishNameId,
                            RequiredFishRawId = rawFishId,
                            RequiredFishCount = count,
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
                            TranslatedRequirementText =
                                string.Format(
                                    "{0} {1:D2}:{2:D2} - {3:D2}:{4:D2}",
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "ET.Short")],
                                    fromHour,
                                    fromMinute,
                                    toHour,
                                    toMinute)
                        };
                    }
                    if ((weather1 = TranslateWeathers(s, lang)) != null)
                        return new FishingRequirement
                        {
                            TranslatedRequirementText = weather1
                        };
                    if ((m = _天候移ろいパターン.Match(s)).Success &&
                        (weather1 = TranslateWeathers(m.Groups["before"].Value, lang)) != null &&
                        (weather2 = TranslateWeathers(m.Groups["after"].Value, lang)) != null)
                    {
                        return new FishingRequirement
                        {
                            TranslatedRequirementText = string.Format("{0}⇒{1}", weather1, weather2)
                        };
                    }
                    else if (s == "天候不問")
                        return new FishingRequirement
                        {
                            TranslatedRequirementText = Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "AnyWeather"), lang]
                        };
                    else if (s == "時間帯不問")
                        return new FishingRequirement
                        {
                            TranslatedRequirementText = Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "AnyTime"), lang]
                        };
                    else if (s == "要フィッシュアイ")
                    {
#if true
                        throw new Exception("5.4にてフィッシュアイの効果が変更されたため条件への記載を削除しました。");
#else
                        return new FishingRequirement
                        {
                            TranslatedRequirementText =
                                string.Format(
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "RequiresAction"), lang],
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Action, "フィッシュアイ"), lang])
                        };
#endif
                    }
                    else if (s == "要引っ掛け釣り")
                        return new FishingRequirement
                        {
                            TranslatedRequirementText =
                                string.Format(
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "RequiresAction"), lang],
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Action, "引っ掛け釣り"), lang])
                        };
                    else if (s == "フィッシュアイ不要")
                    {
#if true
                        throw new Exception("5.4にてフィッシュアイの効果が変更されたため条件への記載を削除しました。");
#else
                        return new FishingRequirement
                        {
                            TranslatedRequirementText =
                                string.Format(
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "NotRequiresAction"), lang],
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Action, "フィッシュアイ"), lang])
                        };
#endif
                    }
                    else
                        throw new Exception();
                });
            if (requirements.Where(requirement => requirement.RequiredFishNameId != null || requirement.RequiredFishCount > 0).Take(2).Count() > 1)
                throw new Exception();
            return new FishingRequirement
            {
                TranslatedRequirementText =
                    string.Join(
                        ", ",
                        requirements
                            .Select(requirement => requirement.TranslatedRequirementText)),
                RequiredFishNameId =
                    requirements
                    .Select(requirement => requirement.RequiredFishNameId)
                    .Where(fishNameId => fishNameId != null)
                    .SingleOrDefault(),
                RequiredFishRawId =
                    requirements
                    .Select(requirement => requirement.RequiredFishRawId)
                    .Where(fishRawId => fishRawId != null)
                    .SingleOrDefault(),
                RequiredFishCount = requirements.Max(item => item.RequiredFishCount),
            };
        }

        private static string TranslateWeathers(string text, string lang)
        {
            var weathers =
                text.Split('/')
                .Select(w => w.TryParseAsWeather())
                .ToArray();
            if (weathers.Any(w => w == WeatherType.None))
                return null;
            return string.Join("/", weathers.Select(w => Translate.Instance[new TranslationTextId(TranslationCategory.Weather, w.ToString()), lang]));
        }

        private static FishingMethod TranslateFishingMethod(TranslationTextId fishingSpotNameId, TranslationTextId targetFishNameId, string fishingSpotRawId, string targetFishRawId, string sourceText, string lang)
        {
            var elements = sourceText.Split(';');
            var bestRoutes = (string[])null;
#if DEBUG
            if (_isDebugMode)
            {
                bestRoutes =
                    _bestBaitList[fishingSpotRawId][targetFishRawId].Routes
                    .Select(route =>
                    {
                        var routeArray = route.ToArray();
                        return
                            string.Join(
                                "⇒",
                                routeArray
                                    .Take(routeArray.Length - 1)
                                    .Concat(Enumerable.Repeat("*", 5 - routeArray.Length))
                                    .Concat(new[] { routeArray[routeArray.Length - 1] }));
                    })
                    .ToArray();
                if (elements.Length != bestRoutes.Length)
                {
                    Report(
                        string.Format(
                            "'{0}'での'{1}'の釣り方は'{2}'で合っていますか?",
                            Translate.Instance[fishingSpotNameId, "ja"],
                            Translate.Instance[targetFishNameId, "ja"],
                            string.Join("; ", bestRoutes.Select(route => route.Replace("*⇒", "")))));
                }
            }
#endif
            var result =
                elements
                .Select(text => TranslateFishingMethodElement(fishingSpotNameId, targetFishNameId, targetFishRawId, text, lang, bestRoutes))
                .ToArray();
            if (result.Where(item => !string.IsNullOrEmpty(item.TranslatedMethodText)).Count() !=
                result.Where(item => !string.IsNullOrEmpty(item.TranslatedMethodText)).Select(item => item.TranslatedMethodText).Distinct().Count())
            {
                throw new Exception(
                    string.Format(
                        "メモにおいて魚の釣り方が重複しています。: spot='{0}', fish='{1}', text='{2}'",
                        Translate.Instance[fishingSpotNameId, "ja"],
                        Translate.Instance[targetFishNameId, "ja"],
                        string.Join(
                            "; ",
                            result
                                .Where(item => !string.IsNullOrEmpty(item.TranslatedMethodText))
                                .Select(item => item.TranslatedMethodText))));

            }
            var baits = result.Select(item => item.FishingBaitNameId).Distinct().ToArray();
            if (baits.Length != 1)
                throw new Exception();
            var separater = ", " + Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "Separater.Or"), lang] + "\n  ";
            var needMooching = result.Select(item => item.NeedMooching).Aggregate(true, (p1, p2) => p1 && p2.Value);
#if DEBUG && false
            if (targetFishRawId == "シャリベネ")
            {
                System.Diagnostics.Debug.WriteLine(
                string.Format(
                    "**SetNeedMooching: lang={0}, text='{1}', mooching={2}",
                    lang,
                    sourceText,
                    needMooching));
            }
#endif
            return new FishingMethod
            {
                FishingBaitNameId = baits[0],
                TranslatedMethodText =
                    string.Join(
                        separater,
                        result.Where(item => !string.IsNullOrEmpty(item.TranslatedMethodText))
                        .Select(item => item.TranslatedMethodText)),
                NeedMooching = needMooching,
            };
        }

        private static FishingMethod TranslateFishingMethodElement(TranslationTextId fishingSpotNameId, TranslationTextId targetFishNameId, string targetFishRawId, string sourceText, string lang, IEnumerable<string> bestRoutes)
        {
            var m = _釣り方詳細パターン.Match(sourceText);
            if (!m.Success)
                throw new Exception();

            var flag = m.Groups["flag"].Success ? m.Groups["flag"].Value : "";
            var DontCheck = flag.Contains("!");

            var baitRawId = m.Groups["bait"].Value;
            var baitNameId = new TranslationTextId(TranslationCategory.FishingBait, baitRawId);
            var baitName = Translate.Instance[baitNameId, lang];

            var atari1 = m.Groups["atari1"].Success ? m.Groups["atari1"].Value : null;
            var hooking1 = atari1 != null && m.Groups["hooking1"].Success ? TranslateHooking(atari1, m.Groups["hooking1"].Value, lang) : null;
            var fish1RawId = m.Groups["fish1"].Success ? m.Groups["fish1"].Value : null;
            var fish1NameId = fish1RawId != null ? new TranslationTextId(TranslationCategory.Fish, fish1RawId) : null;
            var fish1Name =
                fish1NameId != null
                    ? string.Format(
                        Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "HighQualityItem"), lang],
                        Translate.Instance[fish1NameId, lang])
                    : null;

            var atari2 = m.Groups["atari2"].Success ? m.Groups["atari2"].Value : null;
            var hooking2 = atari2 != null && m.Groups["hooking2"].Success ? TranslateHooking(atari2, m.Groups["hooking2"].Value, lang) : null;
            var fish2RawId = m.Groups["fish2"].Success ? m.Groups["fish2"].Value : null;
            var fish2NameId = fish2RawId != null ? new TranslationTextId(TranslationCategory.Fish, fish2RawId) : null;
            var fish2Name =
                fish2NameId != null
                    ? string.Format(
                        Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "HighQualityItem"), lang],
                        Translate.Instance[fish2NameId, lang])
                    : null;

            var atari3 = m.Groups["atari3"].Success ? m.Groups["atari3"].Value : null;
            var hooking3 = atari3 != null && m.Groups["hooking3"].Success ? TranslateHooking(atari3, m.Groups["hooking3"].Value, lang) : null;
            var fish3RawId = m.Groups["fish3"].Success ? m.Groups["fish3"].Value : null;
            var fish3NameId = fish3RawId != null ? new TranslationTextId(TranslationCategory.Fish, fish3RawId) : null;
            var fish3Name =
                fish3NameId != null
                    ? string.Format(
                        Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "HighQualityItem"), lang],
                        Translate.Instance[fish2NameId, lang])
                    : null;

            var atari4 = m.Groups["atari4"].Value;
            var hooking4 = TranslateHooking(atari4, m.Groups["hooking4"].Value, lang);
#if DEBUG
            if (_isDebugMode && !DontCheck)
            {
                if (fish1NameId != null && atari1 != null)
                {
                    if (_atariList[fish1RawId] != atari1)
                    {
                        Report(
                            string.Format(
                            "'{0}'のアタリの強さは'{1}'で合っていますか?",
                            Translate.Instance[fish1NameId, "ja"],
                            atari1));
                    }
                }

                if (fish2NameId != null && atari2 != null)
                {
                    if (_atariList[fish2RawId] != atari2)
                    {
                        Report(
                            string.Format(
                                "'{0}'のアタリの強さは'{1}'で合っていますか?",
                                Translate.Instance[fish2NameId, "ja"],
                                atari2));
                    }
                }

                if (fish3NameId != null && atari3 != null)
                {
                    if (_atariList[fish3RawId] != atari3)
                    {
                        Report(
                            string.Format(
                                "'{0}'のアタリの強さは'{1}'で合っていますか?",
                                Translate.Instance[fish3NameId, "ja"],
                                atari3));
                    }
                }

                if (_atariList[targetFishRawId] != atari4)
                {
                    Report(
                        string.Format(
                            "'{0}'のアタリの強さは'{1}'で合っていますか?",
                            Translate.Instance[targetFishNameId, "ja"],
                            atari4));
                }

                if (!bestRoutes.Contains(
                        string.Join(
                        "⇒",
                        new[]
                        {
                            baitRawId,
                            fish1RawId ?? "*",
                            fish2RawId ?? "*",
                            fish3RawId ?? "*",
                            targetFishRawId
                        })))
                {
                    Report(
                        string.Format(
                            "'{0}'での'{1}'の釣り方は'{2}'で合っていますか?",
                            Translate.Instance[fishingSpotNameId, "ja"],
                            Translate.Instance[targetFishNameId, "ja"],
                            string.Join("; ", bestRoutes.Select(route => route.Replace("*⇒", "")))));
                }
            }
#endif
            var needMooching = false;
            var sbText = new StringBuilder();
            sbText.Append(baitName);
            if (hooking1 != null && fish1Name != null)
            {
                sbText.Append("⇒(");
                sbText.Append(hooking1);
                sbText.Append(") ");
                sbText.Append(fish1Name);
                needMooching = true;
            }
            if (hooking2 != null && fish2Name != null)
            {
                sbText.Append("⇒(");
                sbText.Append(hooking2);
                sbText.Append(") ");
                sbText.Append(fish2Name);
                needMooching = true;
            }
            if (hooking3 != null && fish3Name != null)
            {
                sbText.Append("⇒(");
                sbText.Append(hooking3);
                sbText.Append(") ");
                sbText.Append(fish3Name);
                needMooching = true;
            }
            sbText.Append("⇒(");
            sbText.Append(hooking4);
            sbText.Append(") ");
            sbText.Append(Translate.Instance[targetFishNameId, lang]);
#if DEBUG && false
            if (targetFishRawId == "シャリベネ")
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format(
                        "**SetNeedMooching: lang={0}, text='{1}', mooching={2}",
                        lang,
                        sourceText,
                        needMooching));
            }
#endif
            return new FishingMethod
            {
                TranslatedMethodText = sbText.ToString(),
                FishingBaitNameId = baitNameId,
                NeedMooching = needMooching,
            };
        }

        private static void Report(string text)
        {
#if false
            throw new Exception(text);
#else
            System.Diagnostics.Debug.WriteLine(text);
#endif

        }
    }
}
