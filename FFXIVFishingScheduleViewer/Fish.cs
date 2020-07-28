using System;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer
{
    class Fish
        : IGameDataObject
    {
        private static int _serialNumber = 0;

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, string memo)
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, WeatherType weather, string memo)
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot, weather) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, int hourOfStart, int hourOfEnd, string memo)
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weather, string memo)
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weather) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, string memo)
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, WeatherType weather, string memo)
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, weather)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, weatherBefore, weatherAfter)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, int hourOfStart, int hourOfEnd, string memo)
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weather, string memo)
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weather)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, new[] { fishingBait }, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weatherBefore, weatherAfter)))
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, string memo)
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, WeatherType weather, string memo)
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot, weather) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, string memo)
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weather, string memo)
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weather) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, fishingBaits, memo, new[] { new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, string memo)
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, WeatherType weather, string memo)
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, weather)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, weatherBefore, weatherAfter)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, string memo)
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weather, string memo)
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weather)))
        {
        }

        public Fish(string fishId, FishingSpot[] fishingSpots, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, fishingBaits, memo, fishingSpots.Select(fishingSpot => new FishingCondition(fishingSpot, hourOfStart, hourOfEnd, weatherBefore, weatherAfter)))
        {
        }

        private Fish(string fishId, IEnumerable<FishingBait> fishingBaits, string memo, IEnumerable<FishingCondition> conditions)
        {
            if (fishId.Trim() != fishId)
                throw new ArgumentException();
            Order = _serialNumber++;
            Id = new GameDataObjectId(GameDataObjectCategory.Fish, fishId);
            NameId = new TranslationTextId(TranslationCategory.Fish, fishId);
            DefaultMemoId = new TranslationTextId(TranslationCategory.FishMemo, fishId);
            FishingSpots = conditions.Select(c => c.FishingSpot).ToArray();
            var baits = fishingBaits.ToArray();
            FishingBaits = baits;
            RawMemoText = memo;
            FishingConditions = conditions.ToArray();
            DifficultyValue = conditions.Min(item => item.DifficultyValue);
            DifficultySymbol = DifficultySymbol.None;
        }

        public int Order { get; }
        public GameDataObjectId Id { get; }
        public TranslationTextId NameId { get; }
        public string Name => Translate.Instance[NameId];
        public IEnumerable<FishingSpot> FishingSpots { get; }
        public IEnumerable<FishingBait> FishingBaits { get; }
        public double DifficultyValue { get; }
        public DifficultySymbol DifficultySymbol { get; private set; }
        public IEnumerable<FishingCondition> FishingConditions { get; }
        public string RawMemoText { get; }
        public string DefaultMemoText => Translate.Instance[DefaultMemoId];
        internal TranslationTextId DefaultMemoId { get; }

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
            return
                Translate.Instance.CheckTranslation(NameId)
                .Concat(Translate.Instance.CheckTranslation(DefaultMemoId));
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
    }
}

