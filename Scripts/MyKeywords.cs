using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace TsukiyukiMiyako.Scripts;
public class MyKeywords
{
    // Keyword tags appear before the card description text
    [CustomEnum("EQUIPMENT")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Equipment;
    [CustomEnum("SUPPORT")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Support;
    [CustomEnum("SMG")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Smg;
}