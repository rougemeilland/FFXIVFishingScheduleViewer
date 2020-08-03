using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FFXIVFishingScheduleViewer
{
    class FishDataVerifier
    {
        private class AtariInfo
        {
            public string SpotRawId { get; set; }
            public string FishRawId { get; set; }
            public string Atari { get; set; }
        }

        private class RateInfo
        {
            public string FishRawId { get; set; }
            public string BaitRawId { get; set; }
            public int Order { get; set; }
            public long Numerator { get; set; }
            public long Denominator { get; set; }
            public double Rate => (double)Numerator / Denominator;
        }

        private class BaitRateInfo
        {
            public string BaitRawId { get; set; }
            public double Rate { get; set; }
        }

        private class BestRouteWork
        {
            public string BaitRawId { get; set; }
            public IEnumerable<string> Route { get; set; }
            public IEnumerable<double> Rates { get; set; }
            public double TotalRate { get; set; }
        }

        private class CastingCounters
        {
            public int CountOfCasting { get; set; }
            public int CountOfBait { get; set; }
            public double Rate { get; set; }
            public int CountOfSuccess { get; set; }
        }

        private class ExpectedValues
        {
            public string BaitRawId { get; set; }
            public string TargetFishRawId { get; set; }
            public double CountOfCasting { get; set; }
            public double CountOfBait { get; set; }
            public IEnumerable<IEnumerable<string>> Routes { get; set; }
        }

        private class 軽微な差異を無視する比較子
            : IComparer<double>
        {
            public int Compare(double x, double y)
            {
                if (x < 0 || y < 0)
                    throw new ArgumentException();
                if (x > y)
                {
                    return x > y * 1.01 ? 1 : 0;
                }
                else
                {
                    return x * 1.01 < y ? -1 : 0;
                }
            }
        }

        private const double _deltaRate = 0.0001; // 無視可能な確率の閾値
        private static Regex _atariPattern = new Regex(@"<tr[^>]*>\s*<td[^>]*>\s*</td>\s*<td[^>]*>\s*<a[^>]*>\s*<span[^>]*>\s*<img[^>]*>\s*</span>(?<name>[^<]*)</a>\s*</td>\s*<td[^>]*>\s*<span[^>]*>[^<]*</span>\s*</td>\s*<td[^>]*>\s*<a[^>]*>\s*<canvas[^>]*data-value='{""[0-9]+"":[0-9\.]*,""(?<value>[0-9]+)"":1,""[0-9]+"":[0-9\.]*}'[^>]*>", RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex _ratePattern = new Regex(@"<div[^>]*?title=""(?<fish>[^""]*?)&#10;釣果: *(?<bait>[^&]+)&#10;[0-9\.%]*?\((?<numerator>[0-9]+)/(?<denominator>[0-9]+)\)[^""]*?"".*?>", RegexOptions.Compiled | RegexOptions.Singleline);
        private static IDictionary<string, string> _魚のアタリ = new Dictionary<string, string>()
        {
            { "ソルター", "!" },
        };
        private static IDictionary<string, bool> _使用可能な釣り餌 = new Dictionary<string, bool>()
        {
            { "パラタの墓所@*@サスペンドミノー", false }, // 実際には適合する魚がいない
            { "青翠の奈落@*@モスプパ", false }, // 実際には適合する魚がいない
            { "ムーンドリップ洞窟@*@ユスリカ", false }, // 実際には適合する魚がいない
            { "青燐泉@*@バターワーム", false }, // 実際には適合する魚がいない
            { "クルザス不凍池@*@ドバミミズ", false }, // 実際には適合する魚がいない
            { "クルザス不凍池@*@ザザムシ", false }, // 実際には適合する魚がいない
            { "クリアプール@*@ドバミミズ", false }, // 実際には適合する魚がいない
            { "クリアプール@*@ザザムシ", false }, // 実際には適合する魚がいない
            { "唄う裂谷北部@*@バターワーム", false }, // 実際には適合する魚がいない
            { "雲溜まり@*@ザザムシ", false }, // 実際には適合する魚がいない
            { "流星の尾@*@ハニーワーム", false }, // 実際には適合する魚がいない
            { "ゼッキ島近海@*@ポーラークリル", false }, // 実際には適合する魚がいない
            { "七彩溝@*@ブルートリーチ", false }, // 実際には適合する魚がいない

            { "サマーフォード沿岸@*@ファットワーム", false }, // ファットワームはオーシャンフィッシング専用

            { "サプサ産卵地@*@バターワーム", false }, // バターワームは淡水用釣り餌
            { "船隠しの港@*@ハニーワーム", false }, // ハニーワームは淡水用釣り餌
            { "サゴリー砂丘@*@クロウフライ", false }, // クロウフライは淡水用釣り餌
            { "サゴリー砂海@*@シルバースプーン", false }, // シルバースプーンは淡水用釣り餌
            { "ウィッチドロップ@*@ブルートリーチ", false }, // ブルートリーチは淡水用釣り餌
            { "モック・ウーグル島@*@ツチグモ", false }, // ツチグモは淡水用釣り餌
            { "アジス・ラー旗艦島@*@ストーンラーヴァ", false }, // ストーンラーヴァは淡水用釣り餌
            { "アネス・ソー@*@ツチグモ", false }, // ツチグモは淡水用釣り餌
            { "光輪の祭壇@*@カディスラーヴァ", false }, // カディスラーヴァは淡水用釣り餌
            { "光輪の祭壇@*@ツチグモ", false }, // ツチグモは淡水用釣り餌
            { "光輪の祭壇@*@ブルートリーチ", false }, // ブルートリーチは淡水用釣り餌
            { "光輪の祭壇@*@サスペンドミノー", false }, // サスペンドミノーは淡水用釣り餌
            { "サルウーム・カシュ@*@ツチグモ", false }, // ツチグモは淡水用釣り餌
            { "ロッホ・セル湖@*@ドバミミズ", false }, // ドバミミズは淡水用釣り餌
            { "ロッホ・セル湖@*@サスペンドミノー", false }, // サスペンドミノーは淡水用釣り餌
            { "オノコロ島近海@*@スピナーベイト", false }, // スピナーベイトは淡水用釣り餌
            { "ゼッキ島近海@*@ドバミミズ", false }, // ドバミミズは淡水用釣り餌
            { "ゼッキ島近海@*@ザザムシ", false }, // ザザムシは淡水用釣り餌
            { "波止場全体@*@ドバミミズ", false }, // ドバミミズは淡水用釣り餌

            { "ハウケタ御用邸@*@スプーンワーム", false }, // スプーンワームは海釣り用釣り餌
            { "ゴブリン族の生簀@*@スプーンワーム", false }, // スプーンワームは海釣り用釣り餌
            { "ラベンダーベッド@*@シュリンプフィーダー", false }, // シュリンプフィーダーは海釣り用釣り餌
            { "クリアプール@*@アオイソメ", false }, // アオイソメは海釣り用釣り餌
            { "パプスの大樹@*@ポーラークリル", false }, // ポーラークリルは海釣り用釣り餌
            { "ティモン川@*@ラグワーム", false }, // ポーラークリルは海釣り用釣り餌
            { "ラールガーズリーチ@*@活海老", false }, // 活海老は海釣り用釣り餌
            { "スロウウォッシュ@*@活海老", false }, // 活海老は海釣り用釣り餌
            { "ロッホ・セル湖@*@アオイソメ", false }, // アオイソメは海釣り用釣り餌
            { "七彩溝@*@ショートビルミノー", false }, // ショートビルミノーは海釣り用釣り餌
            { "七彩溝@*@アオイソメ", false },  // アオイソメは海釣り用釣り餌
            { "ウォーヴンオウス@*@モエビ", false },  // モエビは海釣り用釣り餌

            { "クリアプール@*@バイオレットワーム", false }, // バイオレットワームは魔泉釣り用釣り餌

            { "アルファ管区@*@マグマワーム", false }, // マグマワームは溶岩釣り用釣り餌
            { "超星間交信塔@*@マグマワーム", false }, // マグマワームは溶岩釣り用釣り餌
            { "アジス・ラー旗艦島@*@マグマワーム", false }, // マグマワームは溶岩釣り用釣り餌

            { "アルファ管区@*@ジャンボガガンボ", false }, // ジャンボガガンボは雲海釣りまたは浮島釣り用釣り餌
            { "廃液溜まり@*@ジャンボガガンボ", false }, // ジャンボガガンボは雲海釣りまたは浮島釣り用釣り餌
            { "アジス・ラー旗艦島@*@ジャンボガガンボ", false }, // ジャンボガガンボは雲海釣りまたは浮島釣り用釣り餌
            { "光輪の祭壇@*@ジャンボガガンボ", false }, // ジャンボガガンボは雲海釣りまたは浮島釣り用釣り餌
            { "ラールガーズリーチ@*@ジャンボガガンボ", false }, // ジャンボガガンボは雲海釣りまたは浮島釣り用釣り餌
            { "アジム・カート@*@ジャンボガガンボ", false }, // ジャンボガガンボは雲海釣りまたは浮島釣り用釣り餌

            { "七彩溝@*@蚕蛹", false }, // 蚕蛹は塩湖用釣り餌

            { "*@*@淡水万能餌", false }, // 万能餌は特定の魚を釣りたい場合にはあまり向かない
            { "*@*@海水万能餌", false }, // 万能餌は特定の魚を釣りたい場合にはあまり向かない
            { "*@*@万能ルアー", false }, // 万能餌は特定の魚を釣りたい場合にはあまり向かない
            { "*@*@メタルスピナー", false }, // お得意様納品用の魚の釣り餌。
        };

        /// <summary>
        /// 魚に関するデータを検証するためのコードを生成する。
        /// </summary>
        /// <param name="spots">
        /// すべての釣り場の情報を持っている<see cref="FishingSpotCollection"/>オブジェクト。
        /// </param>
        public void GenerateCode(FishingSpotCollection spots)
        {
#if false
            var dir = Path.GetDirectoryName(GetType().Assembly.Location);
            CreateAtariList(spots, dir);
            CreateBaitList(spots, dir);
            System.Diagnostics.Debug.WriteLine("Complete verification.");
#endif
        }

        /// <summary>
        /// 魚のアタリ情報をファイルに出力する。
        /// </summary>
        /// <param name="spots">
        /// すべての釣り場の情報を持っている<see cref="FishingSpotCollection"/>オブジェクト。
        /// </param>
        /// <param name="dir">
        /// 出力先のディレクトリ。
        /// </param>
        private void CreateAtariList(FishingSpotCollection spots, string dir)
        {
            var atariInfoList =
                spots
                .Select(spot =>
                {
                    var spotRawId = ((IGameDataObject)spot).InternalId;
                    return new
                    {
                        document =
                            spot.DownloadCBHPage(
                                new FileInfo(
                                    Path.Combine(
                                        dir,
                                        string.Format(
                                            "__cache-{0}.txt",
                                            EncodeFileName(spotRawId))))).Result,
                        spotRawId,
                    };
                })
                .SelectMany(item => ParseAtari(item.spotRawId, item.document))
                .GroupBy(item => item.FishRawId) // アタリの情報を魚毎にまとめる
                .Select(g =>
                {
                    string defaultAtari;
                    if (_魚のアタリ.TryGetValue(g.Key, out defaultAtari))
                    {
                        // その魚のアタリの情報がデフォルトに存在するならそちらを優先する(データが誤っているなどの場合)
                        return new { fishName = g.Key, atari = defaultAtari };
                    }
                    else
                    {
                        // その魚のアタリの情報がデフォルトに存在しない場合
                        // 集計されたデータからアタリの情報がすべて同一かどうかを調べる
                        var atari = g.Select(item => item.Atari).Distinct().ToArray();
                        if (atari.Length != 1)
                        {
                            // 異なる複数のアタリの情報が存在する場合
                            throw new Exception();
                        }
                        //その魚のアタリの情報を返す
                        return new { fishName = g.Key, atari = atari[0] };
                    }
                });
            // 結果をファイルに出力する
            var outputPath = Path.Combine(dir, "atari_cs.txt");
            using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                writer.WriteLine("#region 魚のアタリ");
                foreach (var atariInfo in atariInfoList)
                    writer.WriteLine(string.Format("{0}new {{ fish = \"{1}\", atari = \"{2}\" }},", new string(' ', 20), atariInfo.fishName, atariInfo.atari));
                writer.WriteLine("#endregion");
            }
        }

        // 
        /// <summary>
        /// ドキュメントを解析し、アタリの数値データを弱震/強震/激震に分類する。
        /// </summary>
        /// <param name="doc">
        /// ドキュメントの内容の文字列
        /// </param>
        /// <returns>
        /// 魚のアタリの情報を持つ<see cref="AtariInfo"/>オブジェクト。
        /// </returns>
        private IEnumerable<AtariInfo> ParseAtari(string spotRawId, string doc)
        {
            return
                _atariPattern.Matches(doc)
                .Cast<Match>()
                .Select(m =>
                {
                    string atari;
                    switch (m.Groups["value"].Value)
                    {
                        case "6": // 弱震
                        case "7": // 弱震
                            atari = "!";
                            break;
                        case "8": // 強震
                        case "9": // 強震
                        case "10": // 強震
                            atari = "!!";
                            break;
                        case "11": // 激震
                        case "12": // 激震
                            atari = "!!!";
                            break;
                        default:
                            throw new Exception();
                    }
                    return new AtariInfo
                    {
                        SpotRawId = spotRawId,
                        FishRawId = m.Groups["name"].Value.Trim(),
                        Atari = atari,
                    };
                });
        }

        /// <summary>
        /// 魚毎の最適な餌と釣り方をファイルに出力する。
        /// </summary>
        /// <param name="spots">
        /// すべての釣り場の情報を持っている<see cref="FishingSpotCollection"/>オブジェクト。
        /// </param>
        /// <param name="dir">
        /// 出力先のディレクトリ。
        /// </param>
        private void CreateBaitList(FishingSpotCollection spots, string dir)
        {
            var comparer = new 軽微な差異を無視する比較子();
            var outputPath = Path.Combine(dir, "bait_cs.txt");
            using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                writer.WriteLine("#region 魚の釣り方");
                foreach (var spot in spots)
                {
                    var spotRawId = ((IGameDataObject)spot).InternalId;
                    // ドキュメントを取得する
                    var cacheFilePath = Path.Combine(dir, string.Format("__cache-{0}.txt", EncodeFileName(spotRawId)));
                    var doc = spot.DownloadCBHPage(new FileInfo(cacheFilePath)).Result;
                    // ドキュメントに含まれている釣果情報を抽出する
                    var rateInfoList =
                        ParseRate(doc);
                    // 魚毎の、その魚を釣るための直接的な釣り餌を Dictionary にまとめる。
                    var indexedBaitInfoList =
                        rateInfoList
                        .Where(rate => CheckBait(spotRawId, rate.FishRawId, rate.BaitRawId) != false)
                        .GroupBy(rate => rate.FishRawId)
                        .Select(g => new
                        {
                            fishraiId = g.Key,
                            elements =
                                g.Select(item => new BaitRateInfo
                                {
                                    BaitRawId = item.BaitRawId,
                                    Rate = item.Rate,
                                })
                                .ToList()
                                .AsEnumerable(),
                        })
                        .ToDictionary(item => item.fishraiId, item => item.elements);
                    // その釣り場から釣れる魚を列挙する
                    foreach (var targetFishRawId in indexedBaitInfoList.Keys)
                    {
                        // 最終的にその魚を釣ることができる釣り餌のうち、必要なキャスティング回数(≒所要時間)が少ないものを一つ選ぶ
                        var found =
                            EnumerateBaitRoute(indexedBaitInfoList, new[] { targetFishRawId }, new double[0], 1.0, targetFishRawId)
                            .GroupBy(item => item.BaitRawId)
                            .Select(g => GetCounters(g.Select(item => item.Route).ToArray(), g.Select(item => item.Rates).ToArray())) // 必要なキャスティング回数などの期待値を計算する
                            .OrderBy(item => item.CountOfCasting, comparer)
                            .FirstOrDefault();
                        // 選ばれた釣り餌と釣り方をファイルに出力する
                        writer.WriteLine(
                            string.Format(
                                "{0}new {{ spot = \"{1}\", fish = \"{2}\", bestBait = \"{3}\", routes = new[] {{ {4} }}, countOfBait = {5:F2}, countOfCasting = {6:F2} }},",
                                new string(' ', 20),
                                spotRawId,
                                found.TargetFishRawId,
                                found.BaitRawId,
                                string.Join(
                                    ", ",
                                    found.Routes
                                        .Select(route => new { bestRoute = route, lengthOfRoute = route.Count(), key = string.Join("**", route) })
                                        .Where(item => item.lengthOfRoute == item.bestRoute.Distinct().Count()) // 同じ魚が複数回出現するルート(泳がせ釣りの共食いなどの場合)は表示結果からは省略する
                                        .OrderBy(item => item.lengthOfRoute) // ルートの表示順序を一意に定めるための順序付け
                                        .ThenBy(item => item.key)
                                        .Select(item =>
                                            string.Format(
                                                "new[] {{ {0} }}",
                                                string.Join(
                                                    ", ",
                                                    item.bestRoute
                                                        .Select(id => string.Format("\"{0}\"", id)))))),
                                found.CountOfBait,
                                found.CountOfCasting));
                    }
                }
                writer.WriteLine("#endregion");
            }
        }

        private static bool? CheckBait(string spotRawId, string fishRawId, string baitRawId)
        {
            bool value;
            var key1 = string.Format("{0}@{1}@{2}", "*", "*", baitRawId);
            if (_使用可能な釣り餌.TryGetValue(key1, out value))
                return value;
            if (fishRawId != null)
            {
                var key2 = string.Format("{0}@{1}@{2}", "*", fishRawId, baitRawId);
                if (_使用可能な釣り餌.TryGetValue(key2, out value))
                    return value;
            }
            var key3 = string.Format("{0}@{1}@{2}", spotRawId, "*", baitRawId);
            if (_使用可能な釣り餌.TryGetValue(key3, out value))
                return value;
            if (fishRawId != null)
            {
                var key4 = string.Format("{0}@{1}@{2}", spotRawId, fishRawId, baitRawId);
                if (_使用可能な釣り餌.TryGetValue(key4, out value))
                    return value;
            }
            return null;
        }

        private static IEnumerable<BestRouteWork> EnumerateBaitRoute(IDictionary<string, IEnumerable<BaitRateInfo>> indexedBaitInfoList, IEnumerable<string> currentRoute, IEnumerable<double> currentRates, double totalRate, string targetFishRawId)
        {
            if (totalRate < _deltaRate)
            {
                // このルートに一致する確率が非常に低い場合
                // このルートは続行を中断して空で復帰する
                return new BestRouteWork[0];
            }
            else
            {
                var currentPosition = currentRoute.First();
                if (currentPosition.IsFishingBaitRawId())
                {
                    // ルートの先頭が釣り餌ならば探索は完了している
                    return
                        new[]
                        {
                            new BestRouteWork
                            {
                                BaitRawId = currentPosition,
                                Route = currentRoute.ToList(),
                                TotalRate = totalRate,
                                Rates = currentRates.ToList(),
                            }
                        };
                }
                else if (currentPosition.IsFishRawId())
                {
                    // ルートの先頭が魚の場合は、次のノード候補を探す
                    IEnumerable<BaitRateInfo> nextValues;
                    if (!indexedBaitInfoList.TryGetValue(currentPosition, out nextValues))
                    {
                        // currentPosition を釣るための釣り餌または魚が存在しない場合
                        // このルートは続行不可能なので、空で復帰する
                        return new BestRouteWork[0];
                    }
                    // 次のノードへ探索を続行する
                    // だたし、targetFishRawId は常に検索対象から除外する
                    return
                        nextValues
                            .Where(nextValue => nextValue.BaitRawId != targetFishRawId)
                            .SelectMany(nextValue =>
                                EnumerateBaitRoute(
                                    indexedBaitInfoList,
                                    new[] { nextValue.BaitRawId }.Concat(currentRoute),
                                    new[] { nextValue.Rate }.Concat(currentRates),
                                    nextValue.Rate * totalRate,
                                    targetFishRawId));

                }
                else
                    throw new Exception(string.Format("'{0}'は不明なIDです。", currentPosition));
            }
        }

        private ExpectedValues GetCounters(IEnumerable<IEnumerable<string>> routeList, IEnumerable<IEnumerable<double>> rateList)
        {
            var baitRawId = routeList.Select(route => route.First()).Distinct().Single();
            var targetFishRawId = routeList.Select(route => route.Last()).Distinct().Single();
            IDictionary<string, IDictionary<string, double>> index = new Dictionary<string, IDictionary<string, double>>();
            foreach (var item in routeList.Zip(rateList, (route, rates) => new { route, rates }))
            {
                var route = item.route.Skip(1).ToList();
                var rates = item.rates;
                if (route.Count() != rates.Count())
                    throw new Exception();
                var key = "--";
                foreach (var item2 in route.Zip(rates, (id, rate) => new { id, rate }))
                {
                    var id = item2.id == targetFishRawId ? "++" : item2.id;
                    var rate = item2.rate;
                    IDictionary<string, double> index2;
                    if (index.TryGetValue(key, out index2))
                    {
                        double rate0;
                        if (index2.TryGetValue(id, out rate0))
                        {
                            if (rate0 != rate)
                                throw new Exception();
                        }
                        else
                            index2.Add(id, rate);
                    }
                    else
                    {
                        index2 = new Dictionary<string, double>();
                        index2.Add(id, rate);
                        index.Add(key, index2);
                    }
                    key = key + "⇒" + id;
                }
            }
            if (index.Keys.Where(s => s == "--").Take(2).Count() != 1)
                throw new Exception();
            var q = GetCounters(index, "--", 1.0, 1, 0);
            var fishCountPerBait = q.Sum(item => item.Rate * item.CountOfSuccess);
            var baitCountPerBait = q.Sum(item => item.Rate * item.CountOfBait);
            var castingCountPerBait = q.Sum(item => item.Rate * item.CountOfCasting);
            return new ExpectedValues
            {
                BaitRawId = baitRawId,
                Routes = routeList,
                TargetFishRawId = targetFishRawId,
                CountOfBait = baitCountPerBait / fishCountPerBait,
                CountOfCasting = castingCountPerBait / fishCountPerBait,
            };
        }

        private IEnumerable<CastingCounters> GetCounters(IDictionary<string, IDictionary<string, double>> index, string key, double totalRate, int countOfBait, int countOfCasting)
        {
            if (key.EndsWith("++"))
            {
                // ゴールに達している場合
                return
                    new[]
                    {
                        new CastingCounters
                        {
                             Rate = totalRate,
                             CountOfBait = countOfBait,
                             CountOfCasting = countOfCasting,
                             CountOfSuccess = 1,
                        }
                    };
            }
            else
            {
                IDictionary<string, double> idRate;
                if (!index.TryGetValue(key, out idRate))
                    throw new Exception();
                var 次に進めることができる確率 = idRate.Sum(item => item.Value);
                if (次に進めることができる確率 < 0.0 || 次に進めることができる確率 > 1.0)
                    throw new Exception();
                var 失敗する確率 = 1.0 - 次に進めることができる確率;
                if (失敗する確率 < 0.0 || 失敗する確率 > 1.0)
                    throw new Exception();
                return
                    idRate
                        .SelectMany(keyValue =>
                            GetCounters(
                                index,
                                key + "⇒" + keyValue.Key,
                                totalRate * keyValue.Value,
                                countOfBait,
                                countOfCasting + 1))
                        .Concat(new[]
                        {
                            new CastingCounters
                            {
                                Rate = totalRate * 失敗する確率,
                                CountOfSuccess = 0,
                                CountOfBait = countOfBait,
                                CountOfCasting = countOfCasting + 1,
                            }
                        });
            }
        }



        private IEnumerable<RateInfo> ParseRate(string doc)
        {

            return
                _ratePattern.Matches(doc)
                .Cast<Match>()
                .Zip(Enumerable.Range(0, int.MaxValue), (m, index) => new { m, index })
                .Select(item =>
                {
                    return new RateInfo
                    {
                        FishRawId = item.m.Groups["fish"].Value.Trim(),
                        BaitRawId = item.m.Groups["bait"].Value.Trim(),
                        Order = item.index,
                        Numerator = int.Parse(item.m.Groups["numerator"].Value),
                        Denominator = int.Parse(item.m.Groups["denominator"].Value),
                    };
                })
                .ToList();
        }

        private static string EncodeFileName(string fileName)
        {
            return
                Regex.Replace(
                    fileName,
                    @"[\\/:\*\?""<>\|]",
                    m =>
                    {
                        switch (m.Value)
                        {
                            case "\\":
                                return "＼";
                            case "/":
                                return "／";
                            case ":":
                                return "：";
                            case "*":
                                return "＊";
                            case "?":
                                return "？";
                            case "\"":
                                return "”";
                            case "<":
                                return "＜";
                            case ">":
                                return "＞";
                            case "|":
                                return "｜";
                            default:
                                return m.Value;
                        }
                    });
        }
    }
}
