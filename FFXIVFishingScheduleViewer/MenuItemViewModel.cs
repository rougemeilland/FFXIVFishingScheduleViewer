using System.Windows.Input;

namespace FFXIVFishingScheduleViewer
{
    abstract class MenuItemViewModel
        : ViewModel
    {
        private class ShowFishInCBHMenuItem
            : ShowPageInCBHMenuItem
        {
            public ShowFishInCBHMenuItem(Fish fish)
            {
                PageItemName = fish.Name;
                PageUrl = fish.GetCBHLink();
            }

            protected override string PageItemName { get; }

            protected override string PageUrl { get; }
        }

        private class ShowSpotInCBHMenuItem
            : ShowPageInCBHMenuItem
        {
            public ShowSpotInCBHMenuItem(FishingSpot spot)
            {
                PageItemName = spot.Name;
                PageUrl = spot.GetCBHLink();
            }

            protected override string PageItemName { get; }

            protected override string PageUrl { get; }
        }

        private class ShowBaitInCBHMenuItem
            : ShowPageInCBHMenuItem
        {
            public ShowBaitInCBHMenuItem(FishingBait bait)
            {
                PageItemName = bait.Name;
                PageUrl = bait.GetCBHLink();
            }

            protected override string PageItemName { get; }

            protected override string PageUrl { get; }
        }

        private abstract class ShowPageInCBHMenuItem
                : MenuItemViewModel
        {
            public override string MenuHeader => string.Format(GUITextTranslate.Instance["Menu.ViewPageInCBH"], PageItemName);
            public override ICommand MenuCommand => new SimpleCommand(p => System.Diagnostics.Process.Start(PageUrl));
            public override bool MenuIsEnabled => PageUrl != null;
            public override bool MenuIsSeparator => false;
            protected abstract string PageItemName { get; }
            protected abstract string PageUrl { get; }
        }

        private class ShowFishInEDBMenuItem
            : ShowPageInEDBMenuItem
        {
            public ShowFishInEDBMenuItem(Fish fish)
            {
                PageItemName = fish.Name;
                PageUrl = fish.GetEDBLink();
            }

            protected override string PageItemName { get; }

            protected override string PageUrl { get; }
        }

        private class ShowBaitInEDBMenuItem
        : ShowPageInEDBMenuItem
        {
            public ShowBaitInEDBMenuItem(FishingBait bait)
            {
                PageItemName = bait.Name;
                PageUrl = bait.GetEDBLink();
            }

            protected override string PageItemName { get; }

            protected override string PageUrl { get; }
        }

        private abstract class ShowPageInEDBMenuItem
                : MenuItemViewModel
        {
            public override string MenuHeader => string.Format(GUITextTranslate.Instance["Menu.ViewPageInEDB"], PageItemName);
            public override ICommand MenuCommand => new SimpleCommand(p => System.Diagnostics.Process.Start(PageUrl));
            public override bool MenuIsEnabled => PageUrl != null;
            public override bool MenuIsSeparator => false;
            protected abstract string PageItemName { get; }
            protected abstract string PageUrl { get; }
        }

        private class SeparatorMenuItem
            : MenuItemViewModel
        {
            public override string MenuHeader => null;

            public override ICommand MenuCommand => new SimpleCommand(p => { });

            public override bool MenuIsEnabled => false;

            public override bool MenuIsSeparator => true;
        }

        private class CancelMenuItem
            : MenuItemViewModel
        {
            public override string MenuHeader => GUITextTranslate.Instance["Menu.Cancel"];

            public override ICommand MenuCommand => new SimpleCommand(p => { });

            public override bool MenuIsEnabled => true;

            public override bool MenuIsSeparator => false;
        }

        private bool _isDisposed;

        protected MenuItemViewModel()
        {
            _isDisposed = false;
        }

        public abstract string MenuHeader { get; }
        public abstract ICommand MenuCommand { get; }
        public abstract bool MenuIsEnabled { get; }
        public abstract bool MenuIsSeparator { get; }

        public static MenuItemViewModel CreateShowFishInCBHMenuItem(Fish fish)
        {
            return new ShowFishInCBHMenuItem(fish);
        }

        public static MenuItemViewModel CreateShowSpotInCBHMenuItem(FishingSpot spot)
        {
            return new ShowSpotInCBHMenuItem(spot);
        }

        public static MenuItemViewModel CreateShowBaitInCBHMenuItem(FishingBait bait)
        {
            return new ShowBaitInCBHMenuItem(bait);
        }

        public static MenuItemViewModel CreateShowFishInEDBMenuItem(Fish fish)
        {
            return new ShowFishInEDBMenuItem(fish);
        }

        public static MenuItemViewModel CreateShowBaitInEDBMenuItem(FishingBait bait)
        {
            return new ShowBaitInEDBMenuItem(bait);
        }

        public static MenuItemViewModel CreateSeparatorMenuItem()
        {
            return new SeparatorMenuItem();
        }

        public static MenuItemViewModel CreateCancelMenuItem()
        {
            return new CancelMenuItem();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
