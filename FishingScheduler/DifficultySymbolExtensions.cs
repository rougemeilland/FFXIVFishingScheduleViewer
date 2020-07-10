using System;
using System.Windows.Media;

namespace FishingScheduler
{
    static class DifficultySymbolExtensions
    {
        public static string GetShortText(this DifficultySymbol symbol)
        {
            switch (symbol)
            {
                case DifficultySymbol.E:
                    return "E";
                case DifficultySymbol.D:
                    return "D";
                case DifficultySymbol.C:
                    return "C";
                case DifficultySymbol.B:
                    return "B";
                case DifficultySymbol.A:
                    return "A";
                case DifficultySymbol.S:
                    return "S";
                default:
                    throw new ArgumentException();
            }
        }

        public static string GetText(this DifficultySymbol symbol)
        {
            switch (symbol)
            {
                case DifficultySymbol.E:
                    return "ありふれている";
                case DifficultySymbol.D:
                    return "普通";
                case DifficultySymbol.C:
                    return "やや稀";
                case DifficultySymbol.B:
                    return "稀";
                case DifficultySymbol.A:
                    return "非常に稀";
                case DifficultySymbol.S:
                    return "極めて稀";
                default:
                    throw new ArgumentException();
            }
        }

        public static Brush GetBackgroundColor(this DifficultySymbol symbol)
        {
            var converter = new BrushConverter();
            switch (symbol)
            {
                case DifficultySymbol.E:
                    return (Brush)converter.ConvertFromString("deepskyblue");
                case DifficultySymbol.D:
                    return (Brush)converter.ConvertFromString("mediumseagreen");
                case DifficultySymbol.C:
                    return (Brush)converter.ConvertFromString("yellowgreen");
                case DifficultySymbol.B:
                    return (Brush)converter.ConvertFromString("#cca641");
                case DifficultySymbol.A:
                    return (Brush)converter.ConvertFromString("coral");
                case DifficultySymbol.S:
                    return (Brush)converter.ConvertFromString("gold");
                default:
                    return (Brush)converter.ConvertFromString("white");
            }
        }
    }
}
