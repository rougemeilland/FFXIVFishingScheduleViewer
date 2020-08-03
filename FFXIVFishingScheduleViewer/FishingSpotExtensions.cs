﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FFXIVFishingScheduleViewer
{
    static class FishingSpotExtensions
    {
        private static IDictionary<string, string> _pageIdSourceOfCBH;

        static FishingSpotExtensions()
        {
            _pageIdSourceOfCBH =
                new[]
                {
                    new { spotId = "リムサ・ロミンサ：上甲板層", pageId = "10101" },
                    new { spotId = "リムサ・ロミンサ：下甲板層", pageId = "10201" },
                    new { spotId = "ゼファードリフト沿岸", pageId = "10301" },
                    new { spotId = "サマーフォード沿岸", pageId = "10302" },
                    new { spotId = "ローグ川", pageId = "10303" },
                    new { spotId = "西アジェレス川", pageId = "10304" },
                    new { spotId = "ニーム川", pageId = "10305" },
                    new { spotId = "ささやきの谷", pageId = "10306" },
                    new { spotId = "モーニングウィドー", pageId = "10401" },
                    new { spotId = "モラビー湾西岸", pageId = "10402" },
                    new { spotId = "シダーウッド沿岸部", pageId = "10403" },
                    new { spotId = "モラビー造船廠", pageId = "10404" },
                    new { spotId = "オシュオン灯台", pageId = "10405" },
                    new { spotId = "ソルトストランド", pageId = "10406" },
                    new { spotId = "キャンドルキープ埠頭", pageId = "10407" },
                    new { spotId = "エンプティハート", pageId = "10408" },
                    new { spotId = "ブラインドアイアン坑道", pageId = "10409" },
                    new { spotId = "南ブラッドショア", pageId = "10501" },
                    new { spotId = "コスタ・デル・ソル", pageId = "10502" },
                    new { spotId = "北ブラッドショア", pageId = "10503" },
                    new { spotId = "隠れ滝", pageId = "10504" },
                    new { spotId = "東アジェレス川", pageId = "10505" },
                    new { spotId = "レインキャッチャー樹林", pageId = "10506" },
                    new { spotId = "レインキャッチャー沼沢地", pageId = "10507" },
                    new { spotId = "レッドマンティス滝", pageId = "10508" },
                    new { spotId = "ロータノ海沖合：船首", pageId = "10509" },
                    new { spotId = "ロータノ海沖合：船尾", pageId = "10510" },
                    new { spotId = "常夏の島北", pageId = "10511" },
                    new { spotId = "スウィフトパーチ入植地", pageId = "10601" },
                    new { spotId = "スカルバレー沿岸部", pageId = "10602" },
                    new { spotId = "ハーフストーン沿岸部", pageId = "10603" },
                    new { spotId = "幻影諸島北岸", pageId = "10604" },
                    new { spotId = "幻影諸島南岸", pageId = "10605" },
                    new { spotId = "ブルワーズ灯台", pageId = "10606" },
                    new { spotId = "船の墓場", pageId = "10607" },
                    new { spotId = "サプサ産卵地", pageId = "10608" },
                    new { spotId = "船隠しの港", pageId = "10609" },
                    new { spotId = "オークウッド", pageId = "10701" },
                    new { spotId = "愚か者の滝", pageId = "10702" },
                    new { spotId = "ブロンズレイク北東岸", pageId = "10703" },
                    new { spotId = "ブロンズレイク・シャロー", pageId = "10704" },
                    new { spotId = "ロングクライム渓谷", pageId = "10801" },
                    new { spotId = "ブロンズレイク北西岸", pageId = "10802" },
                    new { spotId = "ミスト・ヴィレッジ", pageId = "10901" },
                    new { spotId = "グリダニア：翡翠湖畔", pageId = "20101" },
                    new { spotId = "グリダニア：紅茶川水系下流", pageId = "20102" },
                    new { spotId = "グリダニア：囁きの渓谷", pageId = "20201" },
                    new { spotId = "グリダニア：紅茶川水系上流", pageId = "20202" },
                    new { spotId = "葉脈水系", pageId = "20301" },
                    new { spotId = "鏡池", pageId = "20302" },
                    new { spotId = "エバーシェイド", pageId = "20303" },
                    new { spotId = "芽吹の池", pageId = "20304" },
                    new { spotId = "ハウケタ御用邸", pageId = "20305" },
                    new { spotId = "花蜜桟橋", pageId = "20401" },
                    new { spotId = "青翠の奈落", pageId = "20402" },
                    new { spotId = "さざなみ小川", pageId = "20403" },
                    new { spotId = "シルフランド渓谷", pageId = "20404" },
                    new { spotId = "十二神大聖堂", pageId = "20405" },
                    new { spotId = "ハズーバ支流：上流", pageId = "20501" },
                    new { spotId = "ハズーバ支流：中流", pageId = "20502" },
                    new { spotId = "ハズーバ支流：下流", pageId = "20503" },
                    new { spotId = "ハズーバ支流：東", pageId = "20504" },
                    new { spotId = "ゴブリン族の生簀", pageId = "20505" },
                    new { spotId = "根渡り沼", pageId = "20506" },
                    new { spotId = "ウルズの恵み", pageId = "20507" },
                    new { spotId = "さざめき川", pageId = "20601" },
                    new { spotId = "フォールゴウド秋瓜湖畔", pageId = "20602" },
                    new { spotId = "プラウドクリーク", pageId = "20603" },
                    new { spotId = "タホトトル湖畔", pageId = "20604" },
                    new { spotId = "ラベンダーベッド", pageId = "20701" },
                    new { spotId = "シルバーバザー", pageId = "30101" },
                    new { spotId = "ベスパーベイ", pageId = "30102" },
                    new { spotId = "クレセントコーヴ", pageId = "30103" },
                    new { spotId = "ノフィカの井戸", pageId = "30104" },
                    new { spotId = "足跡の谷", pageId = "30105" },
                    new { spotId = "ウエストウインド岬", pageId = "30106" },
                    new { spotId = "パラタの墓所", pageId = "30107" },
                    new { spotId = "ムーンドリップ洞窟", pageId = "30108" },
                    new { spotId = "スートクリーク上流", pageId = "30201" },
                    new { spotId = "スートクリーク下流", pageId = "30202" },
                    new { spotId = "アンホーリーエアー", pageId = "30203" },
                    new { spotId = "クラッチ狭間", pageId = "30204" },
                    new { spotId = "ドライボーン北湧水地", pageId = "30301" },
                    new { spotId = "ドライボーン南湧水地", pageId = "30302" },
                    new { spotId = "ユグラム川", pageId = "30303" },
                    new { spotId = "バーニングウォール", pageId = "30304" },
                    new { spotId = "リザードクリーク", pageId = "30401" },
                    new { spotId = "ザハラクの湧水", pageId = "30402" },
                    new { spotId = "忘れられたオアシス", pageId = "30403" },
                    new { spotId = "サゴリー砂海", pageId = "30404" },
                    new { spotId = "サゴリー砂丘", pageId = "30405" },
                    new { spotId = "青燐泉", pageId = "30501" },
                    new { spotId = "ブルーフォグ湧水地", pageId = "30502" },
                    new { spotId = "ゴブレットビュート", pageId = "30601" },
                    new { spotId = "クルザス川", pageId = "40101" },
                    new { spotId = "ウィッチドロップ", pageId = "40102" },
                    new { spotId = "剣ヶ峰山麓", pageId = "40103" },
                    new { spotId = "聖ダナフェンの落涙", pageId = "40104" },
                    new { spotId = "キャンプ・ドラゴンヘッド溜池", pageId = "40105" },
                    new { spotId = "聖ダナフェンの旅程", pageId = "40106" },
                    new { spotId = "調査隊の氷穴", pageId = "40107" },
                    new { spotId = "スノークローク大氷壁", pageId = "40108" },
                    new { spotId = "イシュガルド大雲海", pageId = "40109" },
                    new { spotId = "リバーズミート", pageId = "40201" },
                    new { spotId = "グレイテール滝", pageId = "40202" },
                    new { spotId = "クルザス不凍池", pageId = "40203" },
                    new { spotId = "クリアプール", pageId = "40204" },
                    new { spotId = "ドラゴンスピット", pageId = "40205" },
                    new { spotId = "ベーンプール南", pageId = "40206" },
                    new { spotId = "アッシュプール", pageId = "40207" },
                    new { spotId = "ベーンプール西", pageId = "40208" },
                    new { spotId = "銀泪湖北岸", pageId = "50101" },
                    new { spotId = "早霜峠", pageId = "50102" },
                    new { spotId = "タングル湿林", pageId = "50103" },
                    new { spotId = "タングル湿林源流", pageId = "50104" },
                    new { spotId = "唄う裂谷", pageId = "50105" },
                    new { spotId = "唄う裂谷北部", pageId = "50106" },
                    new { spotId = "ヴール・シアンシラン", pageId = "60101" },
                    new { spotId = "雲溜まり", pageId = "60102" },
                    new { spotId = "クラウドトップ", pageId = "60103" },
                    new { spotId = "ブルーウィンドウ", pageId = "60104" },
                    new { spotId = "モック・ウーグル島", pageId = "60105" },
                    new { spotId = "アルファ管区", pageId = "60201" },
                    new { spotId = "廃液溜まり", pageId = "60202" },
                    new { spotId = "超星間交信塔", pageId = "60203" },
                    new { spotId = "デルタ管区", pageId = "60204" },
                    new { spotId = "パプスの大樹", pageId = "60205" },
                    new { spotId = "ハビスフィア", pageId = "60206" },
                    new { spotId = "アジス・ラー旗艦島", pageId = "60207" },
                    new { spotId = "悲嘆の飛泉", pageId = "70101" },
                    new { spotId = "ウィロームリバー", pageId = "70102" },
                    new { spotId = "スモーキングウェイスト", pageId = "70103" },
                    new { spotId = "餌食の台地", pageId = "70104" },
                    new { spotId = "モーン大岩窟", pageId = "70105" },
                    new { spotId = "モーン大岩窟西", pageId = "70106" },
                    new { spotId = "アネス・ソー", pageId = "70107" },
                    new { spotId = "光輪の祭壇", pageId = "70108" },
                    new { spotId = "サリャク河", pageId = "70201" },
                    new { spotId = "クイックスピル・デルタ", pageId = "70202" },
                    new { spotId = "サリャク河上流", pageId = "70203" },
                    new { spotId = "サリャク河中州", pageId = "70204" },
                    new { spotId = "エイル・トーム", pageId = "70301" },
                    new { spotId = "グリーンスウォード島", pageId = "70302" },
                    new { spotId = "ウェストン・ウォーター", pageId = "70303" },
                    new { spotId = "ランドロード遺構", pageId = "70304" },
                    new { spotId = "ソーム・アル笠雲", pageId = "70305" },
                    new { spotId = "サルウーム・カシュ", pageId = "70306" },
                    new { spotId = "ミラージュクリーク上流", pageId = "90101" },
                    new { spotId = "ラールガーズリーチ", pageId = "90102" },
                    new { spotId = "星導山寺院入口", pageId = "90103" },
                    new { spotId = "ティモン川", pageId = "90201" },
                    new { spotId = "夜の森", pageId = "90202" },
                    new { spotId = "流星の尾", pageId = "90203" },
                    new { spotId = "ベロジナ川", pageId = "90204" },
                    new { spotId = "ミラージュクリーク", pageId = "90205" },
                    new { spotId = "夫婦池", pageId = "90301" },
                    new { spotId = "スロウウォッシュ", pageId = "90302" },
                    new { spotId = "ヒース滝", pageId = "90303" },
                    new { spotId = "裁定者の像", pageId = "90304" },
                    new { spotId = "ブルズバス", pageId = "90305" },
                    new { spotId = "アームズ・オブ・ミード", pageId = "90306" },
                    new { spotId = "ロッホ・セル湖", pageId = "90401" },
                    new { spotId = "ロッホ・セル湖底北西", pageId = "90451" },
                    new { spotId = "ロッホ・セル湖底中央", pageId = "90452" },
                    new { spotId = "ロッホ・セル湖底南東", pageId = "90453" },
                    new { spotId = "未知の漁場 (ギラバニア湖畔地帯)", pageId = "90491" },
                    new { spotId = "紅玉台場近海", pageId = "100101" },
                    new { spotId = "獄之蓋近海", pageId = "100102" },
                    new { spotId = "ベッコウ島近海", pageId = "100103" },
                    new { spotId = "沖之岩近海", pageId = "100104" },
                    new { spotId = "オノコロ島近海", pageId = "100105" },
                    new { spotId = "イサリ村沿岸", pageId = "100106" },
                    new { spotId = "ゼッキ島近海", pageId = "100107" },
                    new { spotId = "紅玉台場周辺", pageId = "100151" },
                    new { spotId = "碧のタマミズ周辺", pageId = "100152" },
                    new { spotId = "スイの里周辺", pageId = "100153" },
                    new { spotId = "アドヴェンチャー号周辺", pageId = "100154" },
                    new { spotId = "紫水宮周辺", pageId = "100155" },
                    new { spotId = "小林丸周辺", pageId = "100156" },
                    new { spotId = "未知の漁場 (紅玉海:アカククリ×10)", pageId = "100191" },
                    new { spotId = "未知の漁場 (紅玉海:イチモンジ×10)", pageId = "100192" },
                    new { spotId = "アオサギ池", pageId = "100201" },
                    new { spotId = "アオサギ川", pageId = "100202" },
                    new { spotId = "ナマイ村溜池", pageId = "100203" },
                    new { spotId = "七彩溝", pageId = "100204" },
                    new { spotId = "七彩渓谷", pageId = "100205" },
                    new { spotId = "ドマ城前", pageId = "100206" },
                    new { spotId = "城下船場", pageId = "100207" },
                    new { spotId = "無二江東", pageId = "100208" },
                    new { spotId = "無二江西", pageId = "100209" },
                    new { spotId = "梅泉郷", pageId = "100210" },
                    new { spotId = "無二江川底南西", pageId = "100251" },
                    new { spotId = "無二江川底南", pageId = "100252" },
                    new { spotId = "高速魔導駆逐艇L‐XXIII周辺", pageId = "100253" },
                    new { spotId = "沈没川船周辺", pageId = "100254" },
                    new { spotId = "大龍瀑川底", pageId = "100255" },
                    new { spotId = "未知の漁場 (ヤンサ)", pageId = "100291" },
                    new { spotId = "ネム・カール", pageId = "100301" },
                    new { spotId = "ハク・カール", pageId = "100302" },
                    new { spotId = "ヤト・カール上流", pageId = "100303" },
                    new { spotId = "アジム・カート", pageId = "100304" },
                    new { spotId = "タオ・カール", pageId = "100305" },
                    new { spotId = "ヤト・カール下流", pageId = "100306" },
                    new { spotId = "ドタール・カー", pageId = "100307" },
                    new { spotId = "アジム・カート湖底西", pageId = "100351" },
                    new { spotId = "アジム・カート湖底東", pageId = "100352" },
                    new { spotId = "未知の漁場 (アジムステップ)", pageId = "100391" },
                    new { spotId = "ドマ町人地", pageId = "100401" },
                    new { spotId = "波止場全体", pageId = "110101" },
                    new { spotId = "シロガネ", pageId = "110201" },
                    new { spotId = "シロガネ水路", pageId = "110202" },
                    new { spotId = "三学科の座", pageId = "120101" },
                    new { spotId = "四学科の座", pageId = "120102" },
                    new { spotId = "クリスタリウム居室", pageId = "120103" },
                    new { spotId = "廃船街", pageId = "120201" },
                    new { spotId = "風化した裂け目", pageId = "120301" },
                    new { spotId = "錆ついた貯水池", pageId = "120302" },
                    new { spotId = "始まりの湖", pageId = "120303" },
                    new { spotId = "サレン郷", pageId = "120304" },
                    new { spotId = "ケンの島 (釣り)", pageId = "120305" },
                    new { spotId = "始まりの湖北東", pageId = "120351" },
                    new { spotId = "ケンの島 (銛)", pageId = "120352" },
                    new { spotId = "始まりの湖南東", pageId = "120353" },
                    new { spotId = "未知の漁場 (レイクランド)", pageId = "120391" },
                    new { spotId = "ワッツリバー上流", pageId = "120401" },
                    new { spotId = "ホワイトオイルフォールズ", pageId = "120402" },
                    new { spotId = "ワッツリバー下流", pageId = "120403" },
                    new { spotId = "シャープタンの泉", pageId = "120404" },
                    new { spotId = "コルシア島沿岸西", pageId = "120405" },
                    new { spotId = "シーゲイザーの入江", pageId = "120406" },
                    new { spotId = "コルシア島沿岸東", pageId = "120407" },
                    new { spotId = "砂の川", pageId = "120501" },
                    new { spotId = "ナバースの断絶", pageId = "120502" },
                    new { spotId = "アンバーヒル", pageId = "120503" },
                    new { spotId = "手鏡の湖", pageId = "120601" },
                    new { spotId = "姿見の湖", pageId = "120602" },
                    new { spotId = "上の子らの流れ", pageId = "120603" },
                    new { spotId = "中の子らの流れ", pageId = "120604" },
                    new { spotId = "末の子らの流れ", pageId = "120605" },
                    new { spotId = "聖ファスリクの額", pageId = "120606" },
                    new { spotId = "コラードの排水溝", pageId = "120607" },
                    new { spotId = "リェー・ギア城北", pageId = "120651" },
                    new { spotId = "魚たちの街", pageId = "120652" },
                    new { spotId = "姿見の湖中央", pageId = "120653" },
                    new { spotId = "ジズム・ラーン", pageId = "120654" },
                    new { spotId = "姿見の湖南", pageId = "120655" },
                    new { spotId = "未知の漁場 (イル・メグ)", pageId = "120691" },
                    new { spotId = "トゥシ・メキタ湖", pageId = "120701" },
                    new { spotId = "血の酒坏", pageId = "120702" },
                    new { spotId = "ロツァトル川", pageId = "120703" },
                    new { spotId = "ミュリルの郷愁南", pageId = "120704" },
                    new { spotId = "ウォーヴンオウス", pageId = "120705" },
                    new { spotId = "ミュリルの落涙", pageId = "120706" },
                    new { spotId = "トゥシ・メキタ湖北", pageId = "120751" },
                    new { spotId = "ダワトリ溺没神殿", pageId = "120752" },
                    new { spotId = "トゥシ・メキタ湖中央", pageId = "120753" },
                    new { spotId = "トゥシ・メキタ湖南", pageId = "120754" },
                    new { spotId = "未知の漁場 (ラケティカ大森林)", pageId = "120791" },
                    new { spotId = "フラウンダーの穴蔵", pageId = "120801" },
                    new { spotId = "陸人の墓標", pageId = "120802" },
                    new { spotId = "キャリバン海底谷北西", pageId = "120803" },
                    new { spotId = "キャリバンの古巣穴西", pageId = "120804" },
                    new { spotId = "キャリバンの古巣穴東", pageId = "120805" },
                    new { spotId = "プルプラ洞", pageId = "120806" },
                    new { spotId = "ノルヴラント大陸斜面", pageId = "120807" },
                    new { spotId = "ガラディオン湾沖合", pageId = "130101" },
                    new { spotId = "ガラディオン湾沖合：幻海流", pageId = "130102" },
                    new { spotId = "メルトール海峡南", pageId = "130201" },
                    new { spotId = "メルトール海峡南：幻海流", pageId = "130202" },
                    new { spotId = "メルトール海峡北", pageId = "130301" },
                    new { spotId = "メルトール海峡北：幻海流", pageId = "130302" },
                    new { spotId = "ロータノ海沖合", pageId = "130401" },
                    new { spotId = "ロータノ海沖合：幻海流", pageId = "130402" },
                    new { spotId = "ディアデム諸島の洞穴", pageId = "140101" },
                    new { spotId = "ディアデム諸島の南西池", pageId = "140102" },
                    new { spotId = "ディアデム諸島の北西池", pageId = "140103" },
                    new { spotId = "風吹き抜ける雲海", pageId = "140104" },
                    new { spotId = "風穏やかな雲海", pageId = "140105" },
                    new { spotId = "風渦巻く雲海", pageId = "140106" },
                    new { spotId = "風吹き上がる雲海", pageId = "140107" },
                }
                .ToDictionary(item => item.spotId, item => item.pageId);
        }

        public static string GetCBHLink(this FishingSpot spot, string lang = null)
        {
            string pageId;
            if (!_pageIdSourceOfCBH.TryGetValue(((IGameDataObject)spot).InternalId, out pageId))
                return null;
            var translateId = new TranslationTextId(TranslationCategory.Url, "CBH.SpotPage");
            var format = lang != null ? Translate.Instance[translateId, lang] : Translate.Instance[translateId];
            return string.Format(format, pageId);
        }

        internal static async Task<string> DownloadCBHPage(this FishingSpot fishingSpot, FileInfo cacheFile)
        {
            if (cacheFile.Exists)
                return File.ReadAllText(cacheFile.FullName);
            else
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36");
                    var url = fishingSpot.GetCBHLink("ja");
                    while (true)
                    {
                        var success = false;
                        try
                        {
                            var doc = await client.GetStringAsync(url);
                            Directory.CreateDirectory(cacheFile.DirectoryName);
                            File.WriteAllText(cacheFile.FullName, doc);
                            success = true;
                            return doc;
                        }
                        catch (HttpRequestException)
                        {
                        }
                        finally
                        {
                            if (!success)
                                cacheFile.Delete();
                        }
                    }
                }
            }
        }
    }
}
