﻿using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer
{
    static class FishingBaitExtensions
    {
        private static IDictionary<string, string> _pageIdSourceOfCBH;
        private static IDictionary<string, string> _pageIdSourceOfEDB;

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
                .ToDictionary(item => item.baitId, item => item.pageId);

            _pageIdSourceOfEDB =
                new[]
                {
                    new { baitId = "アオイソメ", pageId = "a933b2f537f" },
                    new { baitId = "イカの切り身", pageId = "2936da56d3b" },
                    new { baitId = "イクラ", pageId = "671be727b9d" },
                    new { baitId = "イワイソメ", pageId = "694102d8a93" },
                    new { baitId = "オヴィムジャーキー", pageId = "01f2d95fe75" },
                    new { baitId = "カディスラーヴァ", pageId = "7d6ba962c36" },
                    new { baitId = "クラブボール", pageId = "44563ec3eec" },
                    new { baitId = "クリル", pageId = "80d5653e43f" },
                    new { baitId = "クリルフィーダー", pageId = "7741529b7ab" },
                    new { baitId = "クロウフライ", pageId = "537c103525e" },
                    new { baitId = "グロウワーム", pageId = "7aa4de1cfb0" },
                    new { baitId = "ゴビーボール", pageId = "8c1c8a0d370" },
                    new { baitId = "ゴブリンジグ", pageId = "5ae73cebfd7" },
                    new { baitId = "サスペンドミノー", pageId = "f4195ff6727" },
                    new { baitId = "サンドゲッコー", pageId = "ff685e53937" },
                    new { baitId = "サンドリーチ", pageId = "64cf535cf45" },
                    new { baitId = "ザザムシ", pageId = "90b08a4d1db" },
                    new { baitId = "ザリガニボール", pageId = "316abbce58a" },
                    new { baitId = "シュリンプフィーダー", pageId = "2b8d118a8de" },
                    new { baitId = "ショートビルミノー", pageId = "bc8c84745f7" },
                    new { baitId = "シルバースプーン", pageId = "81270000bd7" },
                    new { baitId = "シンキングミノー", pageId = "94ccef34657" },
                    new { baitId = "ジャンボガガンボ", pageId = "696b15e1382" },
                    new { baitId = "スカイボール", pageId = "ba252ebbdd9" },
                    new { baitId = "ストーンラーヴァ", pageId = "c6403ce3e45" },
                    new { baitId = "ストリーマー", pageId = "566e28a099a" },
                    new { baitId = "スナーブルフライ", pageId = "8f2ed8aa99c" },
                    new { baitId = "スピナー", pageId = "532f6228183" },
                    new { baitId = "スピナーベイト", pageId = "2e88d1508bc" },
                    new { baitId = "スプーンワーム", pageId = "48378a18867" },
                    new { baitId = "タイカブラ", pageId = "2b263acfd81" },
                    new { baitId = "チョコボフライ", pageId = "93c02475ad9" },
                    new { baitId = "ツチグモ", pageId = "cce659c7c34" },
                    new { baitId = "テッポウムシ", pageId = "bb781c0160d" },
                    new { baitId = "ディアデム・ガガンボ", pageId = "281ef144b40" },
                    new { baitId = "ディアデム・バルーン", pageId = "bf5f0874fea" },
                    new { baitId = "ディアデム・ホバーワーム", pageId = "e60165138d5" },
                    new { baitId = "ディアデム・レッドバルーン", pageId = "48898f49506" },
                    new { baitId = "デザートフロッグ", pageId = "f761a545f51" },
                    new { baitId = "トップウォーターフロッグ", pageId = "eb709402b53" },
                    new { baitId = "ドバミミズ", pageId = "26a32b35867" },
                    new { baitId = "ハナアブ", pageId = "a049462cb08" },
                    new { baitId = "ハニーワーム", pageId = "e4dd97f9650" },
                    new { baitId = "バイオレットワーム", pageId = "e568088edad" },
                    new { baitId = "バスボール", pageId = "740b0650243" },
                    new { baitId = "バターワーム", pageId = "60f4d0c133d" },
                    new { baitId = "バルーンバグ", pageId = "a1c92ca7c95" },
                    new { baitId = "ピルバグ", pageId = "2ef99ae31ce" },
                    new { baitId = "ファットワーム", pageId = "f9472bdfab0" },
                    new { baitId = "フェザントフライ", pageId = "e19d5e28e2e" },
                    new { baitId = "フルーツワーム", pageId = "3bdb03bbd7e" },
                    new { baitId = "フローティングミノー", pageId = "d03b99fd1a9" },
                    new { baitId = "ブラススプーン", pageId = "f30603a5b15" },
                    new { baitId = "ブラッドワーム", pageId = "8c710c92524" },
                    new { baitId = "ブルートリーチ", pageId = "4a1b812df6f" },
                    new { baitId = "ブレーデッドジグ", pageId = "cf50dbc3d8b" },
                    new { baitId = "ヘリングボール", pageId = "79e41cef633" },
                    new { baitId = "ヘヴィメタルジグ", pageId = "687e1dc5662" },
                    new { baitId = "ホバーワーム", pageId = "858f09032eb" },
                    new { baitId = "ポーラークリル", pageId = "65a3af65358" },
                    new { baitId = "マーブルラーヴァ", pageId = "9d8c9f058b0" },
                    new { baitId = "マグマワーム", pageId = "9bed983a255" },
                    new { baitId = "ミスリルスプーン", pageId = "0144e8df81b" },
                    new { baitId = "メタルジグ", pageId = "ef63a5ba615" },
                    new { baitId = "メタルスピナー", pageId = "48ff0da7c83" },
                    new { baitId = "モエビ", pageId = "e98aaaa5681" },
                    new { baitId = "モスプパ", pageId = "00ed364b827" },
                    new { baitId = "ユスリカ", pageId = "bd257411dfc" },
                    new { baitId = "ラグワーム", pageId = "12ba1196a1c" },
                    new { baitId = "ラットの尾", pageId = "72690dd403f" },
                    new { baitId = "レインボースプーン", pageId = "2585f3445d8" },
                    new { baitId = "レッドバルーン", pageId = "413a97d81e7" },
                    new { baitId = "ローリングストーン", pageId = "ced44e7e729" },
                    new { baitId = "ロバーボール", pageId = "2eb1d1e237b" },
                    new { baitId = "海水万能餌", pageId = "22f2bc31a8d" },
                    new { baitId = "活海老", pageId = "dd65f923a76" },
                    new { baitId = "弓角", pageId = "9b0ea84cf32" },
                    new { baitId = "蚕蛹", pageId = "f277c2d3c2c" },
                    new { baitId = "赤虫", pageId = "7e15b7ff99f" },
                    new { baitId = "淡水万能餌", pageId = "3da95b0de98" },
                    new { baitId = "万能ルアー", pageId = "a4b96e5453e" },
                    new { baitId = "蟲箱", pageId = "a40f7c56b8f" },
                }
                .ToDictionary(item => item.baitId, item => item.pageId);
        }

        public static string GetCBHLink(this FishingBait bait)
        {
            string pageId;
            if (!_pageIdSourceOfCBH.TryGetValue(((IGameDataObject)bait).InternalId, out pageId))
                return null;
            var format = Translate.Instance[new TranslationTextId(TranslationCategory.Url, "CBH.BaitPage")];
            return string.Format(format, pageId);
        }

        public static string GetEDBLink(this FishingBait bait)
        {
            string pageId;
            if (!_pageIdSourceOfEDB.TryGetValue(((IGameDataObject)bait).InternalId, out pageId))
                return null;
            var format = Translate.Instance[new TranslationTextId(TranslationCategory.Url, "EDB.BaitPage")];
            return string.Format(format, pageId);
        }

        internal static bool IsFishingBaitRawId(this string text)
        {
            return _pageIdSourceOfCBH.ContainsKey(text);
        }
    }
}
