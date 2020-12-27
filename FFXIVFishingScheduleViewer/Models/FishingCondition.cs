using FFXIVFishingScheduleViewer.Strings;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer.Models
{
    class FishingCondition
    {
        private Fish _fish;
        private ITimeFishingConditionElement _timeCondition;
        private IWeatherFishingConditionElement _weatherCondition;

        public FishingCondition(FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, string memo)
            : this(fishingSpot, fishingBaits, new AlwaysTimeCondition(), new AnyWeatherCondition(), memo)
        {
        }

        public FishingCondition(FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, WeatherType weather, string memo)
            : this(fishingSpot, fishingBaits, new AlwaysTimeCondition(), new SimpleWeatherCondition(fishingSpot.Area, weather), memo)
        {
        }

        public FishingCondition(FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishingSpot, fishingBaits, new AlwaysTimeCondition(), new ChangedWeatherCondition(fishingSpot.Area, weatherBefore, weatherAfter), memo)
        {
        }

        public FishingCondition(FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, string memo)
            : this(fishingSpot, fishingBaits, new SimpleTimeRegionCondition(hourOfStart, hourOfEnd), new AnyWeatherCondition(), memo)
        {
        }

        public FishingCondition(FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weathers, string memo)
            : this(fishingSpot, fishingBaits, new SimpleTimeRegionCondition(hourOfStart, hourOfEnd), new SimpleWeatherCondition(fishingSpot.Area, weathers), memo)
        {
        }

        public FishingCondition(FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishingSpot, fishingBaits, new SimpleTimeRegionCondition(hourOfStart, hourOfEnd), new ChangedWeatherCondition(fishingSpot.Area, weatherBefore, weatherAfter), memo)
        {
        }

        private FishingCondition(FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, ITimeFishingConditionElement timeCondition, IWeatherFishingConditionElement weatherCondition, string memo)
        {
            _fish = null;
            FishingSpot = fishingSpot;
            FishingBaits = fishingBaits.ToArray();
            _timeCondition = timeCondition;
            _weatherCondition = weatherCondition;
            ConditionElements = new[]
            {
                (IFishingConditionElement)_timeCondition,
                _weatherCondition,
            };
            DifficultyValue = timeCondition.DifficultyValue * weatherCondition.DifficultyValue;
            RawMemoText = memo;
            NeedMooching = false;
        }

        public Fish Fish => _fish;
        public FishingSpot FishingSpot { get; }
        public IEnumerable<FishingBait> FishingBaits { get; }
        public IEnumerable<IFishingConditionElement> ConditionElements { get; }
        public double DifficultyValue { get; }
        public string RawMemoText { get; }
        public string DefaultMemoText => Translate.Instance[DefaultMemoId];
        internal TranslationTextId DefaultMemoId { get; private set; }
        internal bool NeedMooching { get; private set; }

        public EorzeaDateTimeHourRegions GetFishingChance(EorzeaDateTimeHourRegions wholeRange, bool useFishEye)
        {
            var region1 = _timeCondition.FindRegions(wholeRange, useFishEye);
            var region2 = _weatherCondition.FindRegions(wholeRange, useFishEye);
            var region = region1.Intersect(region2);
            return region;
        }

        internal void SetParent(Fish fish)
        {
            _fish = fish;
            DefaultMemoId = new TranslationTextId(TranslationCategory.FishMemo, string.Format("{0}**{1}", ((IGameDataObject)fish).InternalId, ((IGameDataObject)FishingSpot).InternalId));
        }

        internal void SetNeedMooching(bool needMooching)
        {
            NeedMooching = needMooching;
        }


        internal IEnumerable<string> CheckTranslation()
        {
            return Translate.Instance.CheckTranslation(DefaultMemoId);
        }
    }
}
