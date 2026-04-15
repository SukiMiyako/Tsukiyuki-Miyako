using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace TsukiyukiMiyako.Scripts;
public class MyKeywords
{
    [CustomEnum("EQUIPMENT")]
    // 放在原版卡牌描述的位置，这里是卡牌描述的前面
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Equipment;
    [CustomEnum("SUPPORT")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Support;
}