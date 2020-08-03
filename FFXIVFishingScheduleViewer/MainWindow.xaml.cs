using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FFXIVFishingScheduleViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
        : Window
    {
        private ISettingProvider _settingProvider;
        private DataContext _dataContext;
        private AreaGroupCollection _areaGroups;
        private FishingSpotCollection _fishingSpots;
        private FishingBaitCollection _fishingBaites;
        private FishCollection _fishes;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            UpgradeSettings();

            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));

            InitializeComponent();

            InitializeData();
            _settingProvider = new SettingProvider(_fishes);
            _dataContext = new DataContext(Dispatcher, _areaGroups, _fishes, _settingProvider);
            DataContext = _dataContext;
            _dataContext.ShowDownloadPageCommand = new SimpleCommand(p =>
            {
                System.Diagnostics.Process.Start(_dataContext.UrlOfDownloadPage);
            });
            _dataContext.OptionMenuCommand = new SimpleCommand(p =>
            {
                var dialog = new OptionWindow();
                dialog.Owner = this;
                dialog.DataContext = _dataContext;
                dialog.ShowDialog();
            });
            _dataContext.ExitMenuCommand = new SimpleCommand(p => Close());
            _dataContext.About.ViewREADMEMenuCommand = new SimpleCommand(p =>
            {
                System.Diagnostics.Process.Start(_dataContext.About.READMEUrlText);
            });
            _dataContext.About.AboutMenuCommand = new SimpleCommand(p =>
            {
                var dialog = new AboutWindow();
                dialog.Owner = this;
                dialog.DataContext = _dataContext.About;
                dialog.ShowDialog();
            });
            RecoverWindowBounds();

#if DEBUG
            SelfCheck();
#endif
            _dataContext.CurrentTime = DateTime.UtcNow;
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            UpdateTimer();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_timer.IsEnabled)
                _timer.Stop();
            _timer.Tick -= _timer_Tick;
            SaveWindowBounds();
            base.OnClosing(e);
        }

        private static void UpgradeSettings()
        {
            if (!Properties.Settings.Default.IsUpgraded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.IsUpgraded = true;
                Properties.Settings.Default.Save();
            }
        }

        private void RecoverWindowBounds()
        {
            var settings = Properties.Settings.Default;
            if (!double.IsNaN(settings.MainWindowLeft) && !double.IsNaN(settings.MainWindowTop) && !double.IsNaN(settings.MainWindowWidth) && !double.IsNaN(settings.MainWindowHeight))
            {
                if (settings.MainWindowLeft >= 0 &&
                    (settings.MainWindowLeft + settings.MainWindowWidth) < SystemParameters.VirtualScreenWidth)
                {
                    Left = settings.MainWindowLeft;
                }

                if (settings.MainWindowTop >= 0 &&
                    (settings.MainWindowTop + settings.MainWindowHeight) < SystemParameters.VirtualScreenHeight)
                {
                    Top = settings.MainWindowTop;
                }

                if (settings.MainWindowWidth > 0 &&
                    settings.MainWindowWidth <= SystemParameters.WorkArea.Width)
                {
                    Width = settings.MainWindowWidth;
                }

                if (settings.MainWindowHeight > 0 &&
                    settings.MainWindowHeight <= SystemParameters.WorkArea.Height)
                {
                    Height = settings.MainWindowHeight;
                }
            }
            if (settings.MainWindowMaximized)
            {
                Loaded += (o, e) => WindowState = WindowState.Maximized;
            }
        }

        private void SaveWindowBounds()
        {
            WindowState = WindowState.Normal; // 最大化解除
            var settings = Properties.Settings.Default;
            settings.MainWindowMaximized = WindowState == WindowState.Maximized;
            settings.MainWindowLeft = Left;
            settings.MainWindowTop = Top;
            settings.MainWindowWidth = Width;
            settings.MainWindowHeight = Height;
            settings.Save();
        }

        private void UpdateTimer()
        {
            var nextTime = (_dataContext.CurrentTime.ToEorzeaDateTime().GetStartOfMinute() + EorzeaTimeSpan.FromMinutes(1)).ToEarthDateTime();
            var now = DateTime.UtcNow;
            while (nextTime - now <= TimeSpan.FromSeconds(1))
                nextTime = (nextTime.ToEorzeaDateTime() + EorzeaTimeSpan.FromMinutes(1)).ToEarthDateTime();
            var interval = nextTime - now;
            _timer.Interval = interval;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            _dataContext.CurrentTime = DateTime.UtcNow;
            UpdateTimer();
        }

        private void InitializeData()
        {
            _areaGroups = new AreaGroupCollection();
            {
                var areaGroup = new AreaGroup("ラノシア");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), };
                    var area = new Area(areaGroup, "リムサ・ロミンサ：上甲板層", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "リムサ・ロミンサ：上甲板層"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), };
                    var area = new Area(areaGroup, "リムサ・ロミンサ：下甲板層", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "リムサ・ロミンサ：下甲板層"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), };
                    var area = new Area(areaGroup, "中央ラノシア", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ゼファードリフト沿岸"));
                    area.FishingSpots.Add(new FishingSpot(area, "ローグ川"));
                    area.FishingSpots.Add(new FishingSpot(area, "西アジェレス川"));
                    area.FishingSpots.Add(new FishingSpot(area, "サマーフォード沿岸"));
                    area.FishingSpots.Add(new FishingSpot(area, "ニーム川"));
                    area.FishingSpots.Add(new FishingSpot(area, "ささやきの谷"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), };
                    var area = new Area(areaGroup, "低地ラノシア", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "モーニングウィドー"));
                    area.FishingSpots.Add(new FishingSpot(area, "モラビー湾西岸"));
                    area.FishingSpots.Add(new FishingSpot(area, "シダーウッド沿岸部"));
                    area.FishingSpots.Add(new FishingSpot(area, "オシュオン灯台"));
                    area.FishingSpots.Add(new FishingSpot(area, "キャンドルキープ埠頭"));
                    area.FishingSpots.Add(new FishingSpot(area, "モラビー造船廠"));
                    area.FishingSpots.Add(new FishingSpot(area, "エンプティハート"));
                    area.FishingSpots.Add(new FishingSpot(area, "ソルトストランド"));
                    area.FishingSpots.Add(new FishingSpot(area, "ブラインドアイアン坑道"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.霧, 5), new WeatherPercentageOfArea(WeatherType.快晴, 45), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), new WeatherPercentageOfArea(WeatherType.暴雨, 5), };
                    var area = new Area(areaGroup, "東ラノシア", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "南ブラッドショア"));
                    area.FishingSpots.Add(new FishingSpot(area, "コスタ・デル・ソル"));
                    area.FishingSpots.Add(new FishingSpot(area, "北ブラッドショア"));
                    area.FishingSpots.Add(new FishingSpot(area, "ロータノ海沖合：船首"));
                    area.FishingSpots.Add(new FishingSpot(area, "ロータノ海沖合：船尾"));
                    area.FishingSpots.Add(new FishingSpot(area, "隠れ滝"));
                    area.FishingSpots.Add(new FishingSpot(area, "東アジェレス川"));
                    area.FishingSpots.Add(new FishingSpot(area, "レインキャッチャー樹林"));
                    area.FishingSpots.Add(new FishingSpot(area, "レインキャッチャー沼沢地"));
                    area.FishingSpots.Add(new FishingSpot(area, "レッドマンティス滝"));
                    area.FishingSpots.Add(new FishingSpot(area, "常夏の島北"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.暴風, 10), };
                    var area = new Area(areaGroup, "西ラノシア", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "スウィフトパーチ入植地"));
                    area.FishingSpots.Add(new FishingSpot(area, "スカルバレー沿岸部"));
                    area.FishingSpots.Add(new FishingSpot(area, "ブルワーズ灯台"));
                    area.FishingSpots.Add(new FishingSpot(area, "ハーフストーン沿岸部"));
                    area.FishingSpots.Add(new FishingSpot(area, "幻影諸島北岸"));
                    area.FishingSpots.Add(new FishingSpot(area, "船の墓場"));
                    area.FishingSpots.Add(new FishingSpot(area, "サプサ産卵地"));
                    area.FishingSpots.Add(new FishingSpot(area, "幻影諸島南岸"));
                    area.FishingSpots.Add(new FishingSpot(area, "船隠しの港"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雷, 10), new WeatherPercentageOfArea(WeatherType.雷雨, 10), };
                    var area = new Area(areaGroup, "高地ラノシア", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "オークウッド"));
                    area.FishingSpots.Add(new FishingSpot(area, "愚か者の滝"));
                    area.FishingSpots.Add(new FishingSpot(area, "ブロンズレイク・シャロー"));
                    area.FishingSpots.Add(new FishingSpot(area, "ブロンズレイク北東岸"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.霧, 15), new WeatherPercentageOfArea(WeatherType.雨, 15), };
                    var area = new Area(areaGroup, "外地ラノシア", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ロングクライム渓谷"));
                    area.FishingSpots.Add(new FishingSpot(area, "ブロンズレイク北西岸"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), };
                    var area = new Area(areaGroup, "ミスト・ヴィレッジ", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ミスト・ヴィレッジ"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("黒衣森");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雨, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 15), };
                    var area = new Area(areaGroup, "グリダニア：新市街", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "グリダニア：翡翠湖畔"));
                    area.FishingSpots.Add(new FishingSpot(area, "グリダニア：紅茶川水系下流"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雨, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 15), };
                    var area = new Area(areaGroup, "グリダニア：旧市街", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "グリダニア：囁きの渓谷"));
                    area.FishingSpots.Add(new FishingSpot(area, "グリダニア：紅茶川水系上流"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雷, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 15), };
                    var area = new Area(areaGroup, "黒衣森：中央森林", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "葉脈水系"));
                    area.FishingSpots.Add(new FishingSpot(area, "鏡池"));
                    area.FishingSpots.Add(new FishingSpot(area, "エバーシェイド"));
                    area.FishingSpots.Add(new FishingSpot(area, "芽吹の池"));
                    area.FishingSpots.Add(new FishingSpot(area, "ハウケタ御用邸"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雷, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 15), };
                    var area = new Area(areaGroup, "黒衣森：東部森林", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "花蜜桟橋"));
                    area.FishingSpots.Add(new FishingSpot(area, "さざなみ小川"));
                    area.FishingSpots.Add(new FishingSpot(area, "十二神大聖堂"));
                    area.FishingSpots.Add(new FishingSpot(area, "青翠の奈落"));
                    area.FishingSpots.Add(new FishingSpot(area, "シルフランド渓谷"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.霧, 5), new WeatherPercentageOfArea(WeatherType.雷雨, 5), new WeatherPercentageOfArea(WeatherType.雷, 15), new WeatherPercentageOfArea(WeatherType.霧, 5), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.快晴, 30), };
                    var area = new Area(areaGroup, "黒衣森：南部森林", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ハズーバ支流：上流"));
                    area.FishingSpots.Add(new FishingSpot(area, "ハズーバ支流：下流"));
                    area.FishingSpots.Add(new FishingSpot(area, "ハズーバ支流：東"));
                    area.FishingSpots.Add(new FishingSpot(area, "ハズーバ支流：中流"));
                    area.FishingSpots.Add(new FishingSpot(area, "ゴブリン族の生簀"));
                    area.FishingSpots.Add(new FishingSpot(area, "根渡り沼"));
                    area.FishingSpots.Add(new FishingSpot(area, "ウルズの恵み"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.霧, 5), new WeatherPercentageOfArea(WeatherType.暴雨, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 5), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.快晴, 30), };
                    var area = new Area(areaGroup, "黒衣森：北部森林", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "さざめき川"));
                    area.FishingSpots.Add(new FishingSpot(area, "フォールゴウド秋瓜湖畔"));
                    area.FishingSpots.Add(new FishingSpot(area, "プラウドクリーク"));
                    area.FishingSpots.Add(new FishingSpot(area, "タホトトル湖畔"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 15), };
                    var area = new Area(areaGroup, "ラベンダーベッド", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ラベンダーベッド"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("ザナラーン");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 25), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), };
                    var area = new Area(areaGroup, "ウルダハ：ナル回廊", weatherOfArea);
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 25), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), };
                    var area = new Area(areaGroup, "ウルダハ：ザル回廊", weatherOfArea);
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 25), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), };
                    var area = new Area(areaGroup, "西ザナラーン", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ノフィカの井戸"));
                    area.FishingSpots.Add(new FishingSpot(area, "足跡の谷"));
                    area.FishingSpots.Add(new FishingSpot(area, "ベスパーベイ"));
                    area.FishingSpots.Add(new FishingSpot(area, "クレセントコーヴ"));
                    area.FishingSpots.Add(new FishingSpot(area, "シルバーバザー"));
                    area.FishingSpots.Add(new FishingSpot(area, "ウエストウインド岬"));
                    area.FishingSpots.Add(new FishingSpot(area, "ムーンドリップ洞窟"));
                    area.FishingSpots.Add(new FishingSpot(area, "パラタの墓所"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.砂塵, 15), new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), };
                    var area = new Area(areaGroup, "中央ザナラーン", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "スートクリーク上流"));
                    area.FishingSpots.Add(new FishingSpot(area, "スートクリーク下流"));
                    area.FishingSpots.Add(new FishingSpot(area, "クラッチ狭間"));
                    area.FishingSpots.Add(new FishingSpot(area, "アンホーリーエアー"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), new WeatherPercentageOfArea(WeatherType.暴雨, 15), };
                    var area = new Area(areaGroup, "東ザナラーン", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ドライボーン北湧水地"));
                    area.FishingSpots.Add(new FishingSpot(area, "ドライボーン南湧水地"));
                    area.FishingSpots.Add(new FishingSpot(area, "ユグラム川"));
                    area.FishingSpots.Add(new FishingSpot(area, "バーニングウォール"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.灼熱波, 20), new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), };
                    var area = new Area(areaGroup, "南ザナラーン", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "リザードクリーク"));
                    area.FishingSpots.Add(new FishingSpot(area, "ザハラクの湧水"));
                    area.FishingSpots.Add(new FishingSpot(area, "忘れられたオアシス"));
                    area.FishingSpots.Add(new FishingSpot(area, "サゴリー砂海"));
                    area.FishingSpots.Add(new FishingSpot(area, "サゴリー砂丘"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 5), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.曇り, 30), new WeatherPercentageOfArea(WeatherType.霧, 50), };
                    var area = new Area(areaGroup, "北ザナラーン", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ブルーフォグ湧水地"));
                    area.FishingSpots.Add(new FishingSpot(area, "青燐泉"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 25), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), };
                    var area = new Area(areaGroup, "ゴブレットビュート", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ゴブレットビュート"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("クルザス");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雪, 60), new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.快晴, 5), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), };
                    var area = new Area(areaGroup, "イシュガルド：上層", weatherOfArea);
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雪, 60), new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.快晴, 5), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), };
                    var area = new Area(areaGroup, "イシュガルド：下層", weatherOfArea);
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.吹雪, 20), new WeatherPercentageOfArea(WeatherType.雪, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.快晴, 5), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), };
                    var area = new Area(areaGroup, "クルザス中央高地", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "クルザス川"));
                    area.FishingSpots.Add(new FishingSpot(area, "聖ダナフェンの旅程"));
                    area.FishingSpots.Add(new FishingSpot(area, "剣ヶ峰山麓"));
                    area.FishingSpots.Add(new FishingSpot(area, "キャンプ・ドラゴンヘッド溜池"));
                    area.FishingSpots.Add(new FishingSpot(area, "調査隊の氷穴"));
                    area.FishingSpots.Add(new FishingSpot(area, "聖ダナフェンの落涙"));
                    area.FishingSpots.Add(new FishingSpot(area, "スノークローク大氷壁"));
                    area.FishingSpots.Add(new FishingSpot(area, "イシュガルド大雲海"));
                    area.FishingSpots.Add(new FishingSpot(area, "ウィッチドロップ"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.吹雪, 20), new WeatherPercentageOfArea(WeatherType.雪, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.快晴, 5), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), };
                    var area = new Area(areaGroup, "クルザス西部高地", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "リバーズミート"));
                    area.FishingSpots.Add(new FishingSpot(area, "グレイテール滝"));
                    area.FishingSpots.Add(new FishingSpot(area, "クルザス不凍池"));
                    area.FishingSpots.Add(new FishingSpot(area, "クリアプール"));
                    area.FishingSpots.Add(new FishingSpot(area, "ドラゴンスピット"));
                    area.FishingSpots.Add(new FishingSpot(area, "ベーンプール南"));
                    area.FishingSpots.Add(new FishingSpot(area, "アッシュプール"));
                    area.FishingSpots.Add(new FishingSpot(area, "ベーンプール西"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("モードゥナ");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 15), new WeatherPercentageOfArea(WeatherType.妖霧, 30), new WeatherPercentageOfArea(WeatherType.快晴, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 25), };
                    var area = new Area(areaGroup, "モードゥナ", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "銀泪湖北岸"));
                    area.FishingSpots.Add(new FishingSpot(area, "タングル湿林源流"));
                    area.FishingSpots.Add(new FishingSpot(area, "唄う裂谷"));
                    area.FishingSpots.Add(new FishingSpot(area, "早霜峠"));
                    area.FishingSpots.Add(new FishingSpot(area, "タングル湿林"));
                    area.FishingSpots.Add(new FishingSpot(area, "唄う裂谷北部"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("アバラシア");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.霊風, 10), };
                    var area = new Area(areaGroup, "アバラシア雲海", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ヴール・シアンシラン"));
                    area.FishingSpots.Add(new FishingSpot(area, "雲溜まり"));
                    area.FishingSpots.Add(new FishingSpot(area, "クラウドトップ"));
                    area.FishingSpots.Add(new FishingSpot(area, "ブルーウィンドウ"));
                    area.FishingSpots.Add(new FishingSpot(area, "モック・ウーグル島"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 35), new WeatherPercentageOfArea(WeatherType.曇り, 35), new WeatherPercentageOfArea(WeatherType.雷, 30), };
                    var area = new Area(areaGroup, "アジス・ラー", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "アルファ管区"));
                    area.FishingSpots.Add(new FishingSpot(area, "廃液溜まり"));
                    area.FishingSpots.Add(new FishingSpot(area, "超星間交信塔"));
                    area.FishingSpots.Add(new FishingSpot(area, "デルタ管区"));
                    area.FishingSpots.Add(new FishingSpot(area, "パプスの大樹"));
                    area.FishingSpots.Add(new FishingSpot(area, "ハビスフィア"));
                    area.FishingSpots.Add(new FishingSpot(area, "アジス・ラー旗艦島"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("ドラヴァニア");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.暴雨, 10), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), };
                    var area = new Area(areaGroup, "イディルシャイア", weatherOfArea);
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雷, 10), new WeatherPercentageOfArea(WeatherType.砂塵, 10), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), };
                    var area = new Area(areaGroup, "高地ドラヴァニア", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "悲嘆の飛泉"));
                    area.FishingSpots.Add(new FishingSpot(area, "ウィロームリバー"));
                    area.FishingSpots.Add(new FishingSpot(area, "スモーキングウェイスト"));
                    area.FishingSpots.Add(new FishingSpot(area, "餌食の台地"));
                    area.FishingSpots.Add(new FishingSpot(area, "モーン大岩窟"));
                    area.FishingSpots.Add(new FishingSpot(area, "モーン大岩窟西"));
                    area.FishingSpots.Add(new FishingSpot(area, "アネス・ソー"));
                    area.FishingSpots.Add(new FishingSpot(area, "光輪の祭壇"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.暴雨, 10), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), };
                    var area = new Area(areaGroup, "低地ドラヴァニア", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "サリャク河"));
                    area.FishingSpots.Add(new FishingSpot(area, "クイックスピル・デルタ"));
                    area.FishingSpots.Add(new FishingSpot(area, "サリャク河上流"));
                    area.FishingSpots.Add(new FishingSpot(area, "サリャク河中州"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.暴風, 10), new WeatherPercentageOfArea(WeatherType.放電, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), };
                    var area = new Area(areaGroup, "ドラヴァニア雲海", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "エイル・トーム"));
                    area.FishingSpots.Add(new FishingSpot(area, "グリーンスウォード島"));
                    area.FishingSpots.Add(new FishingSpot(area, "ウェストン・ウォーター"));
                    area.FishingSpots.Add(new FishingSpot(area, "ランドロード遺構"));
                    area.FishingSpots.Add(new FishingSpot(area, "ソーム・アル笠雲"));
                    area.FishingSpots.Add(new FishingSpot(area, "サルウーム・カシュ"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("ギラバニア");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 45), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雷, 10), };
                    var area = new Area(areaGroup, "ラールガーズリーチ", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ミラージュクリーク上流"));
                    area.FishingSpots.Add(new FishingSpot(area, "ラールガーズリーチ"));
                    area.FishingSpots.Add(new FishingSpot(area, "星導山寺院入口"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 45), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雷, 10), };
                    var area = new Area(areaGroup, "ギラバニア辺境地帯", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ティモン川"));
                    area.FishingSpots.Add(new FishingSpot(area, "夜の森"));
                    area.FishingSpots.Add(new FishingSpot(area, "流星の尾"));
                    area.FishingSpots.Add(new FishingSpot(area, "ベロジナ川"));
                    area.FishingSpots.Add(new FishingSpot(area, "ミラージュクリーク"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 50), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.砂塵, 5), };
                    var area = new Area(areaGroup, "ギラバニア山岳地帯", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "夫婦池"));
                    area.FishingSpots.Add(new FishingSpot(area, "スロウウォッシュ"));
                    area.FishingSpots.Add(new FishingSpot(area, "ヒース滝"));
                    area.FishingSpots.Add(new FishingSpot(area, "裁定者の像"));
                    area.FishingSpots.Add(new FishingSpot(area, "ブルズバス"));
                    area.FishingSpots.Add(new FishingSpot(area, "アームズ・オブ・ミード"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雷雨, 10), };
                    var area = new Area(areaGroup, "ギラバニア湖畔地帯", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ロッホ・セル湖"));
                    area.FishingSpots.Add(new FishingSpot(area, "ロッホ・セル湖底北西"));
                    area.FishingSpots.Add(new FishingSpot(area, "ロッホ・セル湖底中央"));
                    area.FishingSpots.Add(new FishingSpot(area, "ロッホ・セル湖底南東"));
                    area.FishingSpots.Add(new FishingSpot(area, "未知の漁場 (ギラバニア湖畔地帯)"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("オサード");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雷, 10), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 25), };
                    var area = new Area(areaGroup, "紅玉海", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "紅玉台場近海"));
                    area.FishingSpots.Add(new FishingSpot(area, "獄之蓋近海"));
                    area.FishingSpots.Add(new FishingSpot(area, "ベッコウ島近海"));
                    area.FishingSpots.Add(new FishingSpot(area, "沖之岩近海"));
                    area.FishingSpots.Add(new FishingSpot(area, "オノコロ島近海"));
                    area.FishingSpots.Add(new FishingSpot(area, "イサリ村沿岸"));
                    area.FishingSpots.Add(new FishingSpot(area, "ゼッキ島近海"));
                    area.FishingSpots.Add(new FishingSpot(area, "紅玉台場周辺"));
                    area.FishingSpots.Add(new FishingSpot(area, "碧のタマミズ周辺"));
                    area.FishingSpots.Add(new FishingSpot(area, "スイの里周辺"));
                    area.FishingSpots.Add(new FishingSpot(area, "アドヴェンチャー号周辺"));
                    area.FishingSpots.Add(new FishingSpot(area, "紫水宮周辺"));
                    area.FishingSpots.Add(new FishingSpot(area, "小林丸周辺"));
                    area.FishingSpots.Add(new FishingSpot(area, "未知の漁場 (紅玉海:アカククリ×10)"));
                    area.FishingSpots.Add(new FishingSpot(area, "未知の漁場 (紅玉海:イチモンジ×10)"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.暴雨, 5), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "ヤンサ", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "アオサギ池"));
                    area.FishingSpots.Add(new FishingSpot(area, "アオサギ川"));
                    area.FishingSpots.Add(new FishingSpot(area, "ナマイ村溜池"));
                    area.FishingSpots.Add(new FishingSpot(area, "無二江東"));
                    area.FishingSpots.Add(new FishingSpot(area, "無二江西"));
                    area.FishingSpots.Add(new FishingSpot(area, "梅泉郷"));
                    area.FishingSpots.Add(new FishingSpot(area, "七彩渓谷"));
                    area.FishingSpots.Add(new FishingSpot(area, "七彩溝"));
                    area.FishingSpots.Add(new FishingSpot(area, "城下船場"));
                    area.FishingSpots.Add(new FishingSpot(area, "ドマ城前"));
                    area.FishingSpots.Add(new FishingSpot(area, "無二江川底南西"));
                    area.FishingSpots.Add(new FishingSpot(area, "無二江川底南"));
                    area.FishingSpots.Add(new FishingSpot(area, "高速魔導駆逐艇L‐XXIII周辺"));
                    area.FishingSpots.Add(new FishingSpot(area, "沈没川船周辺"));
                    area.FishingSpots.Add(new FishingSpot(area, "大龍瀑川底"));
                    area.FishingSpots.Add(new FishingSpot(area, "未知の漁場 (ヤンサ)"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.暴風, 5), new WeatherPercentageOfArea(WeatherType.風, 5), new WeatherPercentageOfArea(WeatherType.雨, 7), new WeatherPercentageOfArea(WeatherType.霧, 8), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 25), };
                    var area = new Area(areaGroup, "アジムステップ", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ネム・カール"));
                    area.FishingSpots.Add(new FishingSpot(area, "ハク・カール"));
                    area.FishingSpots.Add(new FishingSpot(area, "ヤト・カール上流"));
                    area.FishingSpots.Add(new FishingSpot(area, "アジム・カート"));
                    area.FishingSpots.Add(new FishingSpot(area, "タオ・カール"));
                    area.FishingSpots.Add(new FishingSpot(area, "ヤト・カール下流"));
                    area.FishingSpots.Add(new FishingSpot(area, "ドタール・カー"));
                    area.FishingSpots.Add(new FishingSpot(area, "アジム・カート湖底西"));
                    area.FishingSpots.Add(new FishingSpot(area, "アジム・カート湖底東"));
                    area.FishingSpots.Add(new FishingSpot(area, "未知の漁場 (アジムステップ)"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.暴雨, 5), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "ドマ町人地", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ドマ町人地"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("ひんがしの国");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "クガネ", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "波止場全体"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "シロガネ", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "シロガネ"));
                    area.FishingSpots.Add(new FishingSpot(area, "シロガネ水路"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("ノルヴラント");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.雷雨, 5), };
                    var area = new Area(areaGroup, "クリスタリウム", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "三学科の座"));
                    area.FishingSpots.Add(new FishingSpot(area, "四学科の座"));
                    area.FishingSpots.Add(new FishingSpot(area, "クリスタリウム居室"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.暴風, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 15), };
                    var area = new Area(areaGroup, "ユールモア", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "廃船街"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.雷雨, 5), };
                    var area = new Area(areaGroup, "レイクランド", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "風化した裂け目"));
                    area.FishingSpots.Add(new FishingSpot(area, "錆ついた貯水池"));
                    area.FishingSpots.Add(new FishingSpot(area, "始まりの湖"));
                    area.FishingSpots.Add(new FishingSpot(area, "サレン郷"));
                    area.FishingSpots.Add(new FishingSpot(area, "ケンの島 (釣り)"));
                    area.FishingSpots.Add(new FishingSpot(area, "始まりの湖北東"));
                    area.FishingSpots.Add(new FishingSpot(area, "ケンの島 (銛)"));
                    area.FishingSpots.Add(new FishingSpot(area, "始まりの湖南東"));
                    area.FishingSpots.Add(new FishingSpot(area, "未知の漁場 (レイクランド)"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.暴風, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 15), };
                    var area = new Area(areaGroup, "コルシア島", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "ワッツリバー上流"));
                    area.FishingSpots.Add(new FishingSpot(area, "ホワイトオイルフォールズ"));
                    area.FishingSpots.Add(new FishingSpot(area, "ワッツリバー下流"));
                    area.FishingSpots.Add(new FishingSpot(area, "シャープタンの泉"));
                    area.FishingSpots.Add(new FishingSpot(area, "コルシア島沿岸西"));
                    area.FishingSpots.Add(new FishingSpot(area, "シーゲイザーの入江"));
                    area.FishingSpots.Add(new FishingSpot(area, "コルシア島沿岸東"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 45), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.砂塵, 10), new WeatherPercentageOfArea(WeatherType.灼熱波, 10), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "アム・アレーン", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "砂の川"));
                    area.FishingSpots.Add(new FishingSpot(area, "ナバースの断絶"));
                    area.FishingSpots.Add(new FishingSpot(area, "アンバーヒル"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.雷雨, 10), new WeatherPercentageOfArea(WeatherType.快晴, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), };
                    var area = new Area(areaGroup, "イル・メグ", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "手鏡の湖"));
                    area.FishingSpots.Add(new FishingSpot(area, "姿見の湖"));
                    area.FishingSpots.Add(new FishingSpot(area, "上の子らの流れ"));
                    area.FishingSpots.Add(new FishingSpot(area, "中の子らの流れ"));
                    area.FishingSpots.Add(new FishingSpot(area, "末の子らの流れ"));
                    area.FishingSpots.Add(new FishingSpot(area, "聖ファスリクの額"));
                    area.FishingSpots.Add(new FishingSpot(area, "コラードの排水溝"));
                    area.FishingSpots.Add(new FishingSpot(area, "リェー・ギア城北"));
                    area.FishingSpots.Add(new FishingSpot(area, "魚たちの街"));
                    area.FishingSpots.Add(new FishingSpot(area, "姿見の湖中央"));
                    area.FishingSpots.Add(new FishingSpot(area, "ジズム・ラーン"));
                    area.FishingSpots.Add(new FishingSpot(area, "姿見の湖南"));
                    area.FishingSpots.Add(new FishingSpot(area, "未知の漁場 (イル・メグ)"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霊風, 10), new WeatherPercentageOfArea(WeatherType.快晴, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.曇り, 15), };
                    var area = new Area(areaGroup, "ラケティカ大森林", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "トゥシ・メキタ湖"));
                    area.FishingSpots.Add(new FishingSpot(area, "血の酒坏"));
                    area.FishingSpots.Add(new FishingSpot(area, "ロツァトル川"));
                    area.FishingSpots.Add(new FishingSpot(area, "ミュリルの郷愁南"));
                    area.FishingSpots.Add(new FishingSpot(area, "ウォーヴンオウス"));
                    area.FishingSpots.Add(new FishingSpot(area, "ミュリルの落涙"));
                    area.FishingSpots.Add(new FishingSpot(area, "トゥシ・メキタ湖北"));
                    area.FishingSpots.Add(new FishingSpot(area, "ダワトリ溺没神殿"));
                    area.FishingSpots.Add(new FishingSpot(area, "トゥシ・メキタ湖中央"));
                    area.FishingSpots.Add(new FishingSpot(area, "トゥシ・メキタ湖南"));
                    area.FishingSpots.Add(new FishingSpot(area, "未知の漁場 (ラケティカ大森林)"));
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 60), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "テンペスト", weatherOfArea);
                    area.FishingSpots.Add(new FishingSpot(area, "フラウンダーの穴蔵"));
                    area.FishingSpots.Add(new FishingSpot(area, "陸人の墓標"));
                    area.FishingSpots.Add(new FishingSpot(area, "キャリバン海底谷北西"));
                    area.FishingSpots.Add(new FishingSpot(area, "キャリバンの古巣穴西"));
                    area.FishingSpots.Add(new FishingSpot(area, "キャリバンの古巣穴東"));
                    area.FishingSpots.Add(new FishingSpot(area, "プルプラ洞"));
                    area.FishingSpots.Add(new FishingSpot(area, "ノルヴラント大陸斜面"));
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            {
                var areaGroup = new AreaGroup("エウレカ");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.暴風, 30), new WeatherPercentageOfArea(WeatherType.暴雨, 30), new WeatherPercentageOfArea(WeatherType.雪, 10), };
                    var area = new Area(areaGroup, "エウレカ：アネモス帯", weatherOfArea);
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.霧, 18), new WeatherPercentageOfArea(WeatherType.灼熱波, 18), new WeatherPercentageOfArea(WeatherType.雪, 18), new WeatherPercentageOfArea(WeatherType.雷, 18), new WeatherPercentageOfArea(WeatherType.吹雪, 18), };
                    var area = new Area(areaGroup, "エウレカ：パゴス帯", weatherOfArea);
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.灼熱波, 18), new WeatherPercentageOfArea(WeatherType.雷, 18), new WeatherPercentageOfArea(WeatherType.吹雪, 18), new WeatherPercentageOfArea(WeatherType.霊風, 18), new WeatherPercentageOfArea(WeatherType.雪, 18), };
                    var area = new Area(areaGroup, "エウレカ：ピューロス帯", weatherOfArea);
                    areaGroup.Areas.Add(area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 12), new WeatherPercentageOfArea(WeatherType.暴雨, 22), new WeatherPercentageOfArea(WeatherType.妖霧, 22), new WeatherPercentageOfArea(WeatherType.雷雨, 22), new WeatherPercentageOfArea(WeatherType.雪, 22), };
                    var area = new Area(areaGroup, "エウレカ：ヒュダトス帯", weatherOfArea);
                    areaGroup.Areas.Add(area);
                }
                _areaGroups.Add(areaGroup);
            }
            _fishingSpots = new FishingSpotCollection();
            foreach (var areaGroup in _areaGroups)
            {
                foreach (var area in areaGroup.Areas)
                {
                    foreach (var fishingSpot in area.FishingSpots)
                        _fishingSpots.Add(fishingSpot);
                }
            }
            _fishingBaites = new FishingBaitCollection();
            foreach (var fishingBate in new[]
            {
                new FishingBait("モスプパ"),
                new FishingBait("ラグワーム"),
                new FishingBait("ザリガニボール"),
                new FishingBait("ピルバグ"),
                new FishingBait("ゴビーボール"),
                new FishingBait("ブラッドワーム"),
                new FishingBait("ユスリカ"),
                new FishingBait("ラットの尾"),
                new FishingBait("クラブボール"),
                new FishingBait("クロウフライ"),
                new FishingBait("バターワーム"),
                new FishingBait("フローティングミノー"),
                new FishingBait("ブラススプーン"),
                new FishingBait("シュリンプフィーダー"),
                new FishingBait("バスボール"),
                new FishingBait("チョコボフライ"),
                new FishingBait("スプーンワーム"),
                new FishingBait("ハナアブ"),
                new FishingBait("シルバースプーン"),
                new FishingBait("メタルジグ"),
                new FishingBait("シンキングミノー"),
                new FishingBait("サンドリーチ"),
                new FishingBait("ハニーワーム"),
                new FishingBait("ヘリングボール"),
                new FishingBait("フェザントフライ"),
                new FishingBait("ヘヴィメタルジグ"),
                new FishingBait("スピナー"),
                new FishingBait("クリルフィーダー"),
                new FishingBait("サンドゲッコー"),
                new FishingBait("テッポウムシ"),
                new FishingBait("ミスリルスプーン"),
                new FishingBait("スナーブルフライ"),
                new FishingBait("トップウォーターフロッグ"),
                new FishingBait("グロウワーム"),
                new FishingBait("ホバーワーム"),
                new FishingBait("ローリングストーン"),
                new FishingBait("レインボースプーン"),
                new FishingBait("スピナーベイト"),
                new FishingBait("ストリーマー"),
                new FishingBait("弓角"),
                new FishingBait("カディスラーヴァ"),
                new FishingBait("ポーラークリル"),
                new FishingBait("バルーンバグ"),
                new FishingBait("ストーンラーヴァ"),
                new FishingBait("ツチグモ"),
                new FishingBait("ゴブリンジグ"),
                new FishingBait("ブレーデッドジグ"),
                new FishingBait("レッドバルーン"),
                new FishingBait("マグマワーム"),
                new FishingBait("バイオレットワーム"),
                new FishingBait("ブルートリーチ"),
                new FishingBait("ジャンボガガンボ"),
                new FishingBait("イクラ"),
                new FishingBait("ドバミミズ"),
                new FishingBait("赤虫"),
                new FishingBait("蚕蛹"),
                new FishingBait("活海老"),
                new FishingBait("タイカブラ"),
                new FishingBait("サスペンドミノー"),
                new FishingBait("ザザムシ"),
                new FishingBait("アオイソメ"),
                new FishingBait("フルーツワーム"),
                new FishingBait("モエビ"),
                new FishingBait("デザートフロッグ"),
                new FishingBait("マーブルラーヴァ"),
                new FishingBait("オヴィムジャーキー"),
                new FishingBait("ロバーボール"),
                new FishingBait("ショートビルミノー"),
                new FishingBait("蟲箱"),
                new FishingBait("イカの切り身"),
                new FishingBait("淡水万能餌"),
                new FishingBait("海水万能餌"),
                new FishingBait("メタルスピナー"),
                new FishingBait("イワイソメ"),
                new FishingBait("クリル"),
                new FishingBait("ファットワーム"),
                new FishingBait("万能ルアー"),
                new FishingBait("ディアデム・バルーン"),
                new FishingBait("ディアデム・レッドバルーン"),
                new FishingBait("ディアデム・ガガンボ"),
                new FishingBait("ディアデム・ホバーワーム"),
                new FishingBait("スカイボール"),
                new FishingBait("スモールギグヘッド"),
                new FishingBait("ミドルギグヘッド"),
                new FishingBait("ラージギグヘッド"),
            })
            {
                _fishingBaites.Add(fishingBate);
            }
            _fishes = new FishCollection();

            Func<string, FishingSpot> ToFishingSpot = id => _fishingSpots[new GameDataObjectId(GameDataObjectCategory.FishingSpot, id)];
            Func<string, FishingBait> ToFishingBait = id => _fishingBaites[new GameDataObjectId(GameDataObjectCategory.FishingBait, id)];
            foreach (var fish in new[]
            {
                // リムサ・ロミンサ
                new Fish("ゴールデンフィン", ToFishingSpot("リムサ・ロミンサ：上甲板層"), ToFishingBait("ピルバグ"), 9, 14, "ピルバグ⇒(!!!プレ)"),
                new Fish("メガオクトパス", ToFishingSpot("リムサ・ロミンサ：下甲板層"), ToFishingBait("ピルバグ"), 9, 17, "ピルバグ⇒(!!スト)ハーバーヘリングHQ⇒(!!!スト); ピルバグ⇒(!プレ)メルトールゴビーHQ⇒(!!!スト)"),

                // 中央ラノシア
                new Fish("ザルエラ", ToFishingSpot("ゼファードリフト沿岸"), ToFishingBait("ラットの尾"), 9, 14, "ラットの尾⇒(!!!プレ)"),
                new Fish("トリックスター", ToFishingSpot("ローグ川"), ToFishingBait("モスプパ"), 9, 14, "モスプパ⇒(!!!プレ)"),
                new Fish("スナガクレ", ToFishingSpot("西アジェレス川"), ToFishingBait("ザリガニボール"), "ザリガニボール⇒(!!!プレ)"),
                new Fish("ギガシャーク", ToFishingSpot("サマーフォード沿岸"), ToFishingBait("フローティングミノー"), WeatherType.快晴 | WeatherType.晴れ, "フローティングミノー⇒(!!スト)ハーバーヘリングHQ⇒(!!スト)オーガバラクーダHQ⇒(!!!スト); フローティングミノー⇒(!!スト)ハーバーヘリングHQ⇒(!!!スト); フローティングミノー⇒(!プレ)メルトールゴビーHQ⇒(!!!スト)"),
                new Fish("ハイパーチ", ToFishingSpot("ニーム川"), ToFishingBait("シンキングミノー"), 5, 8, "シンキングミノー⇒(!!!スト)"),
                new Fish("クリスタルパーチ", ToFishingSpot("ささやきの谷"), ToFishingBait("バターワーム"), WeatherType.曇り | WeatherType.霧 | WeatherType.風, "バターワーム⇒(!!!プレ)"),

                // 低地ラノシア
                new Fish("オオゴエナマズ", ToFishingSpot("モーニングウィドー"), ToFishingBait("モスプパ"), "モスプパ⇒(!!!プレ)"),
                new Fish("オシュオンプリント", ToFishingSpot("モラビー湾西岸"), ToFishingBait("ゴビーボール"), "ゴビーボール⇒(!!!スト)"),
                new Fish("シルドラ", ToFishingSpot("シダーウッド沿岸部"), ToFishingBait("スプーンワーム"), WeatherType.雨, "スプーンワーム⇒(!!!プレ)"),
                new Fish("マヒマヒ", ToFishingSpot("オシュオン灯台"), ToFishingBait("弓角"), 10, 18, "弓角⇒(!!!スト)"),
                new Fish("シルバーソブリン", ToFishingSpot("オシュオン灯台"), ToFishingBait("弓角"), "弓角⇒(!!!スト)"),
                new Fish("サーベルタイガーコッド", ToFishingSpot("キャンドルキープ埠頭"), ToFishingBait("ゴビーボール"), 16, 22, "ゴビーボール⇒(!!!スト)"),
                new Fish("ザ・リッパー", ToFishingSpot("モラビー造船廠"), ToFishingBait("ゴビーボール"), 21, 3, "ゴビーボール⇒(!!!プレ)"),
                new Fish("フェアリークィーン", ToFishingSpot("エンプティハート"), ToFishingBait("スピナー"), WeatherType.曇り | WeatherType.霧 | WeatherType.風, "スピナー⇒(!!!スト)"),
                new Fish("メテオサバイバー", ToFishingSpot("ソルトストランド"), ToFishingBait("ラットの尾"), 3, 5, WeatherType.曇り | WeatherType.風 | WeatherType.霧, "ラットの尾⇒(!!!スト)"),
                new Fish("オヤジウオ", ToFishingSpot("ブラインドアイアン坑道"), ToFishingBait("ハナアブ"), 17, 19, "ハナアブ⇒(!!!プレ)"),

                // 東ラノシア
                new Fish(
                    "フルムーンサーディン",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("南ブラッドショア"), new[] { ToFishingBait("スプーンワーム") }, 18, 6, "スプーンワーム⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("コスタ・デル・ソル"), new[] { ToFishingBait("スプーンワーム")}, 18, 6, "スプーンワーム⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("幻影諸島北岸"), new[] { ToFishingBait("スプーンワーム")}, 18, 6, "スプーンワーム⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("幻影諸島南岸"), new[] { ToFishingBait("スプーンワーム")}, 18, 6, "スプーンワーム⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("サプサ産卵地"), new[] { ToFishingBait("メタルジグ")}, 18, 6, "メタルジグ⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("ミスト・ヴィレッジ"), new[] { ToFishingBait("スプーンワーム")}, 18, 6, "スプーンワーム⇒(!プレ)"),
                    }),
                new Fish(
                    "リトルサラオス",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("コスタ・デル・ソル"), new[] { ToFishingBait("ポーラークリル") }, WeatherType.雨 | WeatherType.暴雨, "ポーラークリル⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ロータノ海沖合：船尾"), new[] { ToFishingBait("ポーラークリル") }, WeatherType.雨 | WeatherType.暴雨, "ポーラークリル⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("常夏の島北"), new[] { ToFishingBait("ポーラークリル") }, WeatherType.雨 | WeatherType.暴雨, "ポーラークリル⇒(!!スト)"),
                    }),
                new Fish(
                    "ロックロブスター",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("ロータノ海沖合：船尾"), new[] { ToFishingBait("ポーラークリル") }, 17, 22, "ポーラークリル⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("船隠しの港"), new[] { ToFishingBait("ポーラークリル") }, 17, 22, "ポーラークリル⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("シルバーバザー"), new[] { ToFishingBait("ポーラークリル") }, 17, 22, "ポーラークリル⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("クレセントコーヴ"), new[] { ToFishingBait("ポーラークリル") }, 17, 22, "ポーラークリル⇒(!!スト)"),
                    }),
                new Fish(
                    "ダークスリーパー",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("隠れ滝"), new[] { ToFishingBait("ブラススプーン") }, 15, 10, "ブラススプーン⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("花蜜桟橋"), new[] { ToFishingBait("シルバースプーン") }, 15, 10, "シルバースプーン⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("ハズーバ支流：上流"), new[] { ToFishingBait("スピナー") }, 15, 10, "スピナー⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("ハズーバ支流：中流"), new[] { ToFishingBait("バターワーム") }, 15, 10, "バターワーム⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("ハズーバ支流：東"), new[] { ToFishingBait("スピナーベイト") }, 15, 10, "スピナーベイト⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("フォールゴウド秋瓜湖畔"), new[] { ToFishingBait("スピナーベイト") }, 15, 10, "スピナーベイト⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("ラベンダーベッド"), new[] { ToFishingBait("ブラススプーン") }, 15, 10, "ブラススプーン⇒(!プレ)"),
                    }),
                new Fish(
                    "射手魚",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("東アジェレス川"), new[] { ToFishingBait("スナーブルフライ") }, WeatherType.晴れ | WeatherType.快晴, "スナーブルフライ⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("レインキャッチャー樹林"), new[] { ToFishingBait("スナーブルフライ") }, WeatherType.晴れ | WeatherType.快晴, "スナーブルフライ⇒(!プレ)"),
                    }),
                new Fish(
                    "雷紋魚",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("レインキャッチャー樹林"), new[] { ToFishingBait("スピナー") }, WeatherType.雨, "スピナー⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ロングクライム渓谷"), new[] { ToFishingBait("テッポウムシ") }, WeatherType.雨, "テッポウムシ⇒(!!スト)"),
                    }),
                new Fish("ビアナックブーン", ToFishingSpot("南ブラッドショア"), ToFishingBait("シュリンプフィーダー"), 20, 23, "シュリンプフィーダー⇒(!!!スト)"),
                new Fish("シャークトゥーナ", ToFishingSpot("コスタ・デル・ソル"), ToFishingBait("スプーンワーム"), 19, 21, WeatherType.快晴 | WeatherType.晴れ, "スプーンワーム⇒(!プレ)フルムーンサーディンHQ⇒(!!!スト)"),
                new Fish("ボンバードフィッシュ", ToFishingSpot("北ブラッドショア"), ToFishingBait("ヘリングボール"), 9, 15, WeatherType.快晴, "ヘリングボール⇒(!!!スト)"),
                new Fish("オールドマーリン", ToFishingSpot("ロータノ海沖合：船首"), ToFishingBait("ピルバグ"), WeatherType.雨 | WeatherType.暴雨, WeatherType.快晴, "ピルバグ⇒(!!スト)ハーバーヘリングHQ⇒(!!スト)オーガバラクーダHQ⇒(!!!スト)"),
                new Fish("ネプトの竜", ToFishingSpot("ロータノ海沖合：船尾"), ToFishingBait("ポーラークリル"), WeatherType.雨 | WeatherType.暴雨, "(要リトルサラオス×3) ポーラークリル⇒(!!スト)\nポーラークリル⇒(!!!スト); ポーラークリル⇒(!プレ)メルトールゴビーHQ⇒(!!!スト)"),
                new Fish("ソルター", ToFishingSpot("隠れ滝"), ToFishingBait("ハニーワーム"), 17, 20, WeatherType.快晴 | WeatherType.晴れ, "@!ハニーワーム⇒(!プレ)"),
                new Fish("ドラウンドスナイパー", ToFishingSpot("東アジェレス川"), ToFishingBait("スナーブルフライ"), WeatherType.快晴 | WeatherType.晴れ, "スナーブルフライ⇒(!!!スト)"),
                new Fish("テルプシコレアン", ToFishingSpot("レインキャッチャー樹林"), ToFishingBait("ハニーワーム"), WeatherType.霧, "ハニーワーム⇒(!!!プレ)"),
                new Fish("ミラースケイル", ToFishingSpot("レインキャッチャー沼沢地"), ToFishingBait("ユスリカ"), 9, 16, WeatherType.快晴 | WeatherType.晴れ, "ユスリカ⇒(!プレ)銅魚HQ⇒(!!!プレ)"),
                new Fish("黄金魚", ToFishingSpot("レッドマンティス滝"), ToFishingBait("ハニーワーム"), WeatherType.快晴 | WeatherType.晴れ, "(要オオモリナマズ×3, 天候不問) ハニーワーム⇒(!プレ)銀魚HQ⇒(!プレ)金魚HQ⇒(!!スト)\nハニーワーム⇒(!プレ)銀魚HQ⇒(!!!スト)"),
                new Fish("海チョコチョコボ", ToFishingSpot("常夏の島北"), ToFishingBait("ラグワーム"), 8, 16, "ラグワーム⇒(!プレ)メルトールゴビーHQ⇒(!!スト)ワフーHQ⇒(!!!スト)"),
                new Fish("リトルペリュコス", ToFishingSpot("常夏の島北"), ToFishingBait("ラグワーム"), WeatherType.雨 | WeatherType.暴雨, "ラグワーム⇒(!プレ)メルトールゴビーHQ⇒(!!スト)ワフーHQ⇒(!!スト)"),

                // 西ラノシア
                new Fish("スケーリーフット", ToFishingSpot("スウィフトパーチ入植地"), ToFishingBait("ポーラークリル"), 19, 3, "ポーラークリル⇒(!!!プレ)"),
                new Fish("ジャンクモンガー", ToFishingSpot("スカルバレー沿岸部"), ToFishingBait("ラグワーム"), 16, 2, "ラグワーム⇒(!プレ)メルトールゴビーHQ⇒(!!スト)ワフーHQ⇒(!!!スト); ラグワーム⇒(!プレ)メルトールゴビーHQ⇒(!!!スト)"),
                new Fish("リムレーンズソード", ToFishingSpot("ブルワーズ灯台"), ToFishingBait("弓角"), 9, 14, WeatherType.快晴 | WeatherType.晴れ, "弓角⇒(!!!スト)"),
                new Fish("ローンリッパー", ToFishingSpot("ハーフストーン沿岸部"), ToFishingBait("ヘヴィメタルジグ"), WeatherType.暴風, "ヘヴィメタルジグ⇒(!!!スト)"),
                new Fish("ヘルムズマンズハンド", ToFishingSpot("幻影諸島北岸"), ToFishingBait("ピルバグ"), 9, 15, WeatherType.曇り | WeatherType.風 | WeatherType.霧, "ピルバグ⇒(!プレ)オーシャンクラウドHQ⇒(!!!スト)"),
                // モラモラは釣り手帳で「天候条件：あり」となっているが、各攻略情報を見る限り釣れない天候はない模様なので、ここには登録しない。
                new Fish("ラブカ", ToFishingSpot("船の墓場"), ToFishingBait("ラグワーム"), 17, 3, WeatherType.曇り | WeatherType.風 | WeatherType.霧, "ラグワーム⇒(!プレ)メルトールゴビーHQ⇒(!!スト)ワフーHQ⇒(!!!スト)ジャイアントスキッドHQ⇒(!!!スト)"),
                new Fish("キャプテンズチャリス", ToFishingSpot("サプサ産卵地"), ToFishingBait("メタルジグ"), 23, 2, "メタルジグ⇒(!プレ)フルムーンサーディンHQ⇒(!!!スト)"),
                new Fish("コエラカントゥス", ToFishingSpot("幻影諸島南岸"), ToFishingBait("スプーンワーム"), 22, 3, WeatherType.曇り | WeatherType.風 | WeatherType.霧, "スプーンワーム⇒(!プレ)フルムーンサーディンHQ⇒(!!!スト)"),
                new Fish("エンドセラス", ToFishingSpot("幻影諸島南岸"), ToFishingBait("スプーンワーム"), 20, 4, WeatherType.快晴 | WeatherType.晴れ, WeatherType.曇り | WeatherType.風 | WeatherType.霧, "スプーンワーム⇒(!プレ)フルムーンサーディンHQ⇒(!!!スト)"),
                new Fish("シーハッグ", ToFishingSpot("船隠しの港"), ToFishingBait("フローティングミノー"), 19, 2, WeatherType.快晴 | WeatherType.晴れ, WeatherType.快晴 | WeatherType.晴れ, "フローティングミノー⇒(!プレ)メルトールゴビーHQ⇒(!!スト)ワフーHQ⇒(!!!スト)"),

                // 高地ラノシア
                new Fish(
                    "ジェイドイール",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("オークウッド"), new[] { ToFishingBait("バターワーム") }, 17, 10, "バターワーム⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("愚か者の滝"), new[] { ToFishingBait("シルバースプーン") }, 17, 10, "シルバースプーン⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ブロンズレイク北東岸"), new[] { ToFishingBait("フローティングミノー") }, 17, 10, "フローティングミノー⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ブロンズレイク・シャロー"), new[] { ToFishingBait("バターワーム") }, 17, 10, "バターワーム⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ブロンズレイク北西岸"), new[] { ToFishingBait("バターワーム") }, 17, 10, "バターワーム⇒(!!スト)"),
                    }),
                new Fish("トラマフィッシュ", ToFishingSpot("オークウッド"), ToFishingBait("スピナー"), 17, 20, WeatherType.曇り | WeatherType.霧, "スピナー⇒(!プレ)スカルピンHQ⇒(!!!スト)"),
                new Fish("ジャンヌ・トラウト", ToFishingSpot("愚か者の滝"), ToFishingBait("クロウフライ"), 4, 6, "クロウフライ⇒(!!!スト)"),
                new Fish("ワーム・オブ・ニーム", ToFishingSpot("ブロンズレイク・シャロー"), ToFishingBait("バターワーム"), 19, 22, "バターワーム⇒(!!!スト)"),
                new Fish(
                    "スプリングキング",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("ブロンズレイク北東岸"), new[] { ToFishingBait("スピナーベイト") }, 16, 19, "スピナーベイト⇒(!!!スト)"),
                        new FishingCondition(ToFishingSpot("ブロンズレイク北西岸"), new[] { ToFishingBait("スピナーベイト") }, 16, 19, "スピナーベイト⇒(!!!スト)"),
                    }),

                // 外地ラノシア
                new Fish("大鈍甲", ToFishingSpot("ロングクライム渓谷"), ToFishingBait("スピナーベイト"), 4, 9, "スピナーベイト⇒(!!スト)"),
                new Fish("サンダーガット", ToFishingSpot("ロングクライム渓谷"), ToFishingBait("テッポウムシ"), 19, 3, WeatherType.雨, "テッポウムシ⇒(!!!スト)"),

                // ミスト・ヴィレッジ
                new Fish("トゥイッチビアード", ToFishingSpot("ミスト・ヴィレッジ"), ToFishingBait("スプーンワーム"), 4, 6, WeatherType.快晴 | WeatherType.晴れ, "スプーンワーム⇒(!プレ)フルムーンサーディンHQ⇒(!!!スト)"),

                // グリダニア
                new Fish(
                    "雨乞魚",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("グリダニア：翡翠湖畔"), new FishingBait[]{ ToFishingBait("テッポウムシ") }, 17, 2, WeatherType.雨, "テッポウムシ⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("グリダニア：囁きの渓谷"), new FishingBait[]{ ToFishingBait("テッポウムシ") }, 17, 2, WeatherType.雨, "テッポウムシ⇒(!!スト)"),
                    }),
                new Fish("招嵐王", ToFishingSpot("グリダニア：翡翠湖畔"), ToFishingBait("テッポウムシ"), 17, 2, WeatherType.雨, "テッポウムシ⇒(!!!スト)"),
                new Fish("ブラッディブルワー", ToFishingSpot("グリダニア：紅茶川水系下流"), ToFishingBait("ザリガニボール"), "ザリガニボール⇒(!!!スト)"),
                new Fish("マトロンカープ", ToFishingSpot("グリダニア：囁きの渓谷"), ToFishingBait("ブラッドワーム"), 15, 21, "ブラッドワーム⇒(!!!プレ)"),
                new Fish("意地ブナ", ToFishingSpot("グリダニア：紅茶川水系上流"), ToFishingBait("クロウフライ"), 9, 14, WeatherType.曇り | WeatherType.霧, "クロウフライ⇒(!!!プレ)"),

                // 中央森林
                new Fish("カイラージョン", ToFishingSpot("葉脈水系"), ToFishingBait("モスプパ"), "モスプパ⇒(!プレ)ゼブラゴビーHQ⇒(!!!プレ)"),
                new Fish("人面魚", ToFishingSpot("鏡池"), ToFishingBait("バターワーム"), 21, 3, WeatherType.雨, "バターワーム⇒(!!!スト)"),
                new Fish(
                    "ブラックイール",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("エバーシェイド"), new[] { ToFishingBait("バスボール") }, 17, 10, "バスボール⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("芽吹の池"), new[] { ToFishingBait("バスボール") }, 17, 10, "バスボール⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("青翠の奈落"), new[] { ToFishingBait("バスボール") }, 17, 10, "バスボール⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ハズーバ支流：上流"), new[] { ToFishingBait("バスボール") }, 17, 10, "バスボール⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ハズーバ支流：下流"), new[] { ToFishingBait("バスボール") }, 17, 10, "バスボール⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ハズーバ支流：東"), new[] { ToFishingBait("バスボール") }, 17, 10, "バスボール⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ユグラム川"), new[] { ToFishingBait("バターワーム") }, 17, 10, "バターワーム⇒(!!スト)"),
                    }),
                new Fish("レヴィンライト", ToFishingSpot("エバーシェイド"), ToFishingBait("ハナアブ"), 18, 23, "ハナアブ⇒(!!!プレ)"),
                new Fish("グリーンジェスター", ToFishingSpot("芽吹の池"), ToFishingBait("ハニーワーム"), 18, 21, "ハニーワーム⇒(!!!スト)"),
                new Fish("ブラッドバス", ToFishingSpot("ハウケタ御用邸"), ToFishingBait("ハニーワーム"), WeatherType.雷, "ハニーワーム⇒(!プレ)銀魚HQ⇒(!!!スト)"),

                // 東部森林
                new Fish("ダークアンブッシャー", ToFishingSpot("花蜜桟橋"), ToFishingBait("モスプパ"), 21, 3, "モスプパ⇒(!プレ)ゼブラゴビーHQ⇒(!!!プレ)"),
                new Fish("モルバ", ToFishingSpot("さざなみ小川"), ToFishingBait("ユスリカ"), 18, 2, "ユスリカ⇒(!プレ)グラディエーターベタHQ⇒(!!!スト)"),
                new Fish("ターミネーター", ToFishingSpot("十二神大聖堂"), ToFishingBait("バターワーム"), 21, 23, "バターワーム⇒(!!!スト)"),
                new Fish("シルフズベーン", ToFishingSpot("青翠の奈落"), ToFishingBait("バターワーム"), WeatherType.雨, "バターワーム⇒(!プレ)銅魚HQ⇒(!!!スト)"),
                new Fish(
                    "オークルート",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("シルフランド渓谷"), new[] { ToFishingBait("テッポウムシ") }, 17, 10, "テッポウムシ⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("プラウドクリーク"), new[] { ToFishingBait("テッポウムシ") }, 17, 10, "テッポウムシ⇒(!!スト)"),
                    }),
                new Fish("マッシュルームクラブ", ToFishingSpot("シルフランド渓谷"), ToFishingBait("スピナー"), WeatherType.曇り | WeatherType.霧, "スピナー⇒(!プレ)スカルピンHQ⇒(!!スト)"),
                new Fish("マジック・マッシュルームクラブ", ToFishingSpot("シルフランド渓谷"), ToFishingBait("スピナー"), WeatherType.雨 | WeatherType.雷, WeatherType.曇り | WeatherType.霧, "スピナー⇒(!プレ)スカルピンHQ⇒(!!!スト)"),

                // 南部森林
                new Fish("ビッグバイパー", ToFishingSpot("ハズーバ支流：上流"), ToFishingBait("バターワーム"), 18, 19, "バターワーム⇒(!!!スト)"),
                new Fish("フローティングボルダー", ToFishingSpot("ハズーバ支流：下流"), ToFishingBait("バスボール"), 0, 8, WeatherType.曇り | WeatherType.霧, "バスボール⇒(!!!スト)"),
                new Fish("グリナー", ToFishingSpot("ハズーバ支流：東"), ToFishingBait("ハナアブ"), "ハナアブ⇒(!!!スト)"),
                new Fish("シンカー", ToFishingSpot("ハズーバ支流：中流"), ToFishingBait("シンキングミノー"), 21, 3, WeatherType.快晴 | WeatherType.晴れ, "シンキングミノー⇒(!!!スト)"),
                new Fish("ゴブスレイヤー", ToFishingSpot("ゴブリン族の生簀"), ToFishingBait("ハニーワーム"), WeatherType.雷 | WeatherType.雷雨, "ハニーワーム⇒(!プレ)銀魚HQ⇒(!!!スト)"),
                new Fish("雷神魚", ToFishingSpot("根渡り沼"), ToFishingBait("カディスラーヴァ"), WeatherType.雷雨 | WeatherType.雷, "カディスラーヴァ⇒(!!スト)"),
                new Fish("雷皇子", ToFishingSpot("根渡り沼"), ToFishingBait("カディスラーヴァ"), WeatherType.雷雨, "カディスラーヴァ⇒(!!!スト)"),
                new Fish("オオタキタロ", ToFishingSpot("ウルズの恵み"), ToFishingBait("グロウワーム"), WeatherType.雷 | WeatherType.雷雨, "グロウワーム⇒(!!!スト)"),
                new Fish("ナミタロ", ToFishingSpot("ウルズの恵み"), new[] { ToFishingBait("グロウワーム"), ToFishingBait("トップウォーターフロッグ") }, WeatherType.雷 | WeatherType.雷雨, "(要オオタキタロ×1, 雷雨/雷) グロウワーム⇒(!!!スト)\n(天候不問) トップウォーターフロッグ⇒(!!!スト)"),

                // 北部森林
                new Fish("ブルーウィドー", ToFishingSpot("さざめき川"), ToFishingBait("フローティングミノー"), 9, 14, "フローティングミノー⇒(!!!スト)"),
                new Fish("ジャッジレイ", ToFishingSpot("フォールゴウド秋瓜湖畔"), ToFishingBait("フェザントフライ"), 17, 21, "フェザントフライ⇒(!!!スト)"),
                new Fish("シャドーストリーク", ToFishingSpot("プラウドクリーク"), ToFishingBait("トップウォーターフロッグ"), 4, 10, WeatherType.霧, "トップウォーターフロッグ⇒(!!!スト)"),
                new Fish("コーネリア", ToFishingSpot("タホトトル湖畔"), new[] { ToFishingBait("レインボースプーン"), ToFishingBait("グロウワーム") }, "(要ボクシングプレコ×5) グロウワーム⇒(!!スト)\nレインボースプーン⇒(!!!プレ)"),

                // ラベンダーベッド
                new Fish("スウィートニュート", ToFishingSpot("ラベンダーベッド"), ToFishingBait("ユスリカ"), 23, 4, WeatherType.霧, "ユスリカ⇒(!プレ)グラディエーターベタHQ⇒(!!!スト)"),

                // 西ザナラーン
                new Fish("銅鏡", ToFishingSpot("ノフィカの井戸"), ToFishingBait("バターワーム"), WeatherType.快晴 | WeatherType.晴れ, "バターワーム⇒(!!!プレ)"),
                new Fish("マッドゴーレム", ToFishingSpot("足跡の谷"), ToFishingBait("バターワーム"), 21, 3, "バターワーム⇒(!!!スト)"),
                new Fish("リベットオイスター", ToFishingSpot("ベスパーベイ"), ToFishingBait("ヘヴィメタルジグ"), "ヘヴィメタルジグ⇒(!!!スト)"),
                new Fish("フィンガーズ", ToFishingSpot("クレセントコーヴ"), ToFishingBait("ポーラークリル"), 17, 18, "ポーラークリル⇒(!!!スト)"),
                new Fish("ダーティーヘリング", ToFishingSpot("シルバーバザー"), ToFishingBait("ポーラークリル"), 20, 22, "ポーラークリル⇒(!!!プレ)"),
                new Fish("タイタニックソー", ToFishingSpot("ウエストウインド岬"), ToFishingBait("ラグワーム"), 9, 15, WeatherType.快晴 | WeatherType.晴れ, "ラグワーム⇒(!プレ)メルトールゴビーHQ⇒(!!スト)ワフーHQ⇒(!!!スト)"),
                new Fish("パイレーツハンター", ToFishingSpot("ウエストウインド岬"), ToFishingBait("ラグワーム"), "(要ワフー×6) ラグワーム⇒(!プレ)メルトールゴビーHQ⇒(!!スト)\nラグワーム⇒(!プレ)メルトールゴビーHQ⇒(!!スト)ワフーHQ⇒(!!!スト); ラグワーム⇒(!プレ)メルトールゴビーHQ⇒(!!!スト)"),
                new Fish("ンデンデキ", ToFishingSpot("ムーンドリップ洞窟"), ToFishingBait("ハニーワーム"), 18, 5, WeatherType.霧, "ハニーワーム⇒(!プレ)銀魚HQ⇒(!!スト)アサシンベタHQ⇒(!!!スト); ハニーワーム⇒(!プレ)銀魚HQ⇒(!!!スト)"),
                new Fish("ヴァンパイアウィップ", ToFishingSpot("パラタの墓所"), ToFishingBait("ハニーワーム"), WeatherType.雨, WeatherType.快晴 | WeatherType.曇り | WeatherType.晴れ | WeatherType.霧, "ハニーワーム⇒(!プレ)銀魚HQ⇒(!!!スト)"),

                // 中央ザナラーン
                new Fish("ドリームゴビー", ToFishingSpot("スートクリーク上流"), ToFishingBait("ザリガニボール"), 17, 3, "ザリガニボール⇒(!!!プレ)"),
                new Fish("ヌルヌルキング", ToFishingSpot("スートクリーク下流"), ToFishingBait("ブラッドワーム"), 19, 0, "ブラッドワーム⇒(!!!プレ)"),
                new Fish("オールドソフティ", ToFishingSpot("クラッチ狭間"), ToFishingBait("ブラッドワーム"), 17, 21, "ブラッドワーム⇒(!!!プレ)"),
                new Fish("ダークナイト", ToFishingSpot("アンホーリーエアー"), ToFishingBait("クロウフライ"), WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, "クロウフライ⇒(!!!スト)"),

                // 東ザナラーン
                new Fish("ジンコツシャブリ", ToFishingSpot("ドライボーン北湧水地"), ToFishingBait("ハナアブ"), 20, 3, WeatherType.雨 | WeatherType.暴雨, "ハナアブ⇒(!!!スト)"),
                new Fish("マッドピルグリム", ToFishingSpot("ドライボーン南湧水地"), ToFishingBait("ユスリカ"), 17, 7, WeatherType.雨 | WeatherType.暴雨, "ユスリカ⇒(!!!スト)"),
                new Fish("ナルザルイール", ToFishingSpot("ユグラム川"), ToFishingBait("ハニーワーム"), 17, 10, "ハニーワーム⇒(!!スト)"),
                new Fish("ワーデンズワンド", ToFishingSpot("ユグラム川"), ToFishingBait("ハニーワーム"), 17, 20, WeatherType.快晴, "ハニーワーム⇒(!!!スト)"),
                new Fish("サンディスク", ToFishingSpot("バーニングウォール"), ToFishingBait("カディスラーヴァ"), 10, 15, WeatherType.快晴 | WeatherType.晴れ, "カディスラーヴァ⇒(!!スト)"),
                new Fish("サウザンドイヤー・イーチ", ToFishingSpot("バーニングウォール"), ToFishingBait("テッポウムシ"), WeatherType.霧, "テッポウムシ⇒(!!!スト)"),

                // 南ザナラーン
                new Fish("ホローアイズ", ToFishingSpot("リザードクリーク"), ToFishingBait("ユスリカ"), WeatherType.霧, "ユスリカ⇒(!プレ)銅魚HQ⇒(!!!スト)"),
                new Fish("パガルザンディスカス", ToFishingSpot("ザハラクの湧水"), ToFishingBait("グロウワーム"), WeatherType.灼熱波 | WeatherType.快晴 | WeatherType.晴れ, "グロウワーム⇒(!!スト)"),
                new Fish("ディスコボルス", ToFishingSpot("ザハラクの湧水"), ToFishingBait("グロウワーム"), 12, 18, WeatherType.灼熱波, "グロウワーム⇒(!!!スト)"),
                new Fish("アイアンヌース", ToFishingSpot("忘れられたオアシス"), ToFishingBait("スピナー"), WeatherType.霧, "スピナー⇒(!!!スト)"),
                new Fish("ホーリーカーペット", ToFishingSpot("サゴリー砂海"), ToFishingBait("サンドリーチ"), 9, 16, WeatherType.灼熱波, "サンドリーチ⇒(!!スト)サンドストームライダーHQ⇒(!!!スト)"),
                new Fish("ヘリコプリオン", ToFishingSpot("サゴリー砂海"), ToFishingBait("サンドリーチ"), 8, 20, WeatherType.曇り | WeatherType.霧, WeatherType.灼熱波, "サンドリーチ⇒(!!スト)サンドストームライダーHQ⇒(!!!スト)"),
                new Fish("オルゴイコルコイ", ToFishingSpot("サゴリー砂丘"), ToFishingBait("サンドリーチ"), WeatherType.灼熱波, "サンドリーチ⇒(!!スト)サンドストームライダーHQ⇒(!!!スト)"),

                // 北ザナラーン
                new Fish("ハンニバル", ToFishingSpot("ブルーフォグ湧水地"), ToFishingBait("スピナーベイト"), 22, 4, WeatherType.霧, "スピナーベイト⇒(!プレ)スカルピンHQ⇒(!!!スト)"),
                new Fish("ウーツナイフ・ゼニス", ToFishingSpot("青燐泉"), ToFishingBait("ハニーワーム"), 1, 4, WeatherType.快晴 | WeatherType.晴れ, WeatherType.霧, "ハニーワーム⇒(!プレ)銀魚HQ⇒(!!!スト)"),

                // ゴブレットビュート
                new Fish("スピアノーズ", ToFishingSpot("ゴブレットビュート"), ToFishingBait("カディスラーヴァ"), 21, 0, WeatherType.曇り | WeatherType.霧, "カディスラーヴァ⇒(!!!プレ)"),

                // クルザス中央高地
                new Fish("ダナフェンズマーク", ToFishingSpot("クルザス川"), ToFishingBait("フェザントフライ"), WeatherType.吹雪, "フェザントフライ⇒(!!!スト)"),
                new Fish("カローンズランタン", ToFishingSpot("聖ダナフェンの旅程"), ToFishingBait("グロウワーム"), 0, 4, WeatherType.吹雪 | WeatherType.雪, "グロウワーム⇒(!!!プレ)"),
                new Fish("スターブライト", ToFishingSpot("剣ヶ峰山麓"), ToFishingBait("ストーンラーヴァ"), 21, 4, WeatherType.快晴 | WeatherType.晴れ, "ストーンラーヴァ⇒(!プレ)アバラシアスメルトHQ⇒(!!!スト)"),
                new Fish("ドーンメイデン", ToFishingSpot("キャンプ・ドラゴンヘッド溜池"), ToFishingBait("フェザントフライ"), 5, 7, WeatherType.快晴 | WeatherType.晴れ, "フェザントフライ⇒(!!!スト)"),
                new Fish("メイトリアーク", ToFishingSpot("調査隊の氷穴"), ToFishingBait("ブルートリーチ"), "ブルートリーチ⇒(!プレ)アバラシアスメルトHQ⇒(!!!プレ)"),
                new Fish("ダークスター", ToFishingSpot("聖ダナフェンの落涙"), new[] { ToFishingBait("ブルートリーチ"), ToFishingBait("チョコボフライ") }, 19, 4, WeatherType.吹雪 | WeatherType.雪, "(要ランプマリモ×5, 時間帯不問, 天候不問) チョコボフライ⇒(!プレ)\nブルートリーチ⇒(!プレ)アバラシアスメルトHQ⇒(!!!スト)"),
                new Fish("ブルーコープス", ToFishingSpot("スノークローク大氷壁"), ToFishingBait("カディスラーヴァ"), WeatherType.雪 | WeatherType.吹雪, WeatherType.快晴 | WeatherType.晴れ, "カディスラーヴァ⇒(!!!プレ)"),
                new Fish("アノマロカリス", ToFishingSpot("イシュガルド大雲海"), ToFishingBait("ホバーワーム"), 10, 15, WeatherType.快晴 | WeatherType.晴れ, "ホバーワーム⇒(!!スト)スプリットクラウドHQ⇒(!!!スト)"),
                new Fish("マハール", ToFishingSpot("ウィッチドロップ"), ToFishingBait("ホバーワーム"), WeatherType.快晴 | WeatherType.晴れ, WeatherType.吹雪, "ホバーワーム⇒(!!スト)スプリットクラウドHQ⇒(!!!スト)"),
                new Fish("ショニサウルス", ToFishingSpot("ウィッチドロップ"), ToFishingBait("ホバーワーム"), WeatherType.吹雪, "ホバーワーム⇒(!!スト)スプリットクラウドHQ⇒(!!!スト)マハールHQ⇒(!!!スト)"),

                // クルザス西部高地
                new Fish("クルザスパファー", ToFishingSpot("リバーズミート"), ToFishingBait("ブルートリーチ"), WeatherType.雪 | WeatherType.吹雪, "ブルートリーチ⇒(!プレ)"),
                new Fish("ファットパース", ToFishingSpot("リバーズミート"), ToFishingBait("ブルートリーチ"), "ブルートリーチ⇒(!!!プレ)"),
                new Fish("グレイシャーコア", ToFishingSpot("グレイテール滝"), ToFishingBait("ジャンボガガンボ"), WeatherType.雪 | WeatherType.吹雪, "ジャンボガガンボ⇒(!!スト); ジャンボガガンボ⇒(!プレ)スカイワームHQ⇒(!!スト)"),
                new Fish("ハイウィンドジェリー", ToFishingSpot("グレイテール滝"), ToFishingBait("ジャンボガガンボ"), WeatherType.晴れ | WeatherType.快晴, "(要フィッシュアイ) ジャンボガガンボ⇒(!プレ)スカイワームHQ⇒(!プレ)"),
                new Fish("アイスイーター", ToFishingSpot("グレイテール滝"), ToFishingBait("ジャンボガガンボ"), WeatherType.雪 | WeatherType.吹雪, "ジャンボガガンボ⇒(!!スト)グレイシャーコアHQ⇒(!プレ); ジャンボガガンボ⇒(!プレ)スカイワームHQ⇒(!!スト)グレイシャーコアHQ⇒(!プレ)"),
                new Fish("ヘイルイーター", ToFishingSpot("グレイテール滝"), ToFishingBait("ジャンボガガンボ"), WeatherType.吹雪, "ジャンボガガンボ⇒(!!スト)グレイシャーコアHQ⇒(!!!プレ); ジャンボガガンボ⇒(!プレ)スカイワームHQ⇒(!!スト)グレイシャーコアHQ⇒(!!!プレ)"),
                new Fish("ソーサラーフィッシュ", ToFishingSpot("クルザス不凍池"), ToFishingBait("ハナアブ"), 8, 20, "ハナアブ⇒(!プレ)アバラシアスメルトHQ⇒(!!スト)"),
                new Fish("ホワイトオクトパス", ToFishingSpot("クルザス不凍池"), ToFishingBait("ブルートリーチ"), 8, 18, "ブルートリーチ⇒(!!スト); ブルートリーチ⇒(!プレ)アバラシアスメルトHQ⇒(!!スト); ブルートリーチ⇒(!プレ)ハルオネHQ⇒(!!スト)"),
                new Fish("クルザスクリオネ", ToFishingSpot("クルザス不凍池"), ToFishingBait("ブルートリーチ"), 0, 4, WeatherType.吹雪, "ブルートリーチ⇒(!プレ); ブルートリーチ⇒(!プレ)アバラシアスメルトHQ⇒(!プレ); ブルートリーチ⇒(!プレ)ハルオネHQ⇒(!プレ)"),
                new Fish("フレアフィッシュ", ToFishingSpot("クルザス不凍池"), ToFishingBait("ツチグモ"), 10, 16, WeatherType.吹雪, "ツチグモ⇒(!プレ)ハルオネHQ⇒(!!!スト)"),
                new Fish("ヒートロッド", ToFishingSpot("クリアプール"), ToFishingBait("ブルートリーチ"), WeatherType.吹雪 | WeatherType.雪, "ブルートリーチ⇒(!プレ); ブルートリーチ⇒(!プレ)アバラシアスメルトHQ⇒(!プレ); ブルートリーチ⇒(!プレ)ハルオネHQ⇒(!プレ)"),
                new Fish("カペリン", ToFishingSpot("クリアプール"), ToFishingBait("ブルートリーチ"), 0, 6, "(要フィッシュアイ)ブルートリーチ⇒(!プレ)"),
                new Fish("プリーストフィッシュ", ToFishingSpot("クリアプール"), ToFishingBait("ブルートリーチ"), 0, 6, "(要フィッシュアイ)ブルートリーチ⇒(!!スト)"),
                new Fish("ビショップフィッシュ", ToFishingSpot("クリアプール"), ToFishingBait("ブルートリーチ"), 10, 14, WeatherType.吹雪 | WeatherType.雪, WeatherType.快晴, "(要フィッシュアイ)ブルートリーチ⇒(!!!スト)"),
                new Fish("シャリベネ", ToFishingSpot("クリアプール"), ToFishingBait("ツチグモ"), 0, 3, WeatherType.吹雪, "(要ハルオネ×5) ツチグモ⇒(!プレ)\nツチグモ⇒(!プレ)ハルオネHQ⇒(!!!プレ)"),
                new Fish("イエティキラー", ToFishingSpot("ドラゴンスピット"), ToFishingBait("ブルートリーチ"), "(要引っ掛け釣り)ブルートリーチ⇒(!!!プレ)"),
                new Fish("ベーンクラーケン", ToFishingSpot("ベーンプール南"), ToFishingBait("ブルートリーチ"), WeatherType.吹雪 | WeatherType.雪 | WeatherType.曇り | WeatherType.霧, "(要フィッシュアイ)ブルートリーチ⇒(!!!スト)"),
                new Fish("ネモ", ToFishingSpot("ベーンプール南"), ToFishingBait("ブルートリーチ"), WeatherType.雪, WeatherType.吹雪, "ブルートリーチ⇒(!!!スト)"),
                new Fish("ラ・レアル", ToFishingSpot("アッシュプール"), ToFishingBait("ブルートリーチ"), "ブルートリーチ⇒(!!!スト)"),
                new Fish("氷神魚", ToFishingSpot("ベーンプール西"), ToFishingBait("ブルートリーチ"), WeatherType.雪 | WeatherType.吹雪, "ブルートリーチ⇒(!!スト); ブルートリーチ⇒(!プレ)ハルオネHQ⇒(!!スト)"),
                new Fish("雪乞魚", ToFishingSpot("ベーンプール西"), ToFishingBait("カディスラーヴァ"), WeatherType.雪 | WeatherType.吹雪, "カディスラーヴァ⇒(!プレ)ハルオネHQ⇒(!プレ)"),
                new Fish("氷の巫女", ToFishingSpot("ベーンプール西"), ToFishingBait("カディスラーヴァ"), WeatherType.雪 | WeatherType.吹雪, WeatherType.吹雪, "(要フィッシュアイ)カディスラーヴァ⇒(!プレ)ハルオネHQ⇒(!!!スト)"),

                // モードゥナ
                new Fish("ヘリオバティス", ToFishingSpot("銀泪湖北岸"), ToFishingBait("カディスラーヴァ"), 17, 9, "カディスラーヴァ⇒(!!スト)"),
                new Fish("エーテルラウス", ToFishingSpot("銀泪湖北岸"), ToFishingBait("グロウワーム"), 3, 13, WeatherType.妖霧, "グロウワーム⇒(!!!スト)"),
                new Fish("インフェルノホーン", ToFishingSpot("タングル湿林源流"), ToFishingBait("グロウワーム"), WeatherType.妖霧, WeatherType.晴れ | WeatherType.快晴, "グロウワーム⇒(!!!プレ)"),
                new Fish("血紅龍", ToFishingSpot("唄う裂谷"), ToFishingBait("ハニーワーム"), 4, 12, WeatherType.快晴 | WeatherType.晴れ, WeatherType.霧, "ハニーワーム⇒(!プレ)銀魚HQ⇒(!!!スト)"),
                new Fish("ヴォイドバス", ToFishingSpot("早霜峠"), ToFishingBait("グロウワーム"), WeatherType.快晴 | WeatherType.晴れ, WeatherType.妖霧, "グロウワーム⇒(!!!スト)"),
                new Fish("腐魚", ToFishingSpot("タングル湿林"), ToFishingBait("レインボースプーン"), WeatherType.妖霧, "レインボースプーン⇒(!!スト)"),
                new Fish("ニンジャベタ", ToFishingSpot("タングル湿林"), ToFishingBait("ユスリカ"), 18, 9, WeatherType.妖霧, "ユスリカ⇒(!プレ)グラディエーターベタHQ⇒(!!スト)アサシンベタHQ⇒(!!!スト)"),
                new Fish("ジャノ", ToFishingSpot("唄う裂谷北部"), ToFishingBait("ハニーワーム"), 8, 18, WeatherType.妖霧, "ハニーワーム⇒(!プレ)銀魚HQ⇒(!プレ)金魚HQ⇒(!!!スト)"),
                new Fish("クノ・ザ・キラー", ToFishingSpot("唄う裂谷北部"), ToFishingBait("ハニーワーム"), WeatherType.妖霧, "(要ジャノ×1)ハニーワーム⇒(!プレ)銀魚HQ⇒(!プレ)金魚HQ⇒(!!!スト)\nハニーワーム⇒(!プレ)銀魚HQ⇒(!!スト)アサシンベタHQ⇒(!!!スト); ハニーワーム⇒(!プレ)銀魚HQ⇒(!プレ)金魚HQ⇒(!!スト)アサシンベタHQ⇒(!!!スト)"),

                // アバラシア雲海
                new Fish("出目金", ToFishingSpot("ヴール・シアンシラン"), ToFishingBait("ブルートリーチ"), 9, 15, "ブルートリーチ⇒(!プレ); ブルートリーチ⇒(!プレ)グリロタルパHQ⇒(!プレ)"),
                new Fish(
                    "カイマン",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("ヴール・シアンシラン"), new[] { ToFishingBait("ブレーデッドジグ") }, 18, 21, "ブレーデッドジグ⇒(!プレ)ブルフロッグHQ⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("餌食の台地"), new[] { ToFishingBait("ストーンラーヴァ") }, 18, 21, "ストーンラーヴァ⇒(!プレ)マクロブラキウムHQ⇒(!!スト)"),
                    }),
                new Fish("水墨魚", ToFishingSpot("ヴール・シアンシラン"), ToFishingBait("ブルートリーチ"), 14, 16, WeatherType.快晴 | WeatherType.晴れ, "(要フィッシュアイ)ブルートリーチ⇒(!!!スト)"),
                new Fish("バヌバヌヘッド", ToFishingSpot("雲溜まり"), ToFishingBait("ブレーデッドジグ"), WeatherType.晴れ | WeatherType.快晴, "ブレーデッドジグ⇒(!プレ)"),
                new Fish("ゲイラキラー", ToFishingSpot("雲溜まり"), ToFishingBait("ブルートリーチ"), WeatherType.晴れ | WeatherType.快晴, "ブルートリーチ⇒(!!!スト); ブルートリーチ⇒(!プレ)グリロタルパHQ⇒(!プレ)ブルフロッグHQ⇒(!!!スト); ブルートリーチ⇒(!プレ)ブルフロッグHQ⇒(!!!スト)"),
                new Fish("パイッサキラー", ToFishingSpot("雲溜まり"), ToFishingBait("ブレーデッドジグ"), 8, 12, WeatherType.霧, WeatherType.快晴, "ブレーデッドジグ⇒(!プレ)ブルフロッグHQ⇒(!!!スト)"),
                new Fish(
                    "スターフラワー",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("クラウドトップ"), new[] { ToFishingBait("レッドバルーン") }, WeatherType.快晴 | WeatherType.晴れ, "レッドバルーン⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("モック・ウーグル島"), new[] { ToFishingBait("レッドバルーン") }, WeatherType.快晴 | WeatherType.晴れ, "レッドバルーン⇒(!プレ)"),
                    }),
                new Fish("フリーシーモトロ", ToFishingSpot("クラウドトップ"), ToFishingBait("ジャンボガガンボ"), WeatherType.快晴 | WeatherType.晴れ, "ジャンボガガンボ⇒(!!スト)"),
                new Fish("シーロストラタスモトロ", ToFishingSpot("クラウドトップ"), ToFishingBait("ジャンボガガンボ"), 10, 13, WeatherType.快晴 | WeatherType.晴れ, "ジャンボガガンボ⇒(!!!スト)"),
                new Fish("ストームコア", ToFishingSpot("ブルーウィンドウ"), ToFishingBait("レッドバルーン"), WeatherType.風 | WeatherType.曇り | WeatherType.霧, "レッドバルーン⇒(!!スト)"),
                new Fish("ザ・セカンドワン", ToFishingSpot("ブルーウィンドウ"), ToFishingBait("ジャンボガガンボ"), WeatherType.風, "ジャンボガガンボ⇒(!!!スト)"),
                new Fish("天空珊瑚", ToFishingSpot("モック・ウーグル島"), ToFishingBait("ジャンボガガンボ"), 0, 6, WeatherType.快晴 | WeatherType.晴れ, "(要引っ掛け釣り)ジャンボガガンボ⇒(!!スト)"),
                new Fish("バスキングシャーク", ToFishingSpot("モック・ウーグル島"), ToFishingBait("レッドバルーン"), WeatherType.霧, WeatherType.快晴, "レッドバルーン⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!!スト)"),
                new Fish("クラウドバタフライ", ToFishingSpot("モック・ウーグル島"), new[] { ToFishingBait("ジャンボガガンボ"), ToFishingBait("レッドバルーン") }, 5, 7, WeatherType.快晴, "(要スコーピオンフライ×3) ジャンボガガンボ⇒(!!スト); ジャンボガガンボ⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!スト)\n(要スカイフェアリー・セレネ×3) レッドバルーン⇒(!プレ)\nジャンボガガンボ⇒(!!!プレ)"),

                // アジス・ラー
                new Fish(
                    "ブラッドスキッパー",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("アルファ管区"), new[] { ToFishingBait("バイオレットワーム") }, WeatherType.雷, "バイオレットワーム⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("廃液溜まり"), new[] { ToFishingBait("バイオレットワーム") }, WeatherType.雷, "バイオレットワーム⇒(!プレ)"),
                    }),
                new Fish("ハイアラガンクラブ改", ToFishingSpot("アルファ管区"), ToFishingBait("バイオレットワーム"), "バイオレットワーム⇒(!プレ)白金魚HQ⇒(!!!スト)"),
                new Fish("魔科学物質666", ToFishingSpot("廃液溜まり"), ToFishingBait("バイオレットワーム"), WeatherType.晴れ | WeatherType.曇り | WeatherType.雷, "(要フィッシュアイ)バイオレットワーム⇒(!プレ)白金魚HQ⇒(!!!プレ)"),
                new Fish(
                    "オイルイール",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("超星間交信塔"), new[] { ToFishingBait("バイオレットワーム") }, WeatherType.雷, "バイオレットワーム⇒(!プレ)白金魚HQ⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("アジス・ラー旗艦島"), new[] { ToFishingBait("バイオレットワーム") }, WeatherType.雷, "バイオレットワーム⇒(!プレ)白金魚HQ⇒(!!スト)"),
                    }),
                new Fish("オリファントノーズ", ToFishingSpot("超星間交信塔"), ToFishingBait("バイオレットワーム"), 18, 0, WeatherType.雷, "バイオレットワーム⇒(!!スト)"),
                new Fish("セティ", ToFishingSpot("超星間交信塔"), ToFishingBait("バイオレットワーム"), 18, 22, WeatherType.曇り, WeatherType.雷, "(要フィッシュアイ)バイオレットワーム⇒(!!!プレ)"),
                new Fish("バイオピラルク", ToFishingSpot("デルタ管区"), ToFishingBait("ブルートリーチ"), 18, 3, WeatherType.曇り, "ブルートリーチ⇒(!!!スト); ブルートリーチ⇒(!プレ)エーテルアイHQ⇒(!!!スト)"),
                new Fish("バイオガピラルク", ToFishingSpot("デルタ管区"), ToFishingBait("ツチグモ"), 21, 2, WeatherType.曇り, "ツチグモ⇒(!プレ)エーテルアイHQ⇒(!!!スト)"),
                new Fish("プチアクソロトル", ToFishingSpot("パプスの大樹"), ToFishingBait("ブルートリーチ"), 21, 0, "ブルートリーチ⇒(!プレ); ブルートリーチ⇒(!プレ)エーテルアイHQ⇒(!プレ)"),
                new Fish("肺魚", ToFishingSpot("パプスの大樹"), ToFishingBait("ブルートリーチ"), WeatherType.曇り, "ブルートリーチ⇒(!!スト); ブルートリーチ⇒(!プレ)エーテルアイHQ⇒(!!スト)"),
                new Fish("ハンドレッドアイ", ToFishingSpot("パプスの大樹"), ToFishingBait("ツチグモ"), 6, 10, WeatherType.晴れ | WeatherType.曇り | WeatherType.雷, "ツチグモ⇒(!プレ)エーテルアイHQ⇒(!!!プレ)"),
                new Fish("トゥプクスアラ", ToFishingSpot("ハビスフィア"), ToFishingBait("ジャンボガガンボ"), 15, 18, "ジャンボガガンボ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト); ジャンボガガンボ⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!!スト); ジャンボガガンボ⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト)"),
                new Fish("スチュペンデミス", ToFishingSpot("ハビスフィア"), ToFishingBait("ジャンボガガンボ"), WeatherType.晴れ | WeatherType.曇り | WeatherType.雷, "ジャンボガガンボ⇒(!!!スト); ジャンボガガンボ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト); ジャンボガガンボ⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト)"),
                new Fish("クリスタルピジョン", ToFishingSpot("ハビスフィア"), ToFishingBait("ジャンボガガンボ"), WeatherType.晴れ, WeatherType.雷, "(要フィッシュアイ) ジャンボガガンボ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト); ジャンボガガンボ⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト)"),
                new Fish("ジュエリージェリー", ToFishingSpot("アジス・ラー旗艦島"), ToFishingBait("バイオレットワーム"), 20, 3, "バイオレットワーム⇒(!!スト); バイオレットワーム⇒(!プレ)白金魚HQ⇒(!!スト)"),
                new Fish("バレルアイ", ToFishingSpot("アジス・ラー旗艦島"), ToFishingBait("バイオレットワーム"), WeatherType.雷, "(要フィッシュアイ)バイオレットワーム⇒(!プレ)白金魚HQ⇒(!!スト)"),
                new Fish("オプロプケン", ToFishingSpot("アジス・ラー旗艦島"), ToFishingBait("バイオレットワーム"), WeatherType.雷, "バイオレットワーム⇒(!!スト); バイオレットワーム⇒(!プレ)白金魚HQ⇒(!!スト)"),
                new Fish("アラガンブレード・シャーク", ToFishingSpot("アジス・ラー旗艦島"), ToFishingBait("バイオレットワーム"), WeatherType.曇り, WeatherType.雷, "バイオレットワーム⇒(!プレ)白金魚HQ⇒(!!!スト)"),
                new Fish("オパビニア", ToFishingSpot("アジス・ラー旗艦島"), ToFishingBait("バイオレットワーム"), WeatherType.雷, "(要 オプロプケン×3, 要フィッシュアイ) バイオレットワーム⇒(!!スト); バイオレットワーム⇒(!プレ)白金魚HQ⇒(!!スト)\nバイオレットワーム⇒(!プレ)白金魚HQ⇒(!!!プレ)"),

                // 高地ドラヴァニア
                new Fish(
                    "ピピラ・ピラ",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("悲嘆の飛泉"), new[] { ToFishingBait("ゴブリンジグ") }, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, "ゴブリンジグ⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ウィロームリバー"), new[] { ToFishingBait("ゴブリンジグ") }, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, "ゴブリンジグ⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("餌食の台地"), new[] { ToFishingBait("ゴブリンジグ") }, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, "ゴブリンジグ⇒(!!スト)"),
                    }),
                new Fish("草魚", ToFishingSpot("悲嘆の飛泉"), ToFishingBait("ゴブリンジグ"), WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, "ゴブリンジグ⇒(!!スト)"),
                new Fish(
                    "アカザ",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("悲嘆の飛泉"), new[] { ToFishingBait("ブレーデッドジグ") }, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, "ブレーデッドジグ⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("ウィロームリバー"), new[] { ToFishingBait("ツチグモ") }, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, "ツチグモ⇒(!プレ); ツチグモ⇒(!プレ)マクロブラキウムHQ⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("スモーキングウェイスト"), new[] { ToFishingBait("ブレーデッドジグ") }, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, "ブレーデッドジグ⇒(!プレ)"),
                    }),
                new Fish("スケイルリッパー", ToFishingSpot("悲嘆の飛泉"), ToFishingBait("ブルートリーチ"), WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, "ブルートリーチ⇒(!!!スト)"),
                new Fish("ドラヴァニアンバス", ToFishingSpot("ウィロームリバー"), ToFishingBait("ブルートリーチ"), 0, 6, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, "ブルートリーチ⇒(!!スト); ブルートリーチ⇒(!プレ)マクロブラキウムHQ⇒(!!スト)"),
                new Fish("フォークタン", ToFishingSpot("ウィロームリバー"), ToFishingBait("ブルートリーチ"), 12, 16, WeatherType.砂塵 | WeatherType.曇り, WeatherType.快晴, "ブルートリーチ⇒(!!!スト)"),
                new Fish("ポリプテルス", ToFishingSpot("スモーキングウェイスト"), ToFishingBait("ツチグモ"), 21, 3, "ツチグモ⇒(!!スト); ツチグモ⇒(!プレ)マクロブラキウムHQ⇒(!!スト)"),
                new Fish("アクムノタネ", ToFishingSpot("スモーキングウェイスト"), ToFishingBait("ブルートリーチ"), 22, 2, WeatherType.砂塵 | WeatherType.霧 | WeatherType.曇り, "ブルートリーチ⇒(!!!スト)"),
                new Fish("サンダーボルト", ToFishingSpot("餌食の台地"), ToFishingBait("ストーンラーヴァ"), 22, 4, "ストーンラーヴァ⇒(!プレ)マクロブラキウムHQ⇒(!!スト)"),
                new Fish("サンダースケイル", ToFishingSpot("餌食の台地"), ToFishingBait("ストーンラーヴァ"), 6, 8, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, WeatherType.雷, "ストーンラーヴァ⇒(!プレ)マクロブラキウムHQ⇒(!!!スト)"),
                new Fish("アンバーサラマンダー", ToFishingSpot("餌食の台地"), ToFishingBait("ブルートリーチ"), 6, 12, "ブルートリーチ⇒(!!!スト)"),
                new Fish("メテオトータス", ToFishingSpot("モーン大岩窟"), ToFishingBait("マグマワーム"), WeatherType.快晴 | WeatherType.晴れ, "マグマワーム⇒(!プレ)グラナイトクラブHQ⇒(!!!スト)"),
                new Fish("聖竜の涙", ToFishingSpot("モーン大岩窟西"), ToFishingBait("マグマワーム"), 2, 6, "(要フィッシュアイ)マグマワーム⇒(!!!プレ)"),
                new Fish("マグマラウス", ToFishingSpot("アネス・ソー"), ToFishingBait("マグマワーム"), 18, 6, "マグマワーム⇒(!プレ)グラナイトクラブHQ⇒(!!スト)"),
                new Fish("リドル", ToFishingSpot("アネス・ソー"), ToFishingBait("マグマワーム"), 8, 16, WeatherType.快晴 | WeatherType.晴れ, WeatherType.快晴, "(要フィッシュアイ) マグマワーム⇒(!プレ)グラナイトクラブHQ⇒(!!!スト)"),
                new Fish("ラーヴァスネイル", ToFishingSpot("光輪の祭壇"), ToFishingBait("マグマワーム"), WeatherType.快晴 | WeatherType.晴れ, "マグマワーム⇒(!プレ)"),
                new Fish("溶岩王", ToFishingSpot("光輪の祭壇"), ToFishingBait("マグマワーム"), 10, 17, WeatherType.快晴 | WeatherType.晴れ, "(要フィッシュアイ)マグマワーム⇒(!プレ)グラナイトクラブHQ⇒(!!!スト)"),
                new Fish("溶岩帝王", ToFishingSpot("光輪の祭壇"), ToFishingBait("マグマワーム"), 8, 16, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, WeatherType.快晴 | WeatherType.晴れ, "(要フィッシュアイ)マグマワーム⇒(!プレ)グラナイトクラブHQ⇒(!!!スト)"),
                new Fish("プロブレマティカス", ToFishingSpot("光輪の祭壇"), ToFishingBait("マグマワーム"), 10, 15, WeatherType.快晴 | WeatherType.晴れ, "(要フォッシルアロワナ×3, 要フィッシュアイ, 時間帯不問) マグマワーム⇒(!プレ)グラナイトクラブHQ⇒(!!スト)\n(要グラナイトクラブ×5) マグマワーム⇒(!プレ)\nマグマワーム⇒(!プレ)グラナイトクラブHQ⇒(!!!スト)"),

                // 低地ドラヴァニア
                new Fish("水瓶王", ToFishingSpot("サリャク河"), ToFishingBait("ブレーデッドジグ"), "ブレーデッドジグ⇒(!プレ)香魚HQ⇒(!!!スト)"),
                new Fish("マダムバタフライ", ToFishingSpot("クイックスピル・デルタ"), ToFishingBait("ツチグモ"), 21, 2, WeatherType.快晴, "ツチグモ⇒(!プレ)グリロタルパHQ⇒(!!!スト)"),
                new Fish("フィロソファーアロワナ", ToFishingSpot("サリャク河上流"), ToFishingBait("ブルートリーチ"), 13, 20, WeatherType.快晴 | WeatherType.晴れ, "ブルートリーチ⇒(!!!スト); ブルートリーチ⇒(!プレ)グリロタルパHQ⇒(!!!スト); ブルートリーチ⇒(!プレ)香魚HQ⇒(!!!スト)"),
                new Fish("アブトアード", ToFishingSpot("サリャク河上流"), ToFishingBait("ブルートリーチ"), WeatherType.霧 | WeatherType.曇り, "ブルートリーチ⇒(!!スト)"),
                new Fish("スピーカー", ToFishingSpot("サリャク河上流"), ToFishingBait("ツチグモ"), 16, 8, WeatherType.曇り | WeatherType.霧, WeatherType.暴雨, "ツチグモ⇒(!プレ)グリロタルパHQ⇒(!!!スト)"),
                new Fish("鎧魚", ToFishingSpot("サリャク河上流"), ToFishingBait("ツチグモ"), 1, 4, WeatherType.快晴, "(要グリロタルパ×6, 天候不問, 時間帯不問) ツチグモ⇒(!プレ)\nツチグモ⇒(!プレ)グリロタルパHQ⇒(!!!プレ)"),
                new Fish("サリャクカイマン", ToFishingSpot("サリャク河中州"), ToFishingBait("ブレーデッドジグ"), 15, 18, "ブレーデッドジグ⇒(!プレ)ブルフロッグHQ⇒(!!!スト); ブレーデッドジグ⇒(!プレ)香魚HQ⇒(!!!スト)"),
                new Fish("バーサーカーベタ", ToFishingSpot("サリャク河中州"), ToFishingBait("ブルートリーチ"), WeatherType.快晴 | WeatherType.晴れ, "ブルートリーチ⇒(!プレ); ブルートリーチ⇒(!プレ)ブルフロッグHQ⇒(!プレ); ブルートリーチ⇒(!プレ)香魚HQ⇒(!プレ); ブルートリーチ⇒(!プレ)グリロタルパHQ⇒(!プレ)ブルフロッグHQ⇒(!プレ)"),
                new Fish("ゴブリバス", ToFishingSpot("サリャク河中州"), ToFishingBait("ブルートリーチ"), 0, 6, WeatherType.曇り, WeatherType.霧, "ブルートリーチ⇒(!!スト); ブルートリーチ⇒(!プレ)香魚HQ⇒(!!スト)"),
                new Fish("万能のゴブリバス", ToFishingSpot("サリャク河中州"), ToFishingBait("ゴブリンジグ"), 2, 6, WeatherType.雨, WeatherType.暴雨, "ゴブリンジグ⇒(!プレ)香魚HQ⇒(!!!スト)"),

                // ドラヴァニア雲海
                new Fish("キッシング・グラミー", ToFishingSpot("エイル・トーム"), ToFishingBait("ストーンラーヴァ"), 9, 0, "ストーンラーヴァ⇒(!プレ)"),
                new Fish("ヴィゾーヴニル", ToFishingSpot("エイル・トーム"), ToFishingBait("ブルートリーチ"), 8, 9, "ブルートリーチ⇒(!!!スト)"),
                new Fish("モグルグポンポン", ToFishingSpot("グリーンスウォード島"), ToFishingBait("ブルートリーチ"), 10, 13, WeatherType.暴風, WeatherType.快晴, "ブルートリーチ⇒(!!!プレ)\n※ポンポンポンが釣れたらトレードリリース"),
                new Fish("サカサナマズ", ToFishingSpot("ウェストン・ウォーター"), ToFishingBait("ストーンラーヴァ"), 16, 19, "ストーンラーヴァ⇒(!プレ)"),
                new Fish("アミア・カルヴァ", ToFishingSpot("ウェストン・ウォーター"), ToFishingBait("ブルートリーチ"), 8, 12, "ブルートリーチ⇒(!!スト)"),
                new Fish("ボサボサ", ToFishingSpot("ウェストン・ウォーター"), ToFishingBait("ブルートリーチ"), WeatherType.曇り, WeatherType.暴風, "(要フィッシュアイ)ブルートリーチ⇒(!!!スト)"),
                new Fish("サンセットセイル", ToFishingSpot("ランドロード遺構"), ToFishingBait("ジャンボガガンボ"), 15, 17, WeatherType.快晴 | WeatherType.晴れ, "ジャンボガガンボ⇒(!プレ)"),
                new Fish("ラタトスクソウル", ToFishingSpot("ランドロード遺構"), ToFishingBait("ジャンボガガンボ"), 4, 6, WeatherType.快晴 | WeatherType.晴れ, "ジャンボガガンボ⇒(!!!プレ)"),
                new Fish("プテラノドン", ToFishingSpot("ソーム・アル笠雲"), ToFishingBait("ジャンボガガンボ"), 9, 17, "ジャンボガガンボ⇒(!!!スト); ジャンボガガンボ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト); ジャンボガガンボ⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!!スト); ジャンボガガンボ⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト)"),
                new Fish("ディモルフォドン", ToFishingSpot("ソーム・アル笠雲"), ToFishingBait("ジャンボガガンボ"), WeatherType.快晴, WeatherType.暴風, "(要フィッシュアイ) ジャンボガガンボ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト); ジャンボガガンボ⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト)"),
                new Fish("マナセイル", ToFishingSpot("サルウーム・カシュ"), ToFishingBait("レッドバルーン"), 10, 14, WeatherType.快晴 | WeatherType.晴れ, "レッドバルーン⇒(!プレ); レッドバルーン⇒(!!スト)スカイハイフィッシュHQ⇒(!プレ); レッドバルーン⇒(!プレ)スカイフェアリー・セレネHQ⇒(!プレ); レッドバルーン⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!スト)スカイハイフィッシュHQ⇒(!プレ)"),
                new Fish("ブラウンブーメラン", ToFishingSpot("サルウーム・カシュ"), ToFishingBait("レッドバルーン"), WeatherType.曇り, "レッドバルーン⇒(!プレ); レッドバルーン⇒(!!スト)スカイハイフィッシュHQ⇒(!プレ); レッドバルーン⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!スト)スカイハイフィッシュHQ⇒(!プレ)"),
                new Fish("ストームブラッドライダー", ToFishingSpot("サルウーム・カシュ"), ToFishingBait("ジャンボガガンボ"), WeatherType.快晴, WeatherType.暴風, "(要フィッシュアイ)ジャンボガガンボ⇒(!!!スト)"),
                new Fish("ランデロプテルス", ToFishingSpot("サルウーム・カシュ"), ToFishingBait("レッドバルーン"), 5, 8, WeatherType.暴風, "(要スカイハイフィッシュ×5) レッドバルーン⇒(!!スト); レッドバルーン⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!スト)\nレッドバルーン⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト); レッドバルーン⇒(!プレ)スカイフェアリー・セレネHQ⇒(!!スト)スカイハイフィッシュHQ⇒(!!!スト)"),

                // ラールガーズリーチ
                new Fish(
                    "ミューヌフィッシュ",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("ミラージュクリーク上流"), new[] { ToFishingBait("赤虫") }, WeatherType.曇り | WeatherType.霧, "赤虫⇒(!!スト)ギラバニアントラウトHQ⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ティモン川"), new[] { ToFishingBait("ドバミミズ") }, WeatherType.曇り | WeatherType.霧, "ドバミミズ⇒(!プレ)バルーンフロッグHQ⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("夜の森"), new[] { ToFishingBait("ザザムシ") }, WeatherType.曇り | WeatherType.霧, "ザザムシ⇒(!!スト)"),
                    }),
                new Fish("フックスティーラー", ToFishingSpot("ミラージュクリーク上流"), ToFishingBait("赤虫"), "赤虫⇒(!!スト)ギラバニアントラウトHQ⇒(!!!プレ)"),
                new Fish("シデンナマズ", ToFishingSpot("ラールガーズリーチ"), ToFishingBait("赤虫"), WeatherType.雷, "赤虫⇒(!!スト)ギラバニアントラウトHQ⇒(!!!スト)"),
                new Fish("レッドテイル", ToFishingSpot("星導山寺院入口"), ToFishingBait("ドバミミズ"), WeatherType.曇り | WeatherType.霧, "ドバミミズ⇒(!プレ)バルーンフロッグHQ⇒(!!!スト)"),
                new Fish("レッドテイルゾンビー", ToFishingSpot("星導山寺院入口"), ToFishingBait("ドバミミズ"), 8, 12, WeatherType.曇り, "ドバミミズ⇒(!プレ)バルーンフロッグHQ⇒(!!!スト)"),

                // ギラバニア辺境地帯
                new Fish("サーメットヘッド", ToFishingSpot("ティモン川"), ToFishingBait("ザザムシ"), 16, 20, "ザザムシ⇒(!!!スト)"),
                new Fish("クセナカンサス", ToFishingSpot("ティモン川"), ToFishingBait("ザザムシ"), 16, 20, "ザザムシ⇒(!!!スト)サーメットヘッドHQ⇒(!!!スト)"),
                new Fish("レイスフィッシュ", ToFishingSpot("夜の森"), ToFishingBait("ザザムシ"), 0, 4, WeatherType.霧, "ザザムシ⇒(!!スト)"),
                new Fish("サファイアファン", ToFishingSpot("夜の森"), ToFishingBait("ザザムシ"), WeatherType.雷, "ザザムシ⇒(!!!プレ)"),
                new Fish("小流星", ToFishingSpot("流星の尾"), ToFishingBait("サスペンドミノー"), WeatherType.霧 | WeatherType.曇り, "サスペンドミノー⇒(!!スト)"),
                new Fish("ハンテンナマズ", ToFishingSpot("流星の尾"), ToFishingBait("イクラ"), 16, 19, "イクラ⇒(!プレ)"),
                new Fish("ニルヴァーナクラブ", ToFishingSpot("流星の尾"), ToFishingBait("サスペンドミノー"), WeatherType.霧 | WeatherType.曇り, "サスペンドミノー⇒(!!スト)"),
                new Fish("カーディナルフィッシュ", ToFishingSpot("流星の尾"), ToFishingBait("サスペンドミノー"), 19, 23, WeatherType.霧 | WeatherType.曇り, "サスペンドミノー⇒(!!スト)"),
                new Fish("アークビショップフィッシュ", ToFishingSpot("流星の尾"), ToFishingBait("サスペンドミノー"), 12, 16, "サスペンドミノー⇒(!!!スト)"),
                new Fish("タニクダリ", ToFishingSpot("ベロジナ川"), ToFishingBait("ザザムシ"), WeatherType.霧, "ザザムシ⇒(!!!スト)"),
                new Fish("ミラージュマヒ", ToFishingSpot("ミラージュクリーク"), ToFishingBait("ザザムシ"), 4, 8, WeatherType.晴れ | WeatherType.快晴, "ザザムシ⇒(!!!スト)"),
                new Fish("コープスチャブ", ToFishingSpot("ミラージュクリーク"), ToFishingBait("サスペンドミノー"), 20, 0, WeatherType.快晴, "サスペンドミノー⇒(!!!プレ)"),

                // ギラバニア山岳地帯
                new Fish("ボンドスプリッター", ToFishingSpot("夫婦池"), ToFishingBait("サスペンドミノー"), WeatherType.砂塵, "サスペンドミノー⇒(!!!プレ)"),
                new Fish("ドレパナスピス", ToFishingSpot("夫婦池"), ToFishingBait("サスペンドミノー"), WeatherType.砂塵, "(要ボンドスプリッター×2) サスペンドミノー⇒(!!!プレ)\nサスペンドミノー⇒(!!!スト)"),
                new Fish("ナガレクダリ", ToFishingSpot("スロウウォッシュ"), ToFishingBait("赤虫"), 8, 12, "赤虫⇒(!!スト)ギラバニアントラウトHQ⇒(!!!スト)"),
                new Fish("スティールシャーク", ToFishingSpot("ヒース滝"), ToFishingBait("ザザムシ"), WeatherType.快晴, "ザザムシ⇒(!プレ)"),
                new Fish("ラストティアー", ToFishingSpot("ヒース滝"), ToFishingBait("イクラ"), WeatherType.霧, WeatherType.晴れ, "イクラ⇒(!!!プレ)"),
                new Fish(
                    "瞑想魚",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("裁定者の像"), new[] { ToFishingBait("ザザムシ") }, WeatherType.曇り | WeatherType.風 | WeatherType.霧 | WeatherType.砂塵, "ザザムシ⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("ブルズバス"), new[] { ToFishingBait("ザザムシ") }, WeatherType.曇り | WeatherType.風 | WeatherType.霧 | WeatherType.砂塵, "ザザムシ⇒(!!スト)"),
                    }),
                new Fish("裁定魚", ToFishingSpot("裁定者の像"), ToFishingBait("ザザムシ"), WeatherType.風, "ザザムシ⇒(!!!スト)"),
                new Fish("解脱魚", ToFishingSpot("裁定者の像"), ToFishingBait("サスペンドミノー"), WeatherType.曇り | WeatherType.風 | WeatherType.霧 | WeatherType.砂塵, WeatherType.快晴, "サスペンドミノー⇒(!!!スト)"),
                new Fish("ブルズバイト", ToFishingSpot("ブルズバス"), ToFishingBait("赤虫"), WeatherType.曇り | WeatherType.風 | WeatherType.霧 | WeatherType.砂塵, "赤虫⇒(!!スト)"),
                new Fish("ワニガメ", ToFishingSpot("ブルズバス"), ToFishingBait("サスペンドミノー"), WeatherType.晴れ | WeatherType.快晴, "サスペンドミノー⇒(!!スト)"),
                new Fish("ヘモン", ToFishingSpot("ブルズバス"), ToFishingBait("サスペンドミノー"), 16, 20, WeatherType.曇り, "サスペンドミノー⇒(!!!スト)"),
                new Fish("イースタンパイク", ToFishingSpot("アームズ・オブ・ミード"), ToFishingBait("活海老"), WeatherType.晴れ | WeatherType.快晴, "活海老⇒(!!!スト)"),
                new Fish("ロックフィッシュ", ToFishingSpot("アームズ・オブ・ミード"), ToFishingBait("サスペンドミノー"), 12, 16, WeatherType.曇り | WeatherType.風 | WeatherType.霧 | WeatherType.砂塵, "サスペンドミノー⇒(!プレ)"),
                new Fish("アラミガンベール", ToFishingSpot("アームズ・オブ・ミード"), ToFishingBait("サスペンドミノー"), WeatherType.快晴, WeatherType.晴れ, "サスペンドミノー⇒(!!!プレ)"),

                // ギラバニア湖畔地帯
                new Fish("ソルトミル", ToFishingSpot("ロッホ・セル湖"), ToFishingBait("蚕蛹"), WeatherType.曇り | WeatherType.霧, "蚕蛹⇒(!!スト)ロックソルトフィッシュHQ⇒(!!!スト)"),
                new Fish("スカルプター", ToFishingSpot("ロッホ・セル湖"), ToFishingBait("蚕蛹"), 12, 18, WeatherType.雷雨, "蚕蛹⇒(!!スト)ロックソルトフィッシュHQ⇒(!!!スト)"),
                new Fish("ダイヤモンドアイ", ToFishingSpot("ロッホ・セル湖"), ToFishingBait("蚕蛹"), WeatherType.快晴, "(要フィッシュアイ)蚕蛹⇒(!!スト)ロックソルトフィッシュHQ⇒(!!!スト)"),
                new Fish("ステタカントゥス", ToFishingSpot("ロッホ・セル湖"), ToFishingBait("蚕蛹"), 16, 18, WeatherType.雷雨, "(要スカルプター×2, 要フィッシュアイ) 蚕蛹⇒(!!スト)ロックソルトフィッシュHQ⇒(!!!スト)\n(天候不問, ET16:00～17:59, フィッシュアイ不要)蚕蛹⇒(!!スト)ロックソルトフィッシュHQ⇒(!!!スト)"),

                // 紅玉海
                new Fish("クアル", ToFishingSpot("紅玉台場近海"), ToFishingBait("アオイソメ"), 0, 8, WeatherType.雷, WeatherType.曇り, "アオイソメ⇒(!!!スト)"),
                new Fish("紅龍", ToFishingSpot("紅玉台場近海"), ToFishingBait("アオイソメ"), 4, 8, WeatherType.雷, WeatherType.曇り, "アオイソメ⇒(!!!スト)クアルHQ⇒(!!!スト)"),
                new Fish("鰭竜", ToFishingSpot("獄之蓋近海"), ToFishingBait("アオイソメ"), WeatherType.雷, "アオイソメ⇒(!プレ)紅玉海老HQ⇒(!!!スト)"),
                new Fish("ウキキ", ToFishingSpot("獄之蓋近海"), ToFishingBait("アオイソメ"), 8, 12, WeatherType.風, "アオイソメ⇒(!!!スト)"),
                new Fish("菜食王", ToFishingSpot("獄之蓋近海"), ToFishingBait("アオイソメ"), 20, 0, WeatherType.雷, "アオイソメ⇒(!プレ)紅玉海老HQ⇒(!!!スト)"),
                new Fish("オオテンジクザメ", ToFishingSpot("ベッコウ島近海"), ToFishingBait("アオイソメ"), 10, 18, WeatherType.快晴, "アオイソメ⇒(!!スト); アオイソメ⇒(!プレ)紅玉海老HQ⇒(!!スト)"),
                new Fish("ナナツボシ", ToFishingSpot("ベッコウ島近海"), ToFishingBait("アオイソメ"), 10, 18, WeatherType.雷, WeatherType.晴れ, "アオイソメ⇒(!!!スト)"),
                new Fish("春不知", ToFishingSpot("沖之岩近海"), ToFishingBait("アオイソメ"), 16, 20, "アオイソメ⇒(!!!スト)"),
                new Fish("メカジキ", ToFishingSpot("オノコロ島近海"), ToFishingBait("アオイソメ"), 8, 12, WeatherType.風 | WeatherType.曇り, "(要フィッシュアイ)アオイソメ⇒(!!!スト)"),
                new Fish("ウミダイジャ", ToFishingSpot("オノコロ島近海"), ToFishingBait("活海老"), WeatherType.風, "活海老⇒(!プレ)紅玉海老HQ⇒(!!!スト)"),
                new Fish("ギマ", ToFishingSpot("イサリ村沿岸"), ToFishingBait("アオイソメ"), 5, 7, "アオイソメ⇒(!プレ)"),
                new Fish("バリマンボン", ToFishingSpot("イサリ村沿岸"), ToFishingBait("活海老"), 16, 0, WeatherType.快晴 | WeatherType.晴れ, WeatherType.雷, "活海老⇒(!!!スト)"),
                new Fish("ソクシツキ", ToFishingSpot("ゼッキ島近海"), ToFishingBait("活海老"), WeatherType.雷, "活海老⇒(!プレ)紅玉海老HQ⇒(!!!スト)"),

                // ヤンサ
                new Fish("ザクロウミ", ToFishingSpot("アオサギ池"), ToFishingBait("ドバミミズ"), WeatherType.雨, "(要フィッシュアイ)ドバミミズ⇒(!!!スト)"),
                new Fish("オニニラミ", ToFishingSpot("アオサギ川"), ToFishingBait("サスペンドミノー"), WeatherType.晴れ, WeatherType.快晴, "サスペンドミノー⇒(!!!スト)"),
                new Fish("仙寿の翁", ToFishingSpot("ナマイ村溜池"), ToFishingBait("ザザムシ"), 20, 0, WeatherType.快晴, "ザザムシ⇒(!!!スト)\n※ノゴイが釣れたらトレードリリース"),
                new Fish("シャジクノミ", ToFishingSpot("無二江東"), ToFishingBait("ザザムシ"), 0, 6, WeatherType.曇り, WeatherType.晴れ, "ザザムシ⇒(!!!スト)"),
                new Fish("天女魚", ToFishingSpot("無二江西"), ToFishingBait("ザザムシ"), 16, 0, "ザザムシ⇒(!!スト)"),
                new Fish("羽衣美女", ToFishingSpot("無二江西"), ToFishingBait("ザザムシ"), WeatherType.曇り, WeatherType.快晴 | WeatherType.晴れ, "ザザムシ⇒(!!!スト)"),
                new Fish("パンダ蝶尾", ToFishingSpot("梅泉郷"), ToFishingBait("赤虫"), 10, 18, "(要フィッシュアイ)赤虫⇒(!プレ)"),
                new Fish("絹鯉", ToFishingSpot("梅泉郷"), ToFishingBait("ザザムシ"), 4, 8, WeatherType.雨, "(要フィッシュアイ)ザザムシ⇒(!!!スト)"),
                new Fish("羽衣鯉", ToFishingSpot("梅泉郷"), ToFishingBait("ザザムシ"), WeatherType.霧, "(要フィッシュアイ)ザザムシ⇒(!!!スト)"),
                new Fish("ドマウナギ", ToFishingSpot("七彩渓谷"), ToFishingBait("赤虫"), 17, 10, "赤虫⇒(!!スト)"),
                new Fish("ニジノヒトスジ", ToFishingSpot("七彩渓谷"), ToFishingBait("サスペンドミノー"), 12, 16, WeatherType.霧, "サスペンドミノー⇒(!!!スト)"),
                new Fish("紫彩魚", ToFishingSpot("七彩溝"), ToFishingBait("ザザムシ"), 0, 4, "ザザムシ⇒(!!スト)"),
                new Fish("赤彩魚", ToFishingSpot("七彩溝"), ToFishingBait("ザザムシ"), 4, 8, "ザザムシ⇒(!!スト)"),
                new Fish("橙彩魚", ToFishingSpot("七彩溝"), ToFishingBait("ザザムシ"), 4, 8, "ザザムシ⇒(!!スト)赤彩魚HQ⇒(!プレ)"),
                new Fish("藍彩魚", ToFishingSpot("七彩溝"), ToFishingBait("ザザムシ"), 0, 4, "ザザムシ⇒(!!スト)紫彩魚HQ⇒(!プレ)"),
                new Fish("緑彩魚", ToFishingSpot("七彩溝"), ToFishingBait("ザザムシ"), 0, 16, WeatherType.晴れ, WeatherType.晴れ | WeatherType.快晴, "ザザムシ⇒(!プレ)"),
                new Fish("七彩天主", ToFishingSpot("七彩溝"), ToFishingBait("ザザムシ"), 0, 16, WeatherType.晴れ, WeatherType.晴れ | WeatherType.快晴, "(要 藍彩魚×3, ET 00:00～03:59) ザザムシ⇒(!!スト)紫彩魚HQ⇒(!プレ)\n(要 橙彩魚×3, ET 4:00-7:59) ザザムシ⇒(!!スト)赤彩魚HQ⇒(!プレ)\n(要 緑彩魚×5, ET 00:00～15:59, 晴れ⇒晴れ/快晴) ザザムシ⇒(!プレ)\n(天候不問, 時間帯不問) ザザムシ⇒(!!!スト)"),
                new Fish("無二草魚", ToFishingSpot("城下船場"), ToFishingBait("赤虫"), 20, 4, "赤虫⇒(!!!スト)"),
                new Fish("水天一碧", ToFishingSpot("城下船場"), ToFishingBait("ザザムシ"), 16, 0, WeatherType.暴雨, "ザザムシ⇒(!!!スト)"),
                new Fish("雷遁魚", ToFishingSpot("ドマ城前"), ToFishingBait("ザザムシ"), WeatherType.雨 | WeatherType.暴雨, "ザザムシ⇒(!!スト)"),
                new Fish("ボクデン", ToFishingSpot("ドマ城前"), ToFishingBait("ザザムシ"), 12, 14, "ザザムシ⇒(!!!プレ)"),

                // アジムステップ
                new Fish(
                    "メダカ",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("ネム・カール"), new[] { ToFishingBait("ザザムシ") }, WeatherType.快晴, "ザザムシ⇒(!プレ)"),
                        new FishingCondition(ToFishingSpot("シロガネ水路"), new[] { ToFishingBait("ザザムシ") }, WeatherType.快晴, "ザザムシ⇒(!プレ)"),
                    }),
                new Fish("ベジースキッパー", ToFishingSpot("ネム・カール"), ToFishingBait("赤虫"), 8, 12, WeatherType.晴れ, WeatherType.快晴, "赤虫⇒(!!!プレ)"),
                new Fish("ハクビターリング", ToFishingSpot("ハク・カール"), ToFishingBait("ザザムシ"), 0, 4, WeatherType.雨, "ザザムシ⇒(!!スト)"),
                new Fish("明けの旗魚", ToFishingSpot("ハク・カール"), ToFishingBait("ドバミミズ"), 0, 8, WeatherType.晴れ, WeatherType.霧, "ドバミミズ⇒(!プレ)ザガスHQ⇒(!!!スト)"),
                new Fish("ブレードスキッパー", ToFishingSpot("ヤト・カール上流"), ToFishingBait("サスペンドミノー"), 4, 8, WeatherType.霧, "サスペンドミノー⇒(!!!スト)"),
                new Fish("グッピー", ToFishingSpot("アジム・カート"), ToFishingBait("ザザムシ"), 16, 20, WeatherType.晴れ, WeatherType.快晴, "ザザムシ⇒(!プレ)"),
                new Fish("暮れの魚", ToFishingSpot("アジム・カート"), ToFishingBait("ドバミミズ"), 8, 16, WeatherType.雨 | WeatherType.暴風, WeatherType.曇り, "ドバミミズ⇒(!プレ)ザガスHQ⇒(!!!スト)"),
                new Fish("川の長老", ToFishingSpot("タオ・カール"), ToFishingBait("ドバミミズ"), WeatherType.曇り, WeatherType.風 | WeatherType.霧, "ドバミミズ⇒(!プレ)ザガスHQ⇒(!!スト)"),
                new Fish("シンタクヤブリ", ToFishingSpot("タオ・カール"), ToFishingBait("ザザムシ"), 20, 0, "ザザムシ⇒(!!!プレ)"),
                new Fish("ヤトカガン", ToFishingSpot("ヤト・カール下流"), ToFishingBait("ザザムシ"), WeatherType.風, "ザザムシ⇒(!!!プレ)"),
                new Fish("ナーマの愛寵", ToFishingSpot("ドタール・カー"), ToFishingBait("サスペンドミノー"), 4, 8, WeatherType.雨, WeatherType.晴れ | WeatherType.快晴, "サスペンドミノー⇒(!!!スト)"),
                new Fish("神々の愛", ToFishingSpot("ドタール・カー"), ToFishingBait("ザザムシ"), 5, 7, WeatherType.雨, WeatherType.快晴, "ザザムシ⇒(!!!プレ)\n※ナーマの恵みが釣れたらトレードリリース"),

                // クガネ
                new Fish("ノボリリュウ", ToFishingSpot("波止場全体"), ToFishingBait("活海老"), "活海老⇒(!プレ)紅玉海老HQ⇒(!!!スト)"),

                // シロガネ
                new Fish("バクチウチ", ToFishingSpot("シロガネ"), ToFishingBait("アオイソメ"), "アオイソメ⇒(!!!スト)"),
                new Fish("ヒメダカ", ToFishingSpot("シロガネ水路"), ToFishingBait("ドバミミズ"), 4, 6, "ドバミミズ⇒(!!!プレ)"),

                // クリスタリウム
                new Fish("ペンダントヘッド", ToFishingSpot("クリスタリウム居室"), ToFishingBait("蟲箱"), 18, 22, "蟲箱⇒(!!!プレ)"),

                // ユールモア
                new Fish("グランドデイムバタフライ", ToFishingSpot("廃船街"), ToFishingBait("イカの切り身"), 12, 19, WeatherType.快晴, "イカの切り身⇒(!!!プレ)"),

                // レイクランド
                new Fish("プラチナグッピー", ToFishingSpot("錆ついた貯水池"), ToFishingBait("蟲箱"), WeatherType.快晴, "蟲箱⇒(!プレ)"),
                new Fish("アンフォーギヴン・クラブ", ToFishingSpot("始まりの湖"), ToFishingBait("蟲箱"), WeatherType.霧, "蟲箱⇒(!!!プレ)"),
                new Fish("イモータルジョー", ToFishingSpot("ケンの島 (釣り)"), ToFishingBait("蟲箱"), 16, 0, WeatherType.晴れ | WeatherType.快晴, WeatherType.曇り | WeatherType.霧, "蟲箱⇒(!!!スト)"),

                // コルシア島
                new Fish("ホワイトロンゾ", ToFishingSpot("ワッツリバー下流"), ToFishingBait("蟲箱"), 0, 2, "蟲箱⇒(!!!プレ)"),
                new Fish("ブロンズソール", ToFishingSpot("シャープタンの泉"), ToFishingBait("マーブルラーヴァ"), WeatherType.雨, "マーブルラーヴァ⇒(!!!スト)"),
                new Fish("ヘノドゥス", ToFishingSpot("コルシア島沿岸東"), ToFishingBait("ショートビルミノー"), 16, 0, WeatherType.曇り | WeatherType.霧, "ショートビルミノー⇒(!プレ)スピアヘッドHQ⇒(!!!プレ)"),

                // アム・アレーン
                new Fish("カンムリカブリ", ToFishingSpot("砂の川"), ToFishingBait("オヴィムジャーキー"), 0, 6, WeatherType.砂塵, "オヴィムジャーキー⇒(!!スト)ツノカブリHQ⇒(!!!スト); オヴィムジャーキー⇒(!プレ)ミズカキスナヤモリHQ⇒(!!スト)ツノカブリHQ⇒(!!!スト)"),
                new Fish("トゲトカゲ", ToFishingSpot("アンバーヒル"), ToFishingBait("デザートフロッグ"), 10, 18, WeatherType.晴れ | WeatherType.快晴 | WeatherType.灼熱波, "デザートフロッグ⇒(!プレ)ミズカキスナヤモリHQ⇒(!!!スト)"),
                new Fish("クギトカゲ", ToFishingSpot("アンバーヒル"), ToFishingBait("デザートフロッグ"), 12, 16, WeatherType.快晴, "デザートフロッグ⇒(!プレ)ミズカキスナヤモリHQ⇒(!!!スト)"),

                // イル・メグ
                new Fish("フューリィベタ", ToFishingSpot("姿見の湖"), ToFishingBait("蟲箱"), 20, 0, WeatherType.快晴 | WeatherType.晴れ, "蟲箱⇒(!!!スト)"),
                new Fish("ピクシーレインボー", ToFishingSpot("中の子らの流れ"), ToFishingBait("マーブルラーヴァ"), WeatherType.晴れ | WeatherType.快晴, WeatherType.霧, "マーブルラーヴァ⇒(!!!プレ)"),
                new Fish("水泡眼", ToFishingSpot("コラードの排水溝"), ToFishingBait("蟲箱"), WeatherType.快晴, "蟲箱⇒(!プレ)"),

                // ラケティカ大森林
                new Fish("ロックワの衛士", ToFishingSpot("トゥシ・メキタ湖"), ToFishingBait("ロバーボール"), 10, 12, "ロバーボール⇒(!プレ)クラウンテトラHQ⇒(!!スト)エリオプスHQ⇒(!!!スト)"),
                new Fish("ダイヤモンドピピラ", ToFishingSpot("血の酒坏"), ToFishingBait("ロバーボール"), 12, 20, "ロバーボール⇒(!!スト)"),
                new Fish("ブラックジェットストリーム", ToFishingSpot("ロツァトル川"), ToFishingBait("ロバーボール"), 2, 12, WeatherType.曇り, WeatherType.晴れ, "ロバーボール⇒(!!!プレ)"),
                new Fish("常闇魚", ToFishingSpot("ウォーヴンオウス"), ToFishingBait("蟲箱"), 0, 8, "(要フィッシュアイ) 蟲箱⇒(!プレ)"),

                // テンペスト
                new Fish("オンドの溜息", ToFishingSpot("フラウンダーの穴蔵"), ToFishingBait("イカの切り身"), 12, 14, WeatherType.快晴 | WeatherType.晴れ, "イカの切り身⇒(!!!プレ)"),
                new Fish("アーポアク", ToFishingSpot("キャリバンの古巣穴西"), ToFishingBait("イカの切り身"), 12, 16, WeatherType.快晴, "(要フィッシュアイ)イカの切り身⇒(!プレ)エンシェントシュリンプHQ⇒(!!!プレ)"),
                new Fish(
                    "フードウィンカー",
                    new[]
                    {
                        new FishingCondition(ToFishingSpot("キャリバン海底谷北西"), new[] { ToFishingBait("ショートビルミノー") }, WeatherType.晴れ, "ショートビルミノー⇒(!!スト)"),
                        new FishingCondition(ToFishingSpot("キャリバンの古巣穴東"), new[] { ToFishingBait("ショートビルミノー") }, WeatherType.晴れ, "ショートビルミノー⇒(!!スト)"),
                    }),
                new Fish("スターチェイサー", ToFishingSpot("プルプラ洞"), ToFishingBait("イカの切り身"), 6, 10, WeatherType.曇り, "(要フィッシュアイ)イカの切り身⇒(!!!プレ)"),
            })
            {
                _fishes.Add(fish);
                fish.TranslateMemo();
            }
            Fish.RandDifficulty(_fishes);
        }

#if DEBUG
        private void SelfCheck()
        {
            var result =
                _fishes
                    .SelectMany(fish => fish.CheckTranslation())
                    .Concat(
                        _areaGroups
                            .SelectMany(areaGroup => areaGroup.CheckTranslation()))
                    .Concat(
                        _areaGroups
                            .SelectMany(areaGroup => areaGroup.Areas)
                            .SelectMany(area => area.CheckTranslation()))
                    .Concat(
                        _areaGroups
                            .SelectMany(areaGroup => areaGroup.Areas)
                            .SelectMany(area => area.FishingSpots)
                            .SelectMany(spot => spot.CheckTranslation()))
                    .Concat(
                        _fishes
                            .SelectMany(fish => fish.FishingConditions.Select(c => c.FishingSpot))
                            .SelectMany(fishingSpot => fishingSpot.CheckTranslation()))
                    .Concat(
                        _fishingBaites
                            .SelectMany(bait => bait.CheckTranslation()))
                    .Concat(
                        Enum.GetValues(typeof(WeatherType))
                        .Cast<WeatherType>()
                        .Where(weather => weather != WeatherType.None)
                        .SelectMany(weather => weather.CheckTranslation()))
                    .ToArray();
            if (result.Any())
                throw new Exception(string.Format("Can't translate: {0}", string.Join(", ", result.Select(s => string.Format("'{0}'", s)))));
            System.Threading.Tasks.Task.Run(() => new FishDataVerifier().GenerateCode(_fishingSpots));
        }
#endif
    }
}
