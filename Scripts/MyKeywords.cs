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

    /// <summary>
    /// RABBIT —— 寂寞专属牌的统一标签：
    /// **仅能在[blue]寂寞[/blue]状态下打出，且[blue]寂寞[/blue]结束时它给出的增益一并消失。**
    ///
    /// 纯粹是文案归一：以前每张这类牌都要在手牌描述里写两遍
    /// "仅能在寂寞状态下打出 + 寂寞结束时撤销增益"，卡面塞不下。
    /// 现在规则写在关键词里，卡面只留数值。
    /// </summary>
    [CustomEnum("RABBIT")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Rabbit;
}