using System;
using System.Collections.Generic;
using System.Linq;

namespace FishingScheduler
{
    class Fish
    {
        private IEnumerable<FishingCondition> _conditions;

        public Fish(string fishName, FishingGround fishingGround, FishingBait fishingBait, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingGround) })
        {
        }

        public Fish(string fishName, FishingGround fishingGround, FishingBait fishingBait, WeatherType weather, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingGround, weather) })
        {
        }

        public Fish(string fishName, FishingGround fishingGround, FishingBait fishingBait, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingGround, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishName, FishingGround fishingGround, FishingBait fishingBait, int hourOfStart, int hourOfEnd, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingGround, hourOfStart, hourOfEnd) })
        {
        }

        public Fish(string fishName, FishingGround fishingGround, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weather, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingGround, hourOfStart, hourOfEnd, weather) })
        {
        }

        public Fish(string fishName, FishingGround fishingGround, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, new[] { new FishingCondition(fishingGround, hourOfStart, hourOfEnd, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, FishingBait fishingBait, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround)))
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, FishingBait fishingBait, WeatherType weather, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround, weather)))
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, FishingBait fishingBait, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround, weatherBefore, weatherAfter)))
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, FishingBait fishingBait, int hourOfStart, int hourOfEnd, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround, hourOfStart, hourOfEnd)))
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weather, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround, hourOfStart, hourOfEnd, weather)))
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, FishingBait fishingBait, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishName, new[] { fishingBait }, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround, hourOfStart, hourOfEnd, weatherBefore, weatherAfter)))
        {
        }

        public Fish(string fishName, FishingGround fishingGround, IEnumerable<FishingBait> fishingBaits, string memo = "")
            : this(fishName, fishingBaits, memo, new[] { new FishingCondition(fishingGround) })
        {
        }

        public Fish(string fishName, FishingGround fishingGround, IEnumerable<FishingBait> fishingBaits, WeatherType weather, string memo = "")
            : this(fishName, fishingBaits, memo, new[] { new FishingCondition(fishingGround, weather) })
        {
        }

        public Fish(string fishName, FishingGround fishingGround, IEnumerable<FishingBait> fishingBaits, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishName, fishingBaits, memo, new[] { new FishingCondition(fishingGround, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishName, FishingGround fishingGround, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, string memo = "")
            : this(fishName, fishingBaits, memo, new[] { new FishingCondition(fishingGround, hourOfStart, hourOfEnd) })
        {
        }

        public Fish(string fishName, FishingGround fishingGround, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weather, string memo = "")
            : this(fishName, fishingBaits, memo, new[] { new FishingCondition(fishingGround, hourOfStart, hourOfEnd, weather) })
        {
        }

        public Fish(string fishName, FishingGround fishingGround, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishName, fishingBaits, memo, new[] { new FishingCondition(fishingGround, hourOfStart, hourOfEnd, weatherBefore, weatherAfter) })
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, IEnumerable<FishingBait> fishingBaits, string memo = "")
            : this(fishName, fishingBaits, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround)))
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, IEnumerable<FishingBait> fishingBaits, WeatherType weather, string memo = "")
            : this(fishName, fishingBaits, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround, weather)))
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, IEnumerable<FishingBait> fishingBaits, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishName, fishingBaits, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround, weatherBefore, weatherAfter)))
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, string memo = "")
            : this(fishName, fishingBaits, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround, hourOfStart, hourOfEnd)))
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weather, string memo = "")
            : this(fishName, fishingBaits, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround, hourOfStart, hourOfEnd, weather)))
        {
        }

        public Fish(string fishName, FishingGround[] fishingGrounds, IEnumerable<FishingBait> fishingBaits, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter, string memo = "")
            : this(fishName, fishingBaits, memo, fishingGrounds.Select(fishingGround => new FishingCondition(fishingGround, hourOfStart, hourOfEnd, weatherBefore, weatherAfter)))
        {
        }

        private Fish(string fishName, IEnumerable<FishingBait> fishingBaits, string memo, IEnumerable<FishingCondition> conditions)
        {
            if (fishName.Trim() != fishName)
                throw new ArgumentException();
            Name = fishName;
            FishingGrounds = conditions.Select(c => c.FishingGround).ToArray();
            FishingBaits = fishingBaits.ToArray();
            Memo = memo;
            _conditions = conditions;
            DifficultyValue = conditions.Min(item => item.DifficultyValue);
            DifficultySymbol = DifficultySymbol.None;
        }

        public string Name { get; set; }
        public IEnumerable<FishingGround> FishingGrounds { get; }
        public IEnumerable<FishingBait> FishingBaits { get; }
        public string Memo { get; set; }
        public double DifficultyValue { get; }
        public DifficultySymbol DifficultySymbol { get; set; }

        public FishChanceTimeRegions GetFishingChance(IKeyValueCollection<string, AreaGroup> areaGroups, EorzeaDateTimeHourRegions wholeRegion, EorzeaDateTime now)
        {
            EorzeaDateTimeHourRegions foundRegion = null;
            FishingCondition foundCondition = null;
            foreach (var condition in _conditions)
            {
                var temp = condition.GetFishingChance(areaGroups, wholeRegion);
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

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return Name.Equals(((Fish)o).Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
