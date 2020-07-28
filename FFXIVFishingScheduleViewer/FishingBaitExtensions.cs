using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer
{
    static class FishingBaitExtensions
    {
        private static IDictionary<GameDataObjectId, string> _pageIdSourceOfCBH;

        static FishingBaitExtensions()
        {
            _pageIdSourceOfCBH =
                new[]
                {
                    new { baitId = "モスプパ", pageId = "1001" },
                    new { baitId = "ラグワーム", pageId = "1002" },
                    new { baitId = "ザリガニボール", pageId = "1003" },
                    new { baitId = "ピルバグ", pageId = "1004" },
                    new { baitId = "ゴビーボール", pageId = "1005" },
                    new { baitId = "ブラッドワーム", pageId = "1006" },
                    new { baitId = "ユスリカ", pageId = "1007" },
                    new { baitId = "ラットの尾", pageId = "1008" },
                    new { baitId = "クラブボール", pageId = "1009" },
                    new { baitId = "クロウフライ", pageId = "1010" },
                    new { baitId = "バターワーム", pageId = "1011" },
                    new { baitId = "フローティングミノー", pageId = "1012" },
                    new { baitId = "ブラススプーン", pageId = "1013" },
                    new { baitId = "シュリンプフィーダー", pageId = "1014" },
                    new { baitId = "バスボール", pageId = "1015" },
                    new { baitId = "チョコボフライ", pageId = "1016" },
                    new { baitId = "スプーンワーム", pageId = "1017" },
                    new { baitId = "ハナアブ", pageId = "1018" },
                    new { baitId = "シルバースプーン", pageId = "1019" },
                    new { baitId = "メタルジグ", pageId = "1020" },
                    new { baitId = "シンキングミノー", pageId = "1021" },
                    new { baitId = "サンドリーチ", pageId = "1022" },
                    new { baitId = "ハニーワーム", pageId = "1023" },
                    new { baitId = "ヘリングボール", pageId = "1024" },
                    new { baitId = "フェザントフライ", pageId = "1025" },
                    new { baitId = "ヘヴィメタルジグ", pageId = "1026" },
                    new { baitId = "スピナー", pageId = "1027" },
                    new { baitId = "クリルフィーダー", pageId = "1028" },
                    new { baitId = "サンドゲッコー", pageId = "1029" },
                    new { baitId = "テッポウムシ", pageId = "1030" },
                    new { baitId = "ミスリルスプーン", pageId = "1031" },
                    new { baitId = "スナーブルフライ", pageId = "1032" },
                    new { baitId = "トップウォーターフロッグ", pageId = "1033" },
                    new { baitId = "グロウワーム", pageId = "1034" },
                    new { baitId = "ホバーワーム", pageId = "1035" },
                    new { baitId = "ローリングストーン", pageId = "1036" },
                    new { baitId = "レインボースプーン", pageId = "1037" },
                    new { baitId = "スピナーベイト", pageId = "1038" },
                    new { baitId = "ストリーマー", pageId = "1039" },
                    new { baitId = "弓角", pageId = "1040" },
                    new { baitId = "カディスラーヴァ", pageId = "1041" },
                    new { baitId = "ポーラークリル", pageId = "1042" },
                    new { baitId = "バルーンバグ", pageId = "1043" },
                    new { baitId = "ストーンラーヴァ", pageId = "1044" },
                    new { baitId = "ツチグモ", pageId = "1045" },
                    new { baitId = "ゴブリンジグ", pageId = "1046" },
                    new { baitId = "ブレーデッドジグ", pageId = "1047" },
                    new { baitId = "レッドバルーン", pageId = "1048" },
                    new { baitId = "マグマワーム", pageId = "1049" },
                    new { baitId = "バイオレットワーム", pageId = "1050" },
                    new { baitId = "ブルートリーチ", pageId = "1051" },
                    new { baitId = "ジャンボガガンボ", pageId = "1052" },
                    new { baitId = "イクラ", pageId = "1053" },
                    new { baitId = "ドバミミズ", pageId = "1054" },
                    new { baitId = "赤虫", pageId = "1055" },
                    new { baitId = "蚕蛹", pageId = "1056" },
                    new { baitId = "活海老", pageId = "1057" },
                    new { baitId = "タイカブラ", pageId = "1058" },
                    new { baitId = "サスペンドミノー", pageId = "1059" },
                    new { baitId = "ザザムシ", pageId = "1060" },
                    new { baitId = "アオイソメ", pageId = "1061" },
                    new { baitId = "フルーツワーム", pageId = "1062" },
                    new { baitId = "モエビ", pageId = "1063" },
                    new { baitId = "デザートフロッグ", pageId = "1064" },
                    new { baitId = "マーブルラーヴァ", pageId = "1065" },
                    new { baitId = "オヴィムジャーキー", pageId = "1066" },
                    new { baitId = "ロバーボール", pageId = "1067" },
                    new { baitId = "ショートビルミノー", pageId = "1068" },
                    new { baitId = "蟲箱", pageId = "1069" },
                    new { baitId = "イカの切り身", pageId = "1070" },
                    new { baitId = "淡水万能餌", pageId = "1071" },
                    new { baitId = "海水万能餌", pageId = "1072" },
                    new { baitId = "メタルスピナー", pageId = "1073" },
                    new { baitId = "イワイソメ", pageId = "1074" },
                    new { baitId = "クリル", pageId = "1075" },
                    new { baitId = "ファットワーム", pageId = "1076" },
                    new { baitId = "万能ルアー", pageId = "1077" },
                    new { baitId = "ディアデム・バルーン", pageId = "1078" },
                    new { baitId = "ディアデム・レッドバルーン", pageId = "1079" },
                    new { baitId = "ディアデム・ガガンボ", pageId = "1080" },
                    new { baitId = "ディアデム・ホバーワーム", pageId = "1081" },
                    new { baitId = "スカイボール", pageId = "1082" },
                    new { baitId = "スモールギグヘッド", pageId = "2001" },
                    new { baitId = "ミドルギグヘッド", pageId = "2002" },
                    new { baitId = "ラージギグヘッド", pageId = "2003" },
                }
                .ToDictionary(item => new GameDataObjectId(GameDataObjectCategory.FishingBait, item.baitId), item => item.pageId);
        }

        public static string GetCBHLink(this FishingBait bait)
        {
            string pageId;
            if (!_pageIdSourceOfCBH.TryGetValue(bait.Id, out pageId))
                return null;
            var format = Translate.Instance[new TranslationTextId(TranslationCategory.Url, "CBH.BaitPage")];
            return string.Format(format, pageId);
        }
    }
}
