using System;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer
{
    class Fish
        : IGameDataObject
    {
        private static int _serialNumber = 0;
        private string _rawId;

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, new[] { fishingBait }, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, WeatherType weather, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, new[] { fishingBait }, weather, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, new[] { fishingBait }, weatherBefore, weatherAfter, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, int hourOfStart, int hourOfEnd, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, new[] { fishingBait }, hourOfStart, hourOfEnd, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weather, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, new[] { fishingBait }, hourOfStart, hourOfEnd, weather, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, new[] { fishingBait }, hourOfStart, hourOfEnd, weatherBefore, weatherAfter, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, fishingBaits, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, WeatherType weather, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, fishingBaits, weather, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, fishingBaits, weatherBefore, weatherAfter, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, fishingBaits, hourOfStart, hourOfEnd, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weather, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, fishingBaits, hourOfStart, hourOfEnd, weather, memo) })
        {
        }

        public Fish(string fishId, FishingSpot fishingSpot, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo)
            : this(fishId, new[] { new FishingCondition(fishingSpot, fishingBaits, hourOfStart, hourOfEnd, weatherBefore, weatherAfter, memo) })
        {
        }

        public Fish(string fishId, IEnumerable<FishingCondition> conditions)
        {
            if (fishId.Trim() != fishId)
                throw new ArgumentException();
            _rawId = fishId;
            Order = _serialNumber++;
            Id = new GameDataObjectId(GameDataObjectCategory.Fish, fishId);
            NameId = new TranslationTextId(TranslationCategory.Fish, fishId);
            FishingConditions = conditions.ToArray();
            DifficultyValue = conditions.Min(item => item.DifficultyValue);
            DifficultySymbol = DifficultySymbol.None;
            foreach (var condition in FishingConditions)
                condition.SetParent(this);
        }

        string IGameDataObject.InternalId => _rawId;
        public int Order { get; }
        public GameDataObjectId Id { get; }
        public TranslationTextId NameId { get; }
        public string Name => Translate.Instance[NameId];
        public double DifficultyValue { get; }
        public DifficultySymbol DifficultySymbol { get; private set; }
        public IEnumerable<FishingCondition> FishingConditions { get; }

        public IEnumerable<FishChanceTimeRegions> GetFishingChance(EorzeaDateTimeHourRegions wholeRegion)
        {
            return
                FishingConditions
                .Select(condition => new FishChanceTimeRegions(condition, condition.GetFishingChance(wholeRegion)))
                .ToArray();
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
                FishingConditions.SelectMany(condition => condition.CheckTranslation())
                .Concat(Translate.Instance.CheckTranslation(NameId));
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

