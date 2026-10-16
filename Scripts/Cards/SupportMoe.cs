using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using TsukiyukiMiyako.Scripts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;


// 加入哪个卡池
[Pool(typeof(MiyakoCardPool))]
public class SupportMoe : CustomCardModel
{
    // 基础耗能
    private const int energyCost = 5;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AllEnemies;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    new[]
    {
        HoverTipFactory.FromPower<WeakPower>(),
    };
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4m, ValueProp.Move),
        new RepeatVar(7)
        ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        MyKeywords.Support
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public SupportMoe() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SfxCmd.Play("res://Audio/Moe_ExSkill.mp3");   // 直升机火箭弹发射（Moe_ExSkill.mp3）

        // 等 1 秒：让音频里那七发火箭弹先"发射"，伤害落在命中那一拍上。
        // Cmd.Wait 用的是 Godot 定时器（会跟随游戏时间缩放），玩家开"即时模式"时会被跳过 —— 这是引擎的既定行为。
        await Cmd.Wait(1f);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue) // 造成伤害，数值来源于卡牌的基础伤害属性
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCardCompatibility(this, cardPlay) // 伤害来源于这张卡牌
            .TargetingAllOpponents(base.CombatState!)
            .Execute(choiceContext);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        // 每段 +2：4×7 → 6×7（合计 28 → 42）。
        // 注意这是"每段"加成，改段数时记得同步复核 —— 以前是 7×4，+3/段 = 合计 +12，4 段还扛得住；
        // 现在是 7 段，+3/段就变成 +21 了。
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}