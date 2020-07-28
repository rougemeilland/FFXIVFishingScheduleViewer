using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FFXIVFishingScheduleViewer
{
    static class FishExtensions
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

        private class MemoLine
        {
            public MemoLine()
            {
                Text = null;
                TranslationIdOfBait = null;
            }
            public string Text { get; set; }
            public TranslationTextId TranslationIdOfBait { get; set; }
        }

        private static Regex _直釣りパターン = new Regex(@"^(\((?<requires>(要|天候不問|時間帯不問)[^\)]*)\) *)?(?<bait>[^\(\)⇒]+)⇒\((?<atari>!{1,3}) *(?<hooking>(スト|プレ))?\)$", RegexOptions.Compiled);
        private static Regex _泳がせ釣りパターン = new Regex(@"^(\((?<requires>(要|天候不問|時間帯不問)[^\)]*)\) *)?(?<bait1>[^\(\)⇒]+)⇒\((?<atari1>!{1,3}) *(?<hooking1>(スト|プレ))\)(?<bait2>[^\(\)⇒]+)HQ⇒\((?<atari2>!{1,3}) *(?<hooking2>(スト|プレ))\)$", RegexOptions.Compiled);
        private static Regex _2段泳がせ釣りパターン = new Regex(@"^(\((?<requires>(要|天候不問|時間帯不問)[^\)]*)\) *)?(?<bait1>[^\(\)⇒]+)⇒\((?<atari1>!{1,3}) *(?<hooking1>(スト|プレ))\)(?<bait2>[^\(\)⇒]+)HQ⇒\((?<atari2>!{1,3}) *(?<hooking2>(スト|プレ))\)(?<bait3>[^\(\)⇒]+)HQ⇒\((?<atari3>!{1,3}) *(?<hooking3>(スト|プレ))\)$", RegexOptions.Compiled);
        private static Regex _3段泳がせ釣りパターン = new Regex(@"^(\((?<requires>(要|天候不問|時間帯不問)[^\)]*)\) *)?(?<bait1>[^\(\)⇒]+)⇒\((?<atari1>!{1,3}) *(?<hooking1>(スト|プレ))\)(?<bait2>[^\(\)⇒]+)HQ⇒\((?<atari2>!{1,3}) *(?<hooking2>(スト|プレ))\)(?<bait3>[^\(\)⇒]+)HQ⇒\((?<atari3>!{1,3}) *(?<hooking3>(スト|プレ))\)(?<bait4>[^\(\)⇒]+)HQ⇒\((?<atari4>!{1,3}) *(?<hooking4>(スト|プレ))\)?$", RegexOptions.Compiled);
        private static Regex _トレードリリース推奨パターン = new Regex(@"^※(?<fish>.*)が釣れたらトレードリリース$", RegexOptions.Compiled);
        private static Regex _漁師の直感条件パターン = new Regex(@"^要 *(?<fish>[^× ]+)×(?<count>[0-9]+)$", RegexOptions.Compiled);
        private static Regex _ET時間帯パターン = new Regex(@"^ET ?(?<fromhour>[0-9]+):(?<fromminute>[0-9]+) ?[-～] ?(?<tohour>[0-9]+):(?<tominute>[0-9]+)$", RegexOptions.Compiled);
        private static Regex _天候移ろいパターン = new Regex(@"^(?<before>[^/⇒]+(/[^/⇒]+)*)⇒(?<after>[^/⇒]+(/[^/⇒]+)*)$", RegexOptions.Compiled);
        private static TranslationTextId _unknownBaitNameId = new TranslationTextId(TranslationCategory.FishingBait, "??unknown??");
        private static IDictionary<GameDataObjectId, string> _pageIdSourceOfCBH;

        static FishExtensions()
        {
            _pageIdSourceOfCBH =
                new[]
                {
                    new { fishId = "アイアンヌース", pageId = "265" },
                    new { fishId = "アイスイーター", pageId = "366" },
                    new { fishId = "アイスピックスメルト", pageId = "314" },
                    new { fishId = "アイトーンコルト", pageId = "355" },
                    new { fishId = "アイボリーソール", pageId = "719" },
                    new { fishId = "アオウオ", pageId = "684" },
                    new { fishId = "アオウミグモ", pageId = "3095" },
                    new { fishId = "アカククリ", pageId = "711" },
                    new { fishId = "アカザ", pageId = "321" },
                    new { fishId = "アカゾナエ", pageId = "662" },
                    new { fishId = "アカヒレ", pageId = "573" },
                    new { fishId = "アカメ", pageId = "676" },
                    new { fishId = "アクアフラワー", pageId = "3159" },
                    new { fishId = "アクムノタネ", pageId = "466" },
                    new { fishId = "アケザリガニ", pageId = "580" },
                    new { fishId = "アサシンベタ", pageId = "130" },
                    new { fishId = "アジェレスカープ", pageId = "128" },
                    new { fishId = "アジスフィッシュ", pageId = "378" },
                    new { fishId = "アジムの使者", pageId = "707" },
                    new { fishId = "アジムアカメ", pageId = "780" },
                    new { fishId = "アジモドキ", pageId = "674" },
                    new { fishId = "アステロイデア", pageId = "3009" },
                    new { fishId = "アストロノータス", pageId = "350" },
                    new { fishId = "アダマンポリプテルス", pageId = "523" },
                    new { fishId = "アッシュトゥーナ", pageId = "77" },
                    new { fishId = "アノマロカリス", pageId = "285" },
                    new { fishId = "アバラシアサラマンダー", pageId = "552" },
                    new { fishId = "アバラシアスメルト", pageId = "24" },
                    new { fishId = "アバラシアビターリング", pageId = "514" },
                    new { fishId = "アバラシアピピラ", pageId = "717" },
                    new { fishId = "アバーブクラウド", pageId = "360" },
                    new { fishId = "アビススネイル", pageId = "3024" },
                    new { fishId = "アブトアード", pageId = "460" },
                    new { fishId = "アブラウナギ", pageId = "792" },
                    new { fishId = "アブラクダリ", pageId = "786" },
                    new { fishId = "アマゴ", pageId = "694" },
                    new { fishId = "アミア・カルヴァ", pageId = "392" },
                    new { fishId = "アユナギ", pageId = "791" },
                    new { fishId = "アラガンブレード・シャーク", pageId = "477" },
                    new { fishId = "アラミガンベール", pageId = "822" },
                    new { fishId = "アラミガンリボン", pageId = "619" },
                    new { fishId = "アラワレザリガニ", pageId = "551" },
                    new { fishId = "アリゲーターガー", pageId = "161" },
                    new { fishId = "アルバロッククラブ", pageId = "3031" },
                    new { fishId = "アルビノカイマン", pageId = "3028" },
                    new { fishId = "アルビノガー", pageId = "3033" },
                    new { fishId = "アレン・デル", pageId = "3190" },
                    new { fishId = "アロナクス", pageId = "3262" },
                    new { fishId = "アロワナ", pageId = "82" },
                    new { fishId = "アンクルスニッパー", pageId = "3165" },
                    new { fishId = "アンデッドフィッシュ", pageId = "731" },
                    new { fishId = "アントリオンスラッグ", pageId = "106" },
                    new { fishId = "アンバーサラマンダー", pageId = "451" },
                    new { fishId = "アンバーモンクフィッシュ", pageId = "3004" },
                    new { fishId = "アンパの使者", pageId = "3176" },
                    new { fishId = "アンフォーギヴン・クラブ", pageId = "3181" },
                    new { fishId = "アンモナイト", pageId = "338" },
                    new { fishId = "アークビショップフィッシュ", pageId = "762" },
                    new { fishId = "アーゼマの垂衣", pageId = "3247" },
                    new { fishId = "アーポアク", pageId = "3105" },
                    new { fishId = "アーマードプレコ", pageId = "94" },
                    new { fishId = "イエティキラー", pageId = "463" },
                    new { fishId = "イエローピピラ", pageId = "3083" },
                    new { fishId = "イシアタマ", pageId = "615" },
                    new { fishId = "イシガイ", pageId = "333" },
                    new { fishId = "イシダイ", pageId = "720" },
                    new { fishId = "イチモンジ", pageId = "740" },
                    new { fishId = "イトウオ", pageId = "3038" },
                    new { fishId = "イトマキ", pageId = "657" },
                    new { fishId = "イモータルジョー", pageId = "3193" },
                    new { fishId = "イルサバードバス", pageId = "139" },
                    new { fishId = "イルミナティパーチ", pageId = "404" },
                    new { fishId = "イルミナティマスク", pageId = "778" },
                    new { fishId = "インディゴヘリング", pageId = "75" },
                    new { fishId = "インフェルノスネイル", pageId = "109" },
                    new { fishId = "インフェルノホーン", pageId = "294" },
                    new { fishId = "イージスシュリンプ", pageId = "59" },
                    new { fishId = "イースタンパイク", pageId = "607" },
                    new { fishId = "ウィーディーシードラゴン", pageId = "3052" },
                    new { fishId = "ウォーターボンバー", pageId = "3023" },
                    new { fishId = "ウォームトラウト", pageId = "51" },
                    new { fishId = "ウオノタユウ", pageId = "556" },
                    new { fishId = "ウキキ", pageId = "737" },
                    new { fishId = "ウシガエル", pageId = "685" },
                    new { fishId = "ウスギヌダコ", pageId = "751" },
                    new { fishId = "ウチワ貝", pageId = "561" },
                    new { fishId = "ウナリナマズ", pageId = "7" },
                    new { fishId = "ウミダイジャ", pageId = "818" },
                    new { fishId = "ウミテング", pageId = "649" },
                    new { fishId = "ウーツナイフフィッシュ", pageId = "162" },
                    new { fishId = "ウーツナイフ・ゼニス", pageId = "303" },
                    new { fishId = "エアーキッシング・グラミー", pageId = "3125" },
                    new { fishId = "エクスキューショナー", pageId = "3257" },
                    new { fishId = "エスナカブリ", pageId = "774" },
                    new { fishId = "エディブルオイスター", pageId = "3110" },
                    new { fishId = "エニッドシュリンプ", pageId = "567" },
                    new { fishId = "エバーダークバス", pageId = "3174" },
                    new { fishId = "エラスモサウルス", pageId = "3278" },
                    new { fishId = "エリオプス", pageId = "3088" },
                    new { fishId = "エルダーピクシー", pageId = "3164" },
                    new { fishId = "エルダー・ディニクティス", pageId = "3230" },
                    new { fishId = "エルヴンスピア", pageId = "3119" },
                    new { fishId = "エンシェントシュリンプ", pageId = "3103" },
                    new { fishId = "エンジェルフィッシュ", pageId = "40" },
                    new { fishId = "エンツュイ", pageId = "3071" },
                    new { fishId = "エンドセラス", pageId = "292" },
                    new { fishId = "エーコンスネイル", pageId = "34" },
                    new { fishId = "エーテリックシードラゴン", pageId = "3250" },
                    new { fishId = "エーテルアイ", pageId = "377" },
                    new { fishId = "エーテルラウス", pageId = "287" },
                    new { fishId = "オイルイール", pageId = "415" },
                    new { fishId = "オウスフィッシュ", pageId = "3084" },
                    new { fishId = "オオウチワ貝", pageId = "781" },
                    new { fishId = "オオウナギ", pageId = "646" },
                    new { fishId = "オオゴエナマズ", pageId = "204" },
                    new { fishId = "オオシャル貝", pageId = "313" },
                    new { fishId = "オオタキタロ", pageId = "310" },
                    new { fishId = "オオテンジクザメ", pageId = "609" },
                    new { fishId = "オオナマズ", pageId = "163" },
                    new { fishId = "オオニベ", pageId = "563" },
                    new { fishId = "オオモリナマズ", pageId = "184" },
                    new { fishId = "オオルリデメキン", pageId = "3163" },
                    new { fishId = "オクトアイズ", pageId = "3001" },
                    new { fishId = "オサードサーモン", pageId = "839" },
                    new { fishId = "オサードトラウト", pageId = "576" },
                    new { fishId = "オシュオンズストーン", pageId = "3225" },
                    new { fishId = "オシュオンプリント", pageId = "203" },
                    new { fishId = "オチバウオ", pageId = "525" },
                    new { fishId = "オニダルマ", pageId = "661" },
                    new { fishId = "オニニラミ", pageId = "827" },
                    new { fishId = "オパビニア", pageId = "501" },
                    new { fishId = "オフィシャルボール", pageId = "3066" },
                    new { fishId = "オプロプケン", pageId = "438" },
                    new { fishId = "オヤジウオ", pageId = "238" },
                    new { fishId = "オヤニラミ", pageId = "716" },
                    new { fishId = "オリファントノーズ", pageId = "458" },
                    new { fishId = "オルゴイコルコイ", pageId = "266" },
                    new { fishId = "オンドの宿敵", pageId = "3100" },
                    new { fishId = "オンドの溜息", pageId = "3198" },
                    new { fishId = "オンドの銛", pageId = "3099" },
                    new { fishId = "オーガイール", pageId = "3220" },
                    new { fishId = "オーガバラクーダ", pageId = "58" },
                    new { fishId = "オーガホーン", pageId = "325" },
                    new { fishId = "オークルート", pageId = "118" },
                    new { fishId = "オーシャンクラウド", pageId = "12" },
                    new { fishId = "オータムリーフ", pageId = "3162" },
                    new { fishId = "オールドソフティ", pageId = "226" },
                    new { fishId = "オールドマーリン", pageId = "289" },
                    new { fishId = "カイマン", pageId = "402" },
                    new { fishId = "カイラージョン", pageId = "228" },
                    new { fishId = "カタクチイワシ", pageId = "539" },
                    new { fishId = "カネヒラ", pageId = "677" },
                    new { fishId = "カペリン", pageId = "437" },
                    new { fishId = "カミナ", pageId = "647" },
                    new { fishId = "カラードカープ", pageId = "46" },
                    new { fishId = "カローンズランタン", pageId = "269" },
                    new { fishId = "カワアカメ", pageId = "564" },
                    new { fishId = "カワシジミ", pageId = "697" },
                    new { fishId = "カワダイ", pageId = "712" },
                    new { fishId = "カンムリカブリ", pageId = "3183" },
                    new { fishId = "カーディナルフィッシュ", pageId = "735" },
                    new { fishId = "カーペンタークラブ", pageId = "702" },
                    new { fishId = "カールクラブ", pageId = "784" },
                    new { fishId = "カールザガス", pageId = "788" },
                    new { fishId = "ガスボムパファー", pageId = "406" },
                    new { fishId = "ガトルの汗", pageId = "3169" },
                    new { fishId = "ガラディオンアンチョビ", pageId = "3200" },
                    new { fishId = "ガラディオンゴビー", pageId = "3199" },
                    new { fishId = "ガラ・ルファ", pageId = "409" },
                    new { fishId = "ガリククラブ", pageId = "3062" },
                    new { fishId = "ガー", pageId = "136" },
                    new { fishId = "ガーデンスキッパー", pageId = "3146" },
                    new { fishId = "キサントバス", pageId = "3027" },
                    new { fishId = "キジハタ", pageId = "610" },
                    new { fishId = "キッシング・グラミー", pageId = "343" },
                    new { fishId = "キャプテンズチャリス", pageId = "284" },
                    new { fishId = "キャラコトラウト", pageId = "3005" },
                    new { fishId = "キャラバンイール", pageId = "164" },
                    new { fishId = "キャリバンズスケイル", pageId = "3096" },
                    new { fishId = "キュウケツヤツメ", pageId = "150" },
                    new { fishId = "キワヒルスタ", pageId = "3104" },
                    new { fishId = "キンツギククリ", pageId = "3141" },
                    new { fishId = "キンバーナイト", pageId = "372" },
                    new { fishId = "キー・オブ・ヘヴン", pageId = "3236" },
                    new { fishId = "ギガシャーク", pageId = "209" },
                    new { fishId = "ギガンジャクシ", pageId = "186" },
                    new { fishId = "ギガントオクトパス", pageId = "170" },
                    new { fishId = "ギガントグルーパー", pageId = "348" },
                    new { fishId = "ギガントバス", pageId = "602" },
                    new { fishId = "ギマ", pageId = "748" },
                    new { fishId = "ギラバニアチャブ", pageId = "742" },
                    new { fishId = "ギラバニアントラウト", pageId = "531" },
                    new { fishId = "ギルフィッシュ", pageId = "534" },
                    new { fishId = "クァールフィッシュ", pageId = "154" },
                    new { fishId = "クアル", pageId = "817" },
                    new { fishId = "クイックシルバーブレード", pageId = "3238" },
                    new { fishId = "クイーンズガウン", pageId = "3154" },
                    new { fishId = "クエ", pageId = "667" },
                    new { fishId = "クギトカゲ", pageId = "3195" },
                    new { fishId = "クサイロカジカ", pageId = "590" },
                    new { fishId = "クサフグ", pageId = "645" },
                    new { fishId = "クセナカンサス", pageId = "840" },
                    new { fishId = "クノ・ザ・キラー", pageId = "299" },
                    new { fishId = "クモウツボ", pageId = "642" },
                    new { fishId = "クライミングパーチ", pageId = "63" },
                    new { fishId = "クラウドジェリー", pageId = "126" },
                    new { fishId = "クラウドバタフライ", pageId = "503" },
                    new { fishId = "クラウドフィッシュ", pageId = "331" },
                    new { fishId = "クラウンテトラ", pageId = "3087" },
                    new { fishId = "クラウントラウト", pageId = "447" },
                    new { fishId = "クラウンローチ", pageId = "91" },
                    new { fishId = "クラックシェルケルプ", pageId = "3043" },
                    new { fishId = "クリア・ハルオネ", pageId = "507" },
                    new { fishId = "クリスタリウムテトラ", pageId = "3007" },
                    new { fishId = "クリスタルナイフ", pageId = "3014" },
                    new { fishId = "クリスタルパーチ", pageId = "205" },
                    new { fishId = "クリスタルピジョン", pageId = "493" },
                    new { fishId = "クリスタルフィッシュ", pageId = "379" },
                    new { fishId = "クリムゾントラウト", pageId = "104" },
                    new { fishId = "クリムゾンモンクフィッシュ", pageId = "3218" },
                    new { fishId = "クリームオイスター", pageId = "3002" },
                    new { fishId = "クリーンソーサー", pageId = "3025" },
                    new { fishId = "クルザスオイスター", pageId = "457" },
                    new { fishId = "クルザスクラブ", pageId = "318" },
                    new { fishId = "クルザスクリオネ", pageId = "439" },
                    new { fishId = "クルザスパファー", pageId = "362" },
                    new { fishId = "クレザリガニ", pageId = "588" },
                    new { fishId = "クロズキン", pageId = "650" },
                    new { fishId = "クロナマズ", pageId = "64" },
                    new { fishId = "クロハコフグ", pageId = "639" },
                    new { fishId = "クロムハンマーヘッド", pageId = "3222" },
                    new { fishId = "ググリューサウルス", pageId = "3268" },
                    new { fishId = "グッピー", pageId = "739" },
                    new { fishId = "グラスイール", pageId = "3144" },
                    new { fishId = "グラススキッパー", pageId = "578" },
                    new { fishId = "グラスパーチ", pageId = "52" },
                    new { fishId = "グラディウス", pageId = "3214" },
                    new { fishId = "グラディエーターベタ", pageId = "31" },
                    new { fishId = "グラナイトクラブ", pageId = "354" },
                    new { fishId = "グランドデイムバタフライ", pageId = "3180" },
                    new { fishId = "グランドマーリン", pageId = "3245" },
                    new { fishId = "グリップキリフィッシュ", pageId = "78" },
                    new { fishId = "グリナー", pageId = "250" },
                    new { fishId = "グリムクラブ", pageId = "550" },
                    new { fishId = "グリロタルパ", pageId = "357" },
                    new { fishId = "グリーンジェスター", pageId = "270" },
                    new { fishId = "グルマンクラブ", pageId = "3168" },
                    new { fishId = "グレイカープ", pageId = "3113" },
                    new { fishId = "グレイシャーコア", pageId = "324" },
                    new { fishId = "グレイスキッパー", pageId = "3067" },
                    new { fishId = "グースフィッシュ", pageId = "152" },
                    new { fishId = "グールバラクーダ", pageId = "3209" },
                    new { fishId = "ケイブキリフィッシュ", pageId = "553" },
                    new { fishId = "ケーブヤビー", pageId = "153" },
                    new { fishId = "ゲアイ", pageId = "3127" },
                    new { fishId = "ゲイジングコア", pageId = "3117" },
                    new { fishId = "ゲイラキラー", pageId = "429" },
                    new { fishId = "ゲコクジョウ", pageId = "3073" },
                    new { fishId = "コイマリモ", pageId = "744" },
                    new { fishId = "コウギョクヒトデ", pageId = "669" },
                    new { fishId = "コエラカントゥス", pageId = "291" },
                    new { fishId = "コクレン", pageId = "663" },
                    new { fishId = "コッコステウス", pageId = "3263" },
                    new { fishId = "コツゼツ", pageId = "616" },
                    new { fishId = "コノハタツノコ", pageId = "557" },
                    new { fishId = "コハクヤツメ", pageId = "3060" },
                    new { fishId = "コブラフィッシュ", pageId = "413" },
                    new { fishId = "コボルドパファー", pageId = "123" },
                    new { fishId = "コメットミノウ", pageId = "512" },
                    new { fishId = "コメットータス", pageId = "387" },
                    new { fishId = "コルシアフロウンダー", pageId = "3045" },
                    new { fishId = "コルシアンキングクラブ", pageId = "3191" },
                    new { fishId = "コルシアンハンプヘッド", pageId = "3051" },
                    new { fishId = "コロモダコ", pageId = "604" },
                    new { fishId = "コーネリア", pageId = "296" },
                    new { fishId = "コープスチャブ", pageId = "812" },
                    new { fishId = "コーラルシードラゴン", pageId = "3251" },
                    new { fishId = "コーラルバタフライ", pageId = "20" },
                    new { fishId = "コーラルマンタ", pageId = "3276" },
                    new { fishId = "ゴクソツアンコウ", pageId = "838" },
                    new { fishId = "ゴブスレイヤー", pageId = "258" },
                    new { fishId = "ゴブリバス", pageId = "443" },
                    new { fishId = "ゴブリパス", pageId = "370" },
                    new { fishId = "ゴブリンパーチ", pageId = "127" },
                    new { fishId = "ゴーストシャーク", pageId = "3237" },
                    new { fishId = "ゴールデンシクリッド", pageId = "513" },
                    new { fishId = "ゴールデンフィン", pageId = "208" },
                    new { fishId = "ゴールデンロブスター", pageId = "3077" },
                    new { fishId = "ゴールデンローチ", pageId = "101" },
                    new { fishId = "ゴールドスミスクラブ", pageId = "450" },
                    new { fishId = "サイレンサー", pageId = "3255" },
                    new { fishId = "サウザンドイヤー・イーチ", pageId = "278" },
                    new { fishId = "サウザンパイク", pageId = "121" },
                    new { fishId = "サウスコルシアコッド", pageId = "3046" },
                    new { fishId = "サカサナマズ", pageId = "342" },
                    new { fishId = "サゴリーモンクフィッシュ", pageId = "147" },
                    new { fishId = "サザエ", pageId = "668" },
                    new { fishId = "サニーバタフライ", pageId = "3203" },
                    new { fishId = "サファイアファン", pageId = "761" },
                    new { fishId = "サベネアン・リーフフィッシュ", pageId = "506" },
                    new { fishId = "サリャクカイマン", pageId = "431" },
                    new { fishId = "サリャクスカルピン", pageId = "347" },
                    new { fishId = "サンゴノオトシゴ", pageId = "743" },
                    new { fishId = "サンセットセイル", pageId = "442" },
                    new { fishId = "サンダラフィッシュ", pageId = "135" },
                    new { fishId = "サンダーガット", pageId = "273" },
                    new { fishId = "サンダースケイル", pageId = "494" },
                    new { fishId = "サンダーボルト", pageId = "428" },
                    new { fishId = "サンディスク", pageId = "158" },
                    new { fishId = "サンドエッグ", pageId = "3059" },
                    new { fishId = "サンドストームライダー", pageId = "102" },
                    new { fishId = "サンドフィッシュ", pageId = "89" },
                    new { fishId = "サンドブリーム", pageId = "92" },
                    new { fishId = "サンバス", pageId = "579" },
                    new { fishId = "サーベルタイガーコッド", pageId = "214" },
                    new { fishId = "サーメットヘッド", pageId = "810" },
                    new { fishId = "ザガス", pageId = "577" },
                    new { fishId = "ザクロウミ", pageId = "826" },
                    new { fishId = "ザラビクニン", pageId = "741" },
                    new { fishId = "ザリガニ", pageId = "2" },
                    new { fishId = "ザルエラ", pageId = "201" },
                    new { fishId = "ザ・セカンドワン", pageId = "472" },
                    new { fishId = "ザ・チョッパー", pageId = "3264" },
                    new { fishId = "ザ・リッパー", pageId = "235" },
                    new { fishId = "シアンオクトパス", pageId = "3221" },
                    new { fishId = "シアンシーデビル", pageId = "3106" },
                    new { fishId = "シェイプシフター", pageId = "3049" },
                    new { fishId = "シオウニ", pageId = "701" },
                    new { fishId = "シコラクス", pageId = "3093" },
                    new { fishId = "シップワーム", pageId = "356" },
                    new { fishId = "シデンナマズ", pageId = "808" },
                    new { fishId = "シビトクライ", pageId = "665" },
                    new { fishId = "シミターフィッシュ", pageId = "614" },
                    new { fishId = "シャギーシードラゴン", pageId = "3227" },
                    new { fishId = "シャジクノミ", pageId = "830" },
                    new { fishId = "シャドーストリーク", pageId = "283" },
                    new { fishId = "シャボンアンコウ", pageId = "3098" },
                    new { fishId = "シャリベネ", pageId = "504" },
                    new { fishId = "シャル貝", pageId = "156" },
                    new { fishId = "シャークトゥーナ", pageId = "251" },
                    new { fishId = "シャードガジオン", pageId = "3120" },
                    new { fishId = "シャーラタンサバイバー", pageId = "3266" },
                    new { fishId = "シュリーカー", pageId = "757" },
                    new { fishId = "ショニサウルス", pageId = "308" },
                    new { fishId = "シルクモラモラ", pageId = "732" },
                    new { fishId = "シルドラ", pageId = "233" },
                    new { fishId = "シルバーキティ", pageId = "3090" },
                    new { fishId = "シルバーシャーク", pageId = "111" },
                    new { fishId = "シルバーソブリン", pageId = "213" },
                    new { fishId = "シルフズベーン", pageId = "248" },
                    new { fishId = "シロイカ", pageId = "673" },
                    new { fishId = "シンカー", pageId = "257" },
                    new { fishId = "シンタクヤブリ", pageId = "772" },
                    new { fishId = "シーキューカンバー", pageId = "14" },
                    new { fishId = "シーサバトン", pageId = "3233" },
                    new { fishId = "シーデビル", pageId = "143" },
                    new { fishId = "シートラップ", pageId = "3102" },
                    new { fishId = "シーネトル", pageId = "3244" },
                    new { fishId = "シーハッグ", pageId = "293" },
                    new { fishId = "シーピクル", pageId = "72" },
                    new { fishId = "シーフベタ", pageId = "449" },
                    new { fishId = "シーホース", pageId = "62" },
                    new { fishId = "シーマ", pageId = "87" },
                    new { fishId = "シーラカンス", pageId = "166" },
                    new { fishId = "シーロストラタスモトロ", pageId = "483" },
                    new { fishId = "ジェイドアクソロトル", pageId = "3136" },
                    new { fishId = "ジェイドイール", pageId = "48" },
                    new { fishId = "ジェスターフィッシュ", pageId = "3160" },
                    new { fishId = "ジェナーナの涙粒", pageId = "3134" },
                    new { fishId = "ジャイアントコレクター", pageId = "710" },
                    new { fishId = "ジャイアントスキッド", pageId = "169" },
                    new { fishId = "ジャイアントバス", pageId = "95" },
                    new { fishId = "ジャウー", pageId = "369" },
                    new { fishId = "ジャケットスネイル", pageId = "3010" },
                    new { fishId = "ジャスパーヘッド", pageId = "3206" },
                    new { fishId = "ジャックナイフ", pageId = "43" },
                    new { fishId = "ジャッジレイ", pageId = "218" },
                    new { fishId = "ジャノ", pageId = "298" },
                    new { fishId = "ジャリガイ", pageId = "3017" },
                    new { fishId = "ジャリモグリ", pageId = "538" },
                    new { fishId = "ジャンクモンガー", pageId = "207" },
                    new { fishId = "ジャンヌ・トラウト", pageId = "240" },
                    new { fishId = "ジュエリージェリー", pageId = "416" },
                    new { fishId = "ジュエルマリモ", pageId = "446" },
                    new { fishId = "ジンコツシャブリ", pageId = "227" },
                    new { fishId = "スウィートニュート", pageId = "259" },
                    new { fishId = "スウェットフィッシュ", pageId = "622" },
                    new { fishId = "スカイスウィーパー", pageId = "385" },
                    new { fishId = "スカイハイフィッシュ", pageId = "405" },
                    new { fishId = "スカイフィッシュ", pageId = "132" },
                    new { fishId = "スカイフェアリー・エオス", pageId = "394" },
                    new { fishId = "スカイフェアリー・セレネ", pageId = "353" },
                    new { fishId = "スカイメデューサ", pageId = "371" },
                    new { fishId = "スカイルーム", pageId = "335" },
                    new { fishId = "スカイワーム", pageId = "317" },
                    new { fishId = "スカルイーター", pageId = "3035" },
                    new { fishId = "スカルピン", pageId = "119" },
                    new { fishId = "スカルプター", pageId = "625" },
                    new { fishId = "スケイルリッパー", pageId = "465" },
                    new { fishId = "スケルトンフィッシュ", pageId = "3015" },
                    new { fishId = "スケーリーフット", pageId = "210" },
                    new { fishId = "スコーピオンフライ", pageId = "410" },
                    new { fishId = "スターゲイザーフィッシュ", pageId = "3109" },
                    new { fishId = "スターチェイサー", pageId = "3186" },
                    new { fishId = "スターフラワー", pageId = "323" },
                    new { fishId = "スターブライト", pageId = "281" },
                    new { fishId = "スチュペンデミス", pageId = "425" },
                    new { fishId = "スチールヘッド", pageId = "516" },
                    new { fishId = "スティップリングイール", pageId = "3101" },
                    new { fishId = "スティールシャーク", pageId = "627" },
                    new { fishId = "スティールローチ", pageId = "718" },
                    new { fishId = "ステタカントゥス", pageId = "842" },
                    new { fishId = "ステップシュリンプ", pageId = "793" },
                    new { fishId = "ストームコア", pageId = "346" },
                    new { fishId = "ストームブラッドライダー", pageId = "499" },
                    new { fishId = "ストームライダー", pageId = "435" },
                    new { fishId = "ストーンクラブ", pageId = "26" },
                    new { fishId = "スナガクレ", pageId = "211" },
                    new { fishId = "スナナマズ", pageId = "96" },
                    new { fishId = "スナモグリ", pageId = "19" },
                    new { fishId = "スネイルフィッシュ", pageId = "3261" },
                    new { fishId = "スネークディスカス", pageId = "3032" },
                    new { fishId = "スパイクフィッシュ", pageId = "798" },
                    new { fishId = "スパイダークラブ", pageId = "3228" },
                    new { fishId = "スパエキノス", pageId = "423" },
                    new { fishId = "スピアノーズ", pageId = "255" },
                    new { fishId = "スピアヘッド", pageId = "3054" },
                    new { fishId = "スピリット", pageId = "414" },
                    new { fishId = "スピーカー", pageId = "497" },
                    new { fishId = "スファライフィッシュ", pageId = "775" },
                    new { fishId = "スプリットクラウド", pageId = "134" },
                    new { fishId = "スプリングキング", pageId = "263" },
                    new { fishId = "スプークフィッシュ", pageId = "3156" },
                    new { fishId = "スペクトラルシーホース", pageId = "3273" },
                    new { fishId = "スペクトラルディスカス", pageId = "3272" },
                    new { fishId = "スペクトラルバス", pageId = "3274" },
                    new { fishId = "スペクトラルメガロドン", pageId = "3271" },
                    new { fishId = "スポテッドガー", pageId = "175" },
                    new { fishId = "スポテッドクテノポマ", pageId = "3075" },
                    new { fishId = "スポテッドパファー", pageId = "99" },
                    new { fishId = "スポテッドプレコ", pageId = "73" },
                    new { fishId = "スラッジスキッパー", pageId = "131" },
                    new { fishId = "スリーリップス", pageId = "329" },
                    new { fishId = "スロウンダガー", pageId = "3205" },
                    new { fishId = "ズワイガニ", pageId = "675" },
                    new { fishId = "セイバーサーディン", pageId = "54" },
                    new { fishId = "セティ", pageId = "492" },
                    new { fishId = "セピアソール", pageId = "3042" },
                    new { fishId = "セルリアンローチ", pageId = "3076" },
                    new { fishId = "ゼッキワニ", pageId = "634" },
                    new { fishId = "ゼブラキャットフィッシュ", pageId = "3041" },
                    new { fishId = "ゼブラゴビー", pageId = "6" },
                    new { fishId = "ソクシツキ", pageId = "769" },
                    new { fishId = "ソティス", pageId = "3275" },
                    new { fishId = "ソルター", pageId = "261" },
                    new { fishId = "ソルトシャーク", pageId = "690" },
                    new { fishId = "ソルトシールド", pageId = "611" },
                    new { fishId = "ソルトセラー", pageId = "708" },
                    new { fishId = "ソルトミル", pageId = "752" },
                    new { fishId = "ソーサラーフィッシュ", pageId = "326" },
                    new { fishId = "ソーサーフィッシュ", pageId = "148" },
                    new { fishId = "ソースオクトパス", pageId = "3118" },
                    new { fishId = "ソールディアルビー", pageId = "3148" },
                    new { fishId = "タイガーキャットコッド", pageId = "3265" },
                    new { fishId = "タイガーコッド", pageId = "30" },
                    new { fishId = "タイガーフィッシュ", pageId = "352" },
                    new { fishId = "タイタニックソー", pageId = "180" },
                    new { fishId = "タイニーアダマンタス", pageId = "185" },
                    new { fishId = "タイフーンシュリンプ", pageId = "699" },
                    new { fishId = "タオビターリング", pageId = "594" },
                    new { fishId = "タカアシガニ", pageId = "648" },
                    new { fishId = "タキタロ", pageId = "181" },
                    new { fishId = "タキノボリ", pageId = "114" },
                    new { fishId = "タツマゴ", pageId = "734" },
                    new { fishId = "タニクダリ", pageId = "811" },
                    new { fishId = "タニノボリ", pageId = "528" },
                    new { fishId = "タマカイ", pageId = "660" },
                    new { fishId = "ターニッシュシャーク", pageId = "3204" },
                    new { fishId = "ターミネーター", pageId = "243" },
                    new { fishId = "ダイオウイカ", pageId = "603" },
                    new { fishId = "ダイオウカガミガイ", pageId = "3003" },
                    new { fishId = "ダイヤモンドアイ", pageId = "765" },
                    new { fishId = "ダイヤモンドアロワナ", pageId = "3170" },
                    new { fishId = "ダイヤモンドピピラ", pageId = "3089" },
                    new { fishId = "ダスキーゴビー", pageId = "13" },
                    new { fishId = "ダナフェンズマーク", pageId = "268" },
                    new { fishId = "ダンディーフィッシュ", pageId = "3147" },
                    new { fishId = "ダークアンブッシャー", pageId = "217" },
                    new { fishId = "ダークスター", pageId = "305" },
                    new { fishId = "ダークスリーパー", pageId = "36" },
                    new { fishId = "ダークナイト", pageId = "212" },
                    new { fishId = "ダークノーチラス", pageId = "3229" },
                    new { fishId = "ダークバス", pageId = "56" },
                    new { fishId = "ダークルート", pageId = "3082" },
                    new { fishId = "ダーティーヘリング", pageId = "246" },
                    new { fishId = "チェリートラウト", pageId = "424" },
                    new { fishId = "チェリーヘリング", pageId = "3070" },
                    new { fishId = "チャクラナマズ", pageId = "837" },
                    new { fishId = "チャブ", pageId = "3" },
                    new { fishId = "チョウチョウウオ", pageId = "555" },
                    new { fishId = "チンアナゴ", pageId = "658" },
                    new { fishId = "ツノカブリ", pageId = "3057" },
                    new { fishId = "ツブ", pageId = "654" },
                    new { fishId = "テルプシコレアン", pageId = "275" },
                    new { fishId = "ディスカス", pageId = "105" },
                    new { fishId = "ディスコボルス", pageId = "264" },
                    new { fishId = "ディニクティス", pageId = "172" },
                    new { fishId = "ディモルフォドン", pageId = "475" },
                    new { fishId = "ディープシーイール", pageId = "3256" },
                    new { fishId = "ディープシー・プレイス", pageId = "3217" },
                    new { fishId = "デイジーターバン", pageId = "3135" },
                    new { fishId = "デザートサブマリナー", pageId = "3056" },
                    new { fishId = "デザートソー", pageId = "3061" },
                    new { fishId = "デスローチ", pageId = "549" },
                    new { fishId = "デューンマンタ", pageId = "110" },
                    new { fishId = "トゥイッチビアード", pageId = "253" },
                    new { fishId = "トゥプクスアラ", pageId = "433" },
                    new { fishId = "トウロウマリモ", pageId = "521" },
                    new { fishId = "トゲトカゲ", pageId = "3064" },
                    new { fishId = "トビウオ", pageId = "559" },
                    new { fishId = "トビエイ", pageId = "640" },
                    new { fishId = "トビハゼ", pageId = "643" },
                    new { fishId = "トフィースネイル", pageId = "3036" },
                    new { fishId = "トライポッド", pageId = "3202" },
                    new { fishId = "トラフグ", pageId = "562" },
                    new { fishId = "トラフザメ", pageId = "651" },
                    new { fishId = "トラマフィッシュ", pageId = "241" },
                    new { fishId = "トリックスター", pageId = "202" },
                    new { fishId = "トルネドシャーク", pageId = "398" },
                    new { fishId = "ドォーヌホーン", pageId = "3158" },
                    new { fishId = "ドタールフィッシュ", pageId = "696" },
                    new { fishId = "ドトンフィッシュ", pageId = "100" },
                    new { fishId = "ドブガイ", pageId = "50" },
                    new { fishId = "ドマウナギ", pageId = "584" },
                    new { fishId = "ドマザリガニ", pageId = "575" },
                    new { fishId = "ドマス", pageId = "571" },
                    new { fishId = "ドマ金", pageId = "705" },
                    new { fishId = "ドライグラススキッパー", pageId = "586" },
                    new { fishId = "ドラウンドスナイパー", pageId = "274" },
                    new { fishId = "ドラゴンカブリ", pageId = "364" },
                    new { fishId = "ドラゴンソウル", pageId = "397" },
                    new { fishId = "ドランカードフィッシュ", pageId = "3231" },
                    new { fishId = "ドラヴァニアスメルト", pageId = "440" },
                    new { fishId = "ドラヴァニアンバス", pageId = "361" },
                    new { fishId = "ドリフトアイスフィッシュ", pageId = "3223" },
                    new { fishId = "ドリームゴビー", pageId = "215" },
                    new { fishId = "ドレパナスピス", pageId = "841" },
                    new { fishId = "ドーンメイデン", pageId = "280" },
                    new { fishId = "ナイツバス", pageId = "3085" },
                    new { fishId = "ナイフフィッシュ", pageId = "115" },
                    new { fishId = "ナガヒゲナマズ", pageId = "570" },
                    new { fishId = "ナガレクダリ", pageId = "764" },
                    new { fishId = "ナガレノボリ", pageId = "522" },
                    new { fishId = "ナナツボシ", pageId = "824" },
                    new { fishId = "ナバースマンタ", pageId = "3063" },
                    new { fishId = "ナマケチチブ", pageId = "515" },
                    new { fishId = "ナマコ", pageId = "659" },
                    new { fishId = "ナミタロ", pageId = "311" },
                    new { fishId = "ナルザルイール", pageId = "103" },
                    new { fishId = "ナーマの使者", pageId = "715" },
                    new { fishId = "ナーマの恵み", pageId = "583" },
                    new { fishId = "ナーマの愛寵", pageId = "832" },
                    new { fishId = "ナーマの爪", pageId = "799" },
                    new { fishId = "ニクトサウルス", pageId = "454" },
                    new { fishId = "ニジノヒトスジ", pageId = "831" },
                    new { fishId = "ニジメダカ", pageId = "790" },
                    new { fishId = "ニメーヤトラウト", pageId = "144" },
                    new { fishId = "ニルヴァーナクラブ", pageId = "537" },
                    new { fishId = "ニンジャベタ", pageId = "297" },
                    new { fishId = "ニンブルダンサー", pageId = "3243" },
                    new { fishId = "ヌマムツ", pageId = "452" },
                    new { fishId = "ヌルヌルキング", pageId = "221" },
                    new { fishId = "ネイルドオイスター", pageId = "3240" },
                    new { fishId = "ネプトの竜", pageId = "290" },
                    new { fishId = "ネモ", pageId = "490" },
                    new { fishId = "ノゴイ", pageId = "565" },
                    new { fishId = "ノゾキウオ", pageId = "601" },
                    new { fishId = "ノボリリュウ", pageId = "766" },
                    new { fishId = "ノーザンオイスター", pageId = "835" },
                    new { fishId = "ノーザンパイク", pageId = "122" },
                    new { fishId = "ノーチラス", pageId = "124" },
                    new { fishId = "ノーブルシーマ", pageId = "337" },
                    new { fishId = "ノーブルファン", pageId = "3040" },
                    new { fishId = "ノーブルフィッシュ", pageId = "3128" },
                    new { fishId = "ハイアラガンクラブ", pageId = "375" },
                    new { fishId = "ハイアラガンクラブ改", pageId = "473" },
                    new { fishId = "ハイウィンドジェリー", pageId = "399" },
                    new { fishId = "ハイエーテルラウス", pageId = "3248" },
                    new { fishId = "ハイパーチ", pageId = "232" },
                    new { fishId = "ハイランドパーチ", pageId = "527" },
                    new { fishId = "ハクビターリング", pageId = "623" },
                    new { fishId = "ハグルマガイ", pageId = "804" },
                    new { fishId = "ハチェットフィッシュ", pageId = "3039" },
                    new { fishId = "ハナタツ", pageId = "630" },
                    new { fishId = "ハナヒゲウツボ", pageId = "754" },
                    new { fishId = "ハリセンボン", pageId = "560" },
                    new { fishId = "ハリバット", pageId = "160" },
                    new { fishId = "ハルオネ", pageId = "316" },
                    new { fishId = "ハルオーネパイク", pageId = "145" },
                    new { fishId = "ハンターズアロー", pageId = "3150" },
                    new { fishId = "ハンテンナマズ", pageId = "536" },
                    new { fishId = "ハンドレッドアイ", pageId = "486" },
                    new { fishId = "ハンニバル", pageId = "279" },
                    new { fishId = "ハンプヘッド", pageId = "133" },
                    new { fishId = "ハンマーヘッド", pageId = "70" },
                    new { fishId = "ハンマーロブスター", pageId = "3235" },
                    new { fishId = "ハードキャンディ", pageId = "3022" },
                    new { fishId = "ハーバーヘリング", pageId = "15" },
                    new { fishId = "ハーミットクラブ", pageId = "3172" },
                    new { fishId = "ハーミットズフード", pageId = "3171" },
                    new { fishId = "ハーラルハドック", pageId = "85" },
                    new { fishId = "バイオガピラルク", pageId = "474" },
                    new { fishId = "バイオピラルク", pageId = "403" },
                    new { fishId = "バイスジョー", pageId = "3034" },
                    new { fishId = "バクチウチ", pageId = "767" },
                    new { fishId = "バサバサ", pageId = "382" },
                    new { fishId = "バスキングシャーク", pageId = "476" },
                    new { fishId = "バタフライフィッシュ", pageId = "381" },
                    new { fishId = "バタフライレインボー", pageId = "3065" },
                    new { fishId = "バトルガレー", pageId = "417" },
                    new { fishId = "バヌバヌヘッド", pageId = "349" },
                    new { fishId = "バラマンディ", pageId = "3259" },
                    new { fishId = "バリマンボン", pageId = "825" },
                    new { fishId = "バルーンパファー", pageId = "393" },
                    new { fishId = "バルーンフィッシュ", pageId = "97" },
                    new { fishId = "バルーンフロッグ", pageId = "520" },
                    new { fishId = "バレルアイ", pageId = "427" },
                    new { fishId = "バーサーカーベタ", pageId = "436" },
                    new { fishId = "パイクイール", pageId = "129" },
                    new { fishId = "パイッサキラー", pageId = "491" },
                    new { fishId = "パイレーツハンター", pageId = "300" },
                    new { fishId = "パガルザンディスカス", pageId = "140" },
                    new { fishId = "パガンピラルク", pageId = "566" },
                    new { fishId = "パジャルローチ", pageId = "530" },
                    new { fishId = "パラダイスクラブ", pageId = "3152" },
                    new { fishId = "パンダ蝶尾", pageId = "617" },
                    new { fishId = "パープルゴースト", pageId = "3115" },
                    new { fishId = "パープルバックラー", pageId = "746" },
                    new { fishId = "パールアイ", pageId = "626" },
                    new { fishId = "ヒゲナシ", pageId = "655" },
                    new { fishId = "ヒメダカ", pageId = "816" },
                    new { fishId = "ヒラマサ", pageId = "693" },
                    new { fishId = "ヒンターランドパーチ", pageId = "419" },
                    new { fishId = "ヒースチャー", pageId = "547" },
                    new { fishId = "ヒートプレコ", pageId = "776" },
                    new { fishId = "ヒートロッド", pageId = "327" },
                    new { fishId = "ビアナックブリーム", pageId = "65" },
                    new { fishId = "ビアナックブーン", pageId = "245" },
                    new { fishId = "ビショップフィッシュ", pageId = "489" },
                    new { fishId = "ビッグアイ", pageId = "3133" },
                    new { fishId = "ビッグアーチャー", pageId = "3079" },
                    new { fishId = "ビッグバイパー", pageId = "244" },
                    new { fishId = "ビランクラブ", pageId = "3055" },
                    new { fishId = "ピアレイ", pageId = "3138" },
                    new { fishId = "ピクシーフィッシュ", pageId = "3068" },
                    new { fishId = "ピクシーレインボー", pageId = "3196" },
                    new { fishId = "ピピラ", pageId = "16" },
                    new { fishId = "ピピラ・ピラ", pageId = "341" },
                    new { fishId = "ピラルク", pageId = "182" },
                    new { fishId = "ファイブイルムプレコ", pageId = "60" },
                    new { fishId = "ファットパース", pageId = "462" },
                    new { fishId = "ファングクラム", pageId = "319" },
                    new { fishId = "ファンネルシャーク", pageId = "3269" },
                    new { fishId = "フィッシュモンガー", pageId = "3241" },
                    new { fishId = "フィロソファーアロワナ", pageId = "430" },
                    new { fishId = "フィンガーシュリンプ", pageId = "8" },
                    new { fishId = "フィンガーズ", pageId = "242" },
                    new { fishId = "フェアリークィーン", pageId = "220" },
                    new { fishId = "フェアリークラム", pageId = "3189" },
                    new { fishId = "フェアリーバス", pageId = "33" },
                    new { fishId = "フォッシルアロワナ", pageId = "400" },
                    new { fishId = "フォークタン", pageId = "480" },
                    new { fishId = "フォーレンワン", pageId = "3270" },
                    new { fishId = "フックスティーラー", pageId = "760" },
                    new { fishId = "フッブートサラマンダー", pageId = "3145" },
                    new { fishId = "フッブートビチャー", pageId = "3139" },
                    new { fishId = "フューリィベタ", pageId = "3184" },
                    new { fishId = "フライマンタ", pageId = "137" },
                    new { fishId = "フライングエッグ", pageId = "334" },
                    new { fishId = "フライングガーナード", pageId = "422" },
                    new { fishId = "フラッドトゥーナ", pageId = "729" },
                    new { fishId = "フラッフィー", pageId = "3254" },
                    new { fishId = "フラワリングケルピー", pageId = "3155" },
                    new { fishId = "フリーシーモトロ", pageId = "453" },
                    new { fishId = "フルムーンサーディン", pageId = "83" },
                    new { fishId = "フレアフィッシュ", pageId = "479" },
                    new { fishId = "フローティングソーサー", pageId = "3249" },
                    new { fishId = "フローティングボルダー", pageId = "249" },
                    new { fishId = "フーアイーター", pageId = "3151" },
                    new { fishId = "フードウィンカー", pageId = "3097" },
                    new { fishId = "ブラインフィッシュ", pageId = "25" },
                    new { fishId = "ブラウンブーメラン", pageId = "389" },
                    new { fishId = "ブラスローチ", pageId = "21" },
                    new { fishId = "ブラックイール", pageId = "55" },
                    new { fishId = "ブラックウィップ", pageId = "373" },
                    new { fishId = "ブラックゴースト", pageId = "66" },
                    new { fishId = "ブラックジェットストリーム", pageId = "3197" },
                    new { fishId = "ブラックソール", pageId = "67" },
                    new { fishId = "ブラックトライスター", pageId = "3080" },
                    new { fishId = "ブラックベロジナカープ", pageId = "568" },
                    new { fishId = "ブラックメイジフィッシュ", pageId = "426" },
                    new { fishId = "ブラッディブルワー", pageId = "219" },
                    new { fishId = "ブラッドアイフロッグ", pageId = "3069" },
                    new { fishId = "ブラッドスキッパー", pageId = "412" },
                    new { fishId = "ブラッドスピッター", pageId = "532" },
                    new { fishId = "ブラッドバス", pageId = "271" },
                    new { fishId = "ブラッドバルーン", pageId = "3044" },
                    new { fishId = "ブルズバイト", pageId = "600" },
                    new { fishId = "ブルフロッグ", pageId = "330" },
                    new { fishId = "ブルーウィドー", pageId = "222" },
                    new { fishId = "ブルーオクトパス", pageId = "45" },
                    new { fishId = "ブルーコープス", pageId = "306" },
                    new { fishId = "ブルーサーモン", pageId = "42" },
                    new { fishId = "ブルーライトニング", pageId = "3161" },
                    new { fishId = "ブレードガンナー", pageId = "391" },
                    new { fishId = "ブレードスキッパー", pageId = "821" },
                    new { fishId = "ブロンズソール", pageId = "3182" },
                    new { fishId = "ブロンズレイクトラウト", pageId = "108" },
                    new { fishId = "ブロークンクラブ", pageId = "517" },
                    new { fishId = "ブローフィッシュ", pageId = "49" },
                    new { fishId = "プチアクソロトル", pageId = "374" },
                    new { fishId = "プチクピド", pageId = "3137" },
                    new { fishId = "プテラノドン", pageId = "421" },
                    new { fishId = "プテロダクティルス", pageId = "444" },
                    new { fishId = "プラチナグッピー", pageId = "3029" },
                    new { fishId = "プラチナブリーム", pageId = "3122" },
                    new { fishId = "プリンセストラウト", pageId = "11" },
                    new { fishId = "プリーストフィッシュ", pageId = "456" },
                    new { fishId = "プレイス", pageId = "138" },
                    new { fishId = "プレデターフィッシュ", pageId = "3112" },
                    new { fishId = "プロスペロイール", pageId = "3006" },
                    new { fishId = "プロディガルサン", pageId = "3267" },
                    new { fishId = "プロブレマティカス", pageId = "500" },
                    new { fishId = "プロプケン", pageId = "401" },
                    new { fishId = "ヘイルイーター", pageId = "478" },
                    new { fishId = "ヘノドゥス", pageId = "3053" },
                    new { fishId = "ヘモン", pageId = "814" },
                    new { fishId = "ヘリオバティス", pageId = "178" },
                    new { fishId = "ヘリコプリオン", pageId = "304" },
                    new { fishId = "ヘルムズマンズハンド", pageId = "277" },
                    new { fishId = "ヘルメットクラブ", pageId = "32" },
                    new { fishId = "ヘヴンリーフィッシュ", pageId = "3208" },
                    new { fishId = "ベジースキッパー", pageId = "833" },
                    new { fishId = "ベニウミグモ", pageId = "3018" },
                    new { fishId = "ベニコロモ", pageId = "680" },
                    new { fishId = "ベニザリガニ", pageId = "17" },
                    new { fishId = "ベニヒレ", pageId = "687" },
                    new { fishId = "ベルーガ", pageId = "709" },
                    new { fishId = "ベロジナカープ", pageId = "98" },
                    new { fishId = "ベロジナサーモン", pageId = "745" },
                    new { fishId = "ベロジナソウギョ", pageId = "526" },
                    new { fishId = "ベーンクラーケン", pageId = "459" },
                    new { fishId = "ペタルフィッシュ", pageId = "3142" },
                    new { fishId = "ペティノサウルス", pageId = "511" },
                    new { fishId = "ペンダントヘッド", pageId = "3179" },
                    new { fishId = "ホウネンエソ", pageId = "656" },
                    new { fishId = "ホタテウミヘビ", pageId = "621" },
                    new { fishId = "ホネイワシ", pageId = "3019" },
                    new { fishId = "ホネカブリ", pageId = "69" },
                    new { fishId = "ホネガイ", pageId = "652" },
                    new { fishId = "ホネザリガニ", pageId = "9" },
                    new { fishId = "ホネシャブリ", pageId = "79" },
                    new { fishId = "ホネトカシ", pageId = "524" },
                    new { fishId = "ホローアイズ", pageId = "247" },
                    new { fishId = "ホワイトアロワナ", pageId = "689" },
                    new { fishId = "ホワイトオイルパーチ", pageId = "3188" },
                    new { fishId = "ホワイトオクトパス", pageId = "396" },
                    new { fishId = "ホワイトオスカー", pageId = "368" },
                    new { fishId = "ホワイトホース", pageId = "624" },
                    new { fishId = "ホワイトロンゾ", pageId = "3194" },
                    new { fishId = "ホーリーカーペット", pageId = "267" },
                    new { fishId = "ボウフィン", pageId = "177" },
                    new { fishId = "ボクシングプレコ", pageId = "141" },
                    new { fishId = "ボクデン", pageId = "770" },
                    new { fishId = "ボサボサ", pageId = "498" },
                    new { fishId = "ボトリオレピス", pageId = "3132" },
                    new { fishId = "ボンゴラ", pageId = "18" },
                    new { fishId = "ボンドスプリッター", pageId = "763" },
                    new { fishId = "ボンバードフィッシュ", pageId = "260" },
                    new { fishId = "ポエキリア", pageId = "3140" },
                    new { fishId = "ポリプテルス", pageId = "367" },
                    new { fishId = "ポンポンポン", pageId = "358" },
                    new { fishId = "マイボイ", pageId = "328" },
                    new { fishId = "マウンテンアロワナ", pageId = "613" },
                    new { fishId = "マクロブラキウム", pageId = "322" },
                    new { fishId = "マグマツリー", pageId = "359" },
                    new { fishId = "マグマラウス", pageId = "386" },
                    new { fishId = "マジックバケツ", pageId = "461" },
                    new { fishId = "マジック・マッシュルームクラブ", pageId = "309" },
                    new { fishId = "マズラヤマーリン", pageId = "165" },
                    new { fishId = "マダムバタフライ", pageId = "481" },
                    new { fishId = "マッカチン", pageId = "47" },
                    new { fishId = "マッシュルームクラブ", pageId = "173" },
                    new { fishId = "マッドクラブ", pageId = "44" },
                    new { fishId = "マッドゴーレム", pageId = "229" },
                    new { fishId = "マッドスキッパー", pageId = "27" },
                    new { fishId = "マッドピルグリム", pageId = "225" },
                    new { fishId = "マトロンカープ", pageId = "231" },
                    new { fishId = "マナセイル", pageId = "384" },
                    new { fishId = "マハール", pageId = "307" },
                    new { fishId = "マヒマヒ", pageId = "159" },
                    new { fishId = "マフワイ", pageId = "332" },
                    new { fishId = "マボロシナマズ", pageId = "783" },
                    new { fishId = "マユツクリ", pageId = "3121" },
                    new { fishId = "マリンボム", pageId = "3211" },
                    new { fishId = "マルムケルプ", pageId = "1" },
                    new { fishId = "マーシナリークラブ", pageId = "730" },
                    new { fishId = "マーマンヘアー", pageId = "3253" },
                    new { fishId = "ミストアイ", pageId = "3129" },
                    new { fishId = "ミストキリフィッシュ", pageId = "3030" },
                    new { fishId = "ミスリルソブリン", pageId = "3242" },
                    new { fishId = "ミズカキスナヤモリ", pageId = "3058" },
                    new { fishId = "ミツクリザメ", pageId = "704" },
                    new { fishId = "ミトラスラッグ", pageId = "344" },
                    new { fishId = "ミトンクラブ", pageId = "84" },
                    new { fishId = "ミューヌフィッシュ", pageId = "518" },
                    new { fishId = "ミラージュチャブ", pageId = "529" },
                    new { fishId = "ミラージュマヒ", pageId = "747" },
                    new { fishId = "ミラースケイル", pageId = "276" },
                    new { fishId = "ミラーフィッシュ", pageId = "3143" },
                    new { fishId = "ミンストレルフィッシュ", pageId = "3050" },
                    new { fishId = "ムセル", pageId = "3192" },
                    new { fishId = "ムーンディスク", pageId = "688" },
                    new { fishId = "メイデンカープ", pageId = "23" },
                    new { fishId = "メイトリアーク", pageId = "282" },
                    new { fishId = "メカジキ", pageId = "632" },
                    new { fishId = "メガオクトパス", pageId = "230" },
                    new { fishId = "メガスキッド", pageId = "3224" },
                    new { fishId = "メガピラニア", pageId = "3173" },
                    new { fishId = "メガロドン", pageId = "179" },
                    new { fishId = "メサン＝チェラ", pageId = "3157" },
                    new { fishId = "メダカ", pageId = "574" },
                    new { fishId = "メテオサバイバー", pageId = "239" },
                    new { fishId = "メテオトータス", pageId = "467" },
                    new { fishId = "メルトールゴビー", pageId = "4" },
                    new { fishId = "メルトールバタフライ", pageId = "3213" },
                    new { fishId = "メルトールロブスター", pageId = "3207" },
                    new { fishId = "メンダコ", pageId = "3107" },
                    new { fishId = "モクズガニ", pageId = "670" },
                    new { fishId = "モグルグポンポン", pageId = "482" },
                    new { fishId = "モササウルス", pageId = "733" },
                    new { fishId = "モックピクシー", pageId = "3149" },
                    new { fishId = "モッピィビアード", pageId = "3260" },
                    new { fishId = "モモラ・モラ", pageId = "3212" },
                    new { fishId = "モヨウモンガラドオシ", pageId = "546" },
                    new { fishId = "モラビーフロウンダー", pageId = "22" },
                    new { fishId = "モラモラ", pageId = "171" },
                    new { fishId = "モリナバルウ", pageId = "183" },
                    new { fishId = "モルバ", pageId = "206" },
                    new { fishId = "モンクフィッシュ", pageId = "61" },
                    new { fishId = "モンクベタ", pageId = "533" },
                    new { fishId = "モンケオンケ", pageId = "86" },
                    new { fishId = "ヤツメウナギ", pageId = "68" },
                    new { fishId = "ヤトカガン", pageId = "773" },
                    new { fishId = "ヤトゴビー", pageId = "582" },
                    new { fishId = "ヤルムロブスター", pageId = "418" },
                    new { fishId = "ヤンサアカメ", pageId = "572" },
                    new { fishId = "ヤンサコイ", pageId = "569" },
                    new { fishId = "ユグラムサーモン", pageId = "71" },
                    new { fishId = "ユノハナガニ", pageId = "695" },
                    new { fishId = "ユーリノサウルス", pageId = "445" },
                    new { fishId = "ユールモアバタフライ", pageId = "3020" },
                    new { fishId = "ヨウガンナマズ", pageId = "420" },
                    new { fishId = "ヨツメウオ", pageId = "53" },
                    new { fishId = "ライキナマズ", pageId = "548" },
                    new { fishId = "ライラックゴビー", pageId = "3114" },
                    new { fishId = "ラクサンインクホーン", pageId = "3187" },
                    new { fishId = "ラクサンカープ", pageId = "3008" },
                    new { fishId = "ラケティカゴビー", pageId = "3178" },
                    new { fishId = "ラケティカトラウト", pageId = "3086" },
                    new { fishId = "ラストティアー", pageId = "813" },
                    new { fishId = "ラセンガイ", pageId = "638" },
                    new { fishId = "ラタトスクソウル", pageId = "470" },
                    new { fishId = "ラノシアンジェリー", pageId = "3226" },
                    new { fishId = "ラノシアンパーチ", pageId = "37" },
                    new { fishId = "ラバーズフラワー", pageId = "3011" },
                    new { fishId = "ラビットスキッパー", pageId = "3026" },
                    new { fishId = "ラブカ", pageId = "286" },
                    new { fishId = "ラベンダーレモラ", pageId = "93" },
                    new { fishId = "ラムフォリンクス", pageId = "168" },
                    new { fishId = "ランデロプテルス", pageId = "505" },
                    new { fishId = "ランプフィッシュ", pageId = "3219" },
                    new { fishId = "ランプマリモ", pageId = "74" },
                    new { fishId = "ラ・レアル", pageId = "464" },
                    new { fishId = "ラールガーサンダーボルト", pageId = "554" },
                    new { fishId = "ラーヴァクラブ", pageId = "345" },
                    new { fishId = "ラーヴァスネイル", pageId = "455" },
                    new { fishId = "リトルサラオス", pageId = "155" },
                    new { fishId = "リトルビスマルク", pageId = "3131" },
                    new { fishId = "リトルペリュコス", pageId = "637" },
                    new { fishId = "リトルリヴァイアサン", pageId = "3232" },
                    new { fishId = "リドル", pageId = "495" },
                    new { fishId = "リバークラブ", pageId = "29" },
                    new { fishId = "リバーシュリンプ", pageId = "510" },
                    new { fishId = "リフトセイラー", pageId = "142" },
                    new { fishId = "リベットオイスター", pageId = "234" },
                    new { fishId = "リムレーンズソード", pageId = "216" },
                    new { fishId = "リムレーンズダガー", pageId = "38" },
                    new { fishId = "リムレーンプリント", pageId = "3239" },
                    new { fishId = "リンゴガイ", pageId = "803" },
                    new { fishId = "リーフィーシードラゴン", pageId = "81" },
                    new { fishId = "リーフフィッシュ", pageId = "383" },
                    new { fishId = "ルリカレイ", pageId = "558" },
                    new { fishId = "ルリニシン", pageId = "540" },
                    new { fishId = "ルリマグロ", pageId = "605" },
                    new { fishId = "ルートスキッパー", pageId = "80" },
                    new { fishId = "レイクアーチン", pageId = "320" },
                    new { fishId = "レイクラウス", pageId = "3116" },
                    new { fishId = "レイクランドコッド", pageId = "3130" },
                    new { fishId = "レイザーフィッシュ", pageId = "3047" },
                    new { fishId = "レイスフィッシュ", pageId = "635" },
                    new { fishId = "レインボーシュリンプ", pageId = "3111" },
                    new { fishId = "レインボートラウト", pageId = "28" },
                    new { fishId = "レオパードイール", pageId = "3210" },
                    new { fishId = "レックドクロス", pageId = "3246" },
                    new { fishId = "レッドテイル", pageId = "750" },
                    new { fishId = "レッドテイルゾンビー", pageId = "809" },
                    new { fishId = "レッドハンマーヘッド", pageId = "3021" },
                    new { fishId = "レモンフィッシュ", pageId = "3072" },
                    new { fishId = "レヴィンライト", pageId = "256" },
                    new { fishId = "ロイヤルプレコ", pageId = "112" },
                    new { fishId = "ロイヤルマント", pageId = "691" },
                    new { fishId = "ロウニン", pageId = "666" },
                    new { fishId = "ロガサウルス", pageId = "3252" },
                    new { fishId = "ロズリトオイスター", pageId = "35" },
                    new { fishId = "ロックオイスター", pageId = "700" },
                    new { fishId = "ロッククライマー", pageId = "411" },
                    new { fishId = "ロックスケイル", pageId = "3277" },
                    new { fishId = "ロックソルトフィッシュ", pageId = "608" },
                    new { fishId = "ロックフィッシュ", pageId = "736" },
                    new { fishId = "ロックロブスター", pageId = "149" },
                    new { fishId = "ロックワの衛士", pageId = "3185" },
                    new { fishId = "ロツァトルピラルク", pageId = "3175" },
                    new { fishId = "ロバークラブ", pageId = "3081" },
                    new { fishId = "ロミンサンアンチョビ", pageId = "5" },
                    new { fishId = "ロンカプレコ", pageId = "3167" },
                    new { fishId = "ローズシュリンプ", pageId = "3048" },
                    new { fishId = "ローズブリーム", pageId = "3201" },
                    new { fishId = "ロータノサーディン", pageId = "3216" },
                    new { fishId = "ロータノワフー", pageId = "3215" },
                    new { fishId = "ロープフィッシュ", pageId = "76" },
                    new { fishId = "ローンリッパー", pageId = "262" },
                    new { fishId = "ワイルドアーチン", pageId = "3258" },
                    new { fishId = "ワイルドレッドベタ", pageId = "3074" },
                    new { fishId = "ワッツトラウト", pageId = "3037" },
                    new { fishId = "ワニガメ", pageId = "749" },
                    new { fishId = "ワフー", pageId = "116" },
                    new { fishId = "ワンダラースカルピン", pageId = "151" },
                    new { fishId = "ワンダリングキャットフィッシュ", pageId = "3013" },
                    new { fishId = "ワーデンズワンド", pageId = "254" },
                    new { fishId = "ワーデンフィッシュ", pageId = "3123" },
                    new { fishId = "ワーム・オブ・ニーム", pageId = "252" },
                    new { fishId = "ンデンデキ", pageId = "301" },
                    new { fishId = "ヴァイオラクラム", pageId = "3126" },
                    new { fishId = "ヴァンパイアウィップ", pageId = "302" },
                    new { fishId = "ヴァンパイアブランケット", pageId = "434" },
                    new { fishId = "ヴィゾーヴニル", pageId = "469" },
                    new { fishId = "ヴィースイヤー", pageId = "3177" },
                    new { fishId = "ヴォイドバス", pageId = "295" },
                    new { fishId = "七彩天主", pageId = "844" },
                    new { fishId = "万能のゴブリバス", pageId = "487" },
                    new { fishId = "上質の硬鱗魚", pageId = "801" },
                    new { fishId = "不忠蛇", pageId = "726" },
                    new { fishId = "乳白珊瑚", pageId = "3016" },
                    new { fishId = "人面魚", pageId = "223" },
                    new { fishId = "人食貝", pageId = "3108" },
                    new { fishId = "仙寿の翁", pageId = "828" },
                    new { fishId = "侍魚", pageId = "593" },
                    new { fishId = "供物の小魚", pageId = "785" },
                    new { fishId = "光鱗魚", pageId = "644" },
                    new { fishId = "八角", pageId = "723" },
                    new { fishId = "具足海老", pageId = "703" },
                    new { fishId = "出目金", pageId = "339" },
                    new { fishId = "千牙竜", pageId = "721" },
                    new { fishId = "半生魚", pageId = "678" },
                    new { fishId = "古ぼけた地図G1", pageId = "901" },
                    new { fishId = "古ぼけた地図G10", pageId = "910" },
                    new { fishId = "古ぼけた地図G11", pageId = "911" },
                    new { fishId = "古ぼけた地図G12", pageId = "912" },
                    new { fishId = "古ぼけた地図G2", pageId = "902" },
                    new { fishId = "古ぼけた地図G3", pageId = "903" },
                    new { fishId = "古ぼけた地図G4", pageId = "904" },
                    new { fishId = "古ぼけた地図G5", pageId = "905" },
                    new { fishId = "古ぼけた地図G6", pageId = "906" },
                    new { fishId = "古ぼけた地図G7", pageId = "907" },
                    new { fishId = "古ぼけた地図G8", pageId = "908" },
                    new { fishId = "古ぼけた地図G9", pageId = "909" },
                    new { fishId = "古銭貝", pageId = "664" },
                    new { fishId = "名馬の魂", pageId = "728" },
                    new { fishId = "堀ブナ", pageId = "39" },
                    new { fishId = "夕焼貝", pageId = "797" },
                    new { fishId = "夕空珊瑚", pageId = "395" },
                    new { fishId = "大樹の鱗", pageId = "3166" },
                    new { fishId = "大鈍甲", pageId = "157" },
                    new { fishId = "天女魚", pageId = "620" },
                    new { fishId = "天幕魚", pageId = "581" },
                    new { fishId = "天狗団扇", pageId = "682" },
                    new { fishId = "天空珊瑚", pageId = "441" },
                    new { fishId = "太ったトラウト", pageId = "802" },
                    new { fishId = "太陽貝", pageId = "779" },
                    new { fishId = "学士貝", pageId = "390" },
                    new { fishId = "寺ブナ", pageId = "519" },
                    new { fishId = "射手魚", pageId = "125" },
                    new { fishId = "小流星", pageId = "535" },
                    new { fishId = "嵐神魚", pageId = "448" },
                    new { fishId = "川の長老", pageId = "756" },
                    new { fishId = "巨大オオナマズ", pageId = "789" },
                    new { fishId = "常闇魚", pageId = "3091" },
                    new { fishId = "弓魚", pageId = "589" },
                    new { fishId = "御鮭様", pageId = "597" },
                    new { fishId = "心貝", pageId = "509" },
                    new { fishId = "意地ブナ", pageId = "224" },
                    new { fishId = "招嵐王", pageId = "236" },
                    new { fishId = "斬馬魚", pageId = "686" },
                    new { fishId = "方士魚", pageId = "724" },
                    new { fishId = "明けの旗魚", pageId = "834" },
                    new { fishId = "春不知", pageId = "768" },
                    new { fishId = "春告魚", pageId = "628" },
                    new { fishId = "智慧珊瑚", pageId = "3299" },
                    new { fishId = "暗紫珊瑚", pageId = "3092" },
                    new { fishId = "暮れの魚", pageId = "820" },
                    new { fishId = "月蝶貝", pageId = "795" },
                    new { fishId = "未確認飛行生物", pageId = "408" },
                    new { fishId = "杯貝", pageId = "794" },
                    new { fishId = "梅見魚", pageId = "633" },
                    new { fishId = "槍穂貝", pageId = "800" },
                    new { fishId = "橙彩魚", pageId = "759" },
                    new { fishId = "水墨魚", pageId = "471" },
                    new { fishId = "水天一碧", pageId = "829" },
                    new { fishId = "水泡眼", pageId = "3078" },
                    new { fishId = "水瓶王", pageId = "468" },
                    new { fishId = "氷の巫女", pageId = "488" },
                    new { fishId = "氷神魚", pageId = "365" },
                    new { fishId = "永遠の眼", pageId = "727" },
                    new { fishId = "泥仙人", pageId = "671" },
                    new { fishId = "海の流星", pageId = "3234" },
                    new { fishId = "海チョコチョコボ", pageId = "636" },
                    new { fishId = "溶岩帝王", pageId = "496" },
                    new { fishId = "溶岩王", pageId = "432" },
                    new { fishId = "漁魚", pageId = "653" },
                    new { fishId = "焔魚", pageId = "725" },
                    new { fishId = "無二草魚", pageId = "592" },
                    new { fishId = "煌魚", pageId = "176" },
                    new { fishId = "牛皮魚", pageId = "796" },
                    new { fishId = "獄海月", pageId = "541" },
                    new { fishId = "王魚", pageId = "641" },
                    new { fishId = "白彩魚", pageId = "758" },
                    new { fishId = "白珊瑚", pageId = "10" },
                    new { fishId = "白紙魚", pageId = "595" },
                    new { fishId = "白蝶貝", pageId = "88" },
                    new { fishId = "白金魚", pageId = "376" },
                    new { fishId = "白銀貝", pageId = "777" },
                    new { fishId = "白雲珊瑚", pageId = "315" },
                    new { fishId = "白骨珊瑚", pageId = "544" },
                    new { fishId = "盆栽魚", pageId = "753" },
                    new { fishId = "真鍮魚", pageId = "585" },
                    new { fishId = "瞑想魚", pageId = "612" },
                    new { fishId = "矢巻貝", pageId = "782" },
                    new { fishId = "硬鱗魚", pageId = "591" },
                    new { fishId = "磯の灯篭", pageId = "692" },
                    new { fishId = "神々の愛", pageId = "845" },
                    new { fishId = "空棘魚", pageId = "672" },
                    new { fishId = "竜骨貝", pageId = "3300" },
                    new { fishId = "第二次復興用のカイマン", pageId = "3286" },
                    new { fishId = "第二次復興用のガー", pageId = "3282" },
                    new { fishId = "第二次復興用のクァールフィッシュ", pageId = "3281" },
                    new { fishId = "第二次復興用のクラウドスキッパー", pageId = "3279" },
                    new { fishId = "第二次復興用のスプリットクラウド", pageId = "3287" },
                    new { fishId = "第二次復興用のトゥプクスアラ", pageId = "3289" },
                    new { fishId = "第二次復興用のビターリング", pageId = "3285" },
                    new { fishId = "第二次復興用のピラルク", pageId = "3283" },
                    new { fishId = "第二次復興用のフライマンタ", pageId = "3290" },
                    new { fishId = "第二次復興用のブーメラン", pageId = "3284" },
                    new { fishId = "第二次復興用のヴァンパイアブランケット", pageId = "3288" },
                    new { fishId = "第二次復興用の特注アノマロカリス", pageId = "3296" },
                    new { fishId = "第二次復興用の特注コメットフィッシュ", pageId = "3295" },
                    new { fishId = "第二次復興用の特注ゴブリパス", pageId = "3292" },
                    new { fishId = "第二次復興用の特注スカイフィッシュ", pageId = "3294" },
                    new { fishId = "第二次復興用の特注ドラゴンソウル", pageId = "3298" },
                    new { fishId = "第二次復興用の特注プテロダクティルス", pageId = "3293" },
                    new { fishId = "第二次復興用の特注ラムフォリンクス", pageId = "3297" },
                    new { fishId = "第二次復興用の特注ロマレオサウルス", pageId = "3291" },
                    new { fishId = "第二次復興用の瞑想魚", pageId = "3280" },
                    new { fishId = "紅玉海老", pageId = "545" },
                    new { fishId = "紅玉珊瑚", pageId = "542" },
                    new { fishId = "紅玉藻", pageId = "836" },
                    new { fishId = "紅珊瑚", pageId = "107" },
                    new { fishId = "紅龍", pageId = "843" },
                    new { fishId = "紫彩魚", pageId = "738" },
                    new { fishId = "絹鯉", pageId = "629" },
                    new { fishId = "綱魚", pageId = "713" },
                    new { fishId = "緑彩魚", pageId = "807" },
                    new { fishId = "罪吐き", pageId = "3012" },
                    new { fishId = "羽衣美女", pageId = "819" },
                    new { fishId = "羽衣鯉", pageId = "771" },
                    new { fishId = "聖ファスリクの怒髪", pageId = "3153" },
                    new { fishId = "聖竜の涙", pageId = "484" },
                    new { fishId = "肺魚", pageId = "351" },
                    new { fishId = "腐魚", pageId = "167" },
                    new { fishId = "茄子魚", pageId = "3124" },
                    new { fishId = "草魚", pageId = "340" },
                    new { fishId = "草鮫", pageId = "698" },
                    new { fishId = "菜食王", pageId = "823" },
                    new { fishId = "蒼玉珊瑚", pageId = "543" },
                    new { fishId = "藍彩魚", pageId = "806" },
                    new { fishId = "蛇頭魚", pageId = "679" },
                    new { fishId = "血紅龍", pageId = "312" },
                    new { fishId = "裁定魚", pageId = "631" },
                    new { fishId = "解脱魚", pageId = "815" },
                    new { fishId = "謎めいた魚", pageId = "805" },
                    new { fishId = "赤彩魚", pageId = "755" },
                    new { fishId = "赤銅魚", pageId = "787" },
                    new { fishId = "車海老", pageId = "681" },
                    new { fishId = "輪宝貝", pageId = "683" },
                    new { fishId = "逆龍", pageId = "706" },
                    new { fishId = "金魚", pageId = "146" },
                    new { fishId = "銀魚", pageId = "90" },
                    new { fishId = "銅鏡", pageId = "237" },
                    new { fishId = "銅魚", pageId = "41" },
                    new { fishId = "鎧魚", pageId = "502" },
                    new { fishId = "雨乞魚", pageId = "117" },
                    new { fishId = "雪乞魚", pageId = "363" },
                    new { fishId = "雲海紅珊瑚", pageId = "508" },
                    new { fishId = "雷皇子", pageId = "272" },
                    new { fishId = "雷神魚", pageId = "174" },
                    new { fishId = "雷紋魚", pageId = "113" },
                    new { fishId = "雷遁魚", pageId = "606" },
                    new { fishId = "青彩魚", pageId = "599" },
                    new { fishId = "青珊瑚", pageId = "57" },
                    new { fishId = "青空の涙", pageId = "587" },
                    new { fishId = "青空珊瑚", pageId = "336" },
                    new { fishId = "飛天魚", pageId = "714" },
                    new { fishId = "香魚", pageId = "380" },
                    new { fishId = "魔科学物質123", pageId = "388" },
                    new { fishId = "魔科学物質666", pageId = "485" },
                    new { fishId = "魔魚", pageId = "407" },
                    new { fishId = "魚蜥蜴", pageId = "722" },
                    new { fishId = "鰭竜", pageId = "618" },
                    new { fishId = "黄彩魚", pageId = "598" },
                    new { fishId = "黄金魚", pageId = "288" },
                    new { fishId = "黒蝶貝", pageId = "120" },
                    new { fishId = "黒風魚", pageId = "3094" },
                    new { fishId = "龍魚", pageId = "596" },
                }
                .ToDictionary(item => new GameDataObjectId(GameDataObjectCategory.Fish, item.fishId), item => item.pageId);
        }

        public static void TranslateMemo(this Fish fish)
        {
            foreach (var lang in Translate.SupportedLanguages)
            {
                var memoLines =
                    fish.RawMemoText
                    .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Select(text => TranslateMemoLine(fish, text, lang))
                    .ToArray();
#if DEBUG
                if (memoLines.Any())
                {
                    var baitIds = memoLines.Select(line => line.TranslationIdOfBait).Distinct().ToArray();
                    if (baitIds.Length == 1)
                    {
                        if (fish.FishingBaits.Count() != 1)
                            throw new Exception();
                        if (baitIds[0] == _unknownBaitNameId)
                        {
                            // NOP
                        }
                        else if (baitIds[0] == fish.FishingBaits.Single().NameId)
                        {
                            // OK
                        }
                        else
                        {
                            throw new Exception(
                                string.Format(
                                    "Bad baits: fish='{0}', baits=[{1}], baitsOfMemo=[{2}]",
                                    Translate.Instance[fish.NameId, lang],
                                    string.Join(", ", fish.FishingBaits.Select(bait => string.Format("'{0}'", Translate.Instance[bait.NameId, lang]))),
                                    string.Join(", ", baitIds.Select(id => id.ToString()))));
                        }
                    }
                    else
                    {
                        baitIds = baitIds.Where(bait => bait != _unknownBaitNameId).ToArray();
                        if (baitIds.Length != fish.FishingBaits.Count())
                        {
                            throw new Exception(
                                string.Format(
                                    "Bad baits: fish='{0}', baits=[{1}], baitsOfMemo=[{2}]",
                                    Translate.Instance[fish.NameId, lang],
                                    string.Join(", ", fish.FishingBaits.Select(bait => string.Format("'{0}'", Translate.Instance[bait.NameId, lang]))),
                                    string.Join(", ", baitIds.Select(id => id.ToString()))));
                        }
                        else if (fish.FishingBaits.Select(bait => bait.NameId).Except(baitIds).Any())
                        {
                            throw new Exception(
                                string.Format(
                                    "Bad baits: fish='{0}', baits=[{1}], baitsOfMemo=[{2}]",
                                    Translate.Instance[fish.NameId, lang],
                                    string.Join(", ", fish.FishingBaits.Select(bait => string.Format("'{0}'", Translate.Instance[bait.NameId, lang]))),
                                    string.Join(", ", baitIds.Select(id => id.ToString()))));
                        }
                        else
                        {
                            // OK
                        }

                    }
                }
#endif
                var translatedMemo =
                    string.Join("\n", memoLines.Select(line => line.Text));
                Translate.Instance.Add(fish.DefaultMemoId, lang, translatedMemo);
            }
        }

        public static string GetCBHLink(this Fish fish)
        {
            string pageId;
            if (!_pageIdSourceOfCBH.TryGetValue(fish.Id, out pageId))
                return null;
            var format = Translate.Instance[new TranslationTextId(TranslationCategory.Url, "CBH.FishPage")];
            return string.Format(format, pageId);
        }

        private static MemoLine TranslateMemoLine(Fish fish, string text, string lang)
        {
            Match m;
            if ((m = _直釣りパターン.Match(text)).Success)
            {
                var requires = m.Groups["requires"].Success ? TranslateRequirement(m.Groups["requires"].Value, lang) : null;
                var baitId = new TranslationTextId(TranslationCategory.FishingBait, m.Groups["bait"].Value);
                var baitName = Translate.Instance[baitId, lang];
                var hooking = TranslateHooking(m.Groups["atari"].Value, m.Groups["hooking"].Value, lang);
                return new MemoLine
                {
                    Text =
                        string.Format(
                            "{0}{1}⇒({2}) {3}",
                            requires != null ? string.Format("({0}) ", requires.TranslatedRequirement) : "",
                            baitName,
                            hooking,
                            requires != null && requires.RequiredFishName != null ? requires.RequiredFishName : Translate.Instance[fish.NameId, lang]),
                    TranslationIdOfBait = baitId,
                };
            }
            else if ((m = _泳がせ釣りパターン.Match(text)).Success)
            {
                var requires = m.Groups["requires"].Success ? TranslateRequirement(m.Groups["requires"].Value, lang) : null;
                var bait1Id = new TranslationTextId(TranslationCategory.FishingBait, m.Groups["bait1"].Value);
                var bait1Name = Translate.Instance[bait1Id, lang];
                var hooking1 = TranslateHooking(m.Groups["atari1"].Value, m.Groups["hooking1"].Value, lang);
                var bait2Name =
                    string.Format(
                        Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "HighQualityItem"), lang],
                        Translate.Instance[new TranslationTextId(TranslationCategory.Fish, m.Groups["bait2"].Value), lang]);
                var hooking2 = TranslateHooking(m.Groups["atari2"].Value, m.Groups["hooking2"].Value, lang);
                return new MemoLine
                {
                    Text =
                        string.Format(
                            "{0}{1}⇒({2}) {3}⇒({4}) {5}",
                            requires != null ? string.Format("({0}) ", requires.TranslatedRequirement) : "",
                            bait1Name,
                            hooking1,
                            bait2Name,
                            hooking2,
                            requires != null && requires.RequiredFishName != null ? requires.RequiredFishName : Translate.Instance[fish.NameId, lang]),
                    TranslationIdOfBait = bait1Id,
                };
            }
            else if ((m = _2段泳がせ釣りパターン.Match(text)).Success)
            {
                var requires = m.Groups["requires"].Success ? TranslateRequirement(m.Groups["requires"].Value, lang) : null;
                var bait1Id = new TranslationTextId(TranslationCategory.FishingBait, m.Groups["bait1"].Value);
                var bait1Name = Translate.Instance[bait1Id, lang];
                var hooking1 = TranslateHooking(m.Groups["atari1"].Value, m.Groups["hooking1"].Value, lang);
                var bait2Name =
                    string.Format(
                        Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "HighQualityItem"), lang],
                        Translate.Instance[new TranslationTextId(TranslationCategory.Fish, m.Groups["bait2"].Value), lang]);
                var hooking2 = TranslateHooking(m.Groups["atari2"].Value, m.Groups["hooking2"].Value, lang);
                var bait3Name =
                    string.Format(
                        Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "HighQualityItem"), lang],
                        Translate.Instance[new TranslationTextId(TranslationCategory.Fish, m.Groups["bait3"].Value), lang]);
                var hooking3 = TranslateHooking(m.Groups["atari3"].Value, m.Groups["hooking3"].Value, lang);
                return new MemoLine
                {
                    Text =
                        string.Format(
                            "{0}{1}⇒({2}) {3}⇒({4}) {5}⇒({6}) {7}",
                            requires != null ? string.Format("({0}) ", requires.TranslatedRequirement) : "",
                            bait1Name,
                            hooking1,
                            bait2Name,
                            hooking2,
                            bait3Name,
                            hooking3,
                            requires != null && requires.RequiredFishName != null ? requires.RequiredFishName : Translate.Instance[fish.NameId, lang]),
                    TranslationIdOfBait = bait1Id,
                };
            }
            else if ((m = _3段泳がせ釣りパターン.Match(text)).Success)
            {
                var requires = m.Groups["requires"].Success ? TranslateRequirement(m.Groups["requires"].Value, lang) : null;
                var bait1Id = new TranslationTextId(TranslationCategory.FishingBait, m.Groups["bait1"].Value);
                var bait1Name = Translate.Instance[bait1Id, lang];
                var hooking1 = TranslateHooking(m.Groups["atari1"].Value, m.Groups["hooking1"].Value, lang);
                var bait2Name =
                    string.Format(
                        Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "HighQualityItem"), lang],
                        Translate.Instance[new TranslationTextId(TranslationCategory.Fish, m.Groups["bait2"].Value), lang]);
                var hooking2 = TranslateHooking(m.Groups["atari2"].Value, m.Groups["hooking2"].Value, lang);
                var bait3Name =
                    string.Format(
                        Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "HighQualityItem"), lang],
                        Translate.Instance[new TranslationTextId(TranslationCategory.Fish, m.Groups["bait3"].Value), lang]);
                var hooking3 = TranslateHooking(m.Groups["atari3"].Value, m.Groups["hooking3"].Value, lang);
                var bait4Name =
                    string.Format(
                        Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "HighQualityItem"), lang],
                        Translate.Instance[new TranslationTextId(TranslationCategory.Fish, m.Groups["bait4"].Value), lang]);
                var hooking4 = TranslateHooking(m.Groups["atari4"].Value, m.Groups["hooking4"].Value, lang);
                return new MemoLine
                {
                    Text =
                        string.Format(
                            "{0}{1}⇒({2}) {3}⇒({4}) {5}⇒({6}) {7}⇒({8}) {9}",
                            requires != null ? string.Format("({0}) ", requires.TranslatedRequirement) : "",
                            bait1Name,
                            hooking1,
                            bait2Name,
                            hooking2,
                            bait3Name,
                            hooking3,
                            bait4Name,
                            hooking4,
                            requires != null && requires.RequiredFishName != null ? requires.RequiredFishName : Translate.Instance[fish.NameId, lang]),
                    TranslationIdOfBait = bait1Id,
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
                throw new Exception(string.Format("Bad memo format. fish='{0}'", fish.DefaultMemoId));
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
                            TranslatedRequirement =
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
                .Select(w => w.TryParseAsWeather())
                .ToArray();
            if (weathers.Any(w => w == WeatherType.None))
                return null;
            return string.Join("/", weathers.Select(w => Translate.Instance[new TranslationTextId(TranslationCategory.Weather, w.ToString()), lang]));
        }

    }
}
