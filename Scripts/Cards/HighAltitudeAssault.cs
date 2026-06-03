using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Linq;

namespace TsukiyukiMiyako.Scripts.Cards;

// 加入角色卡池
[Pool(typeof(MiyakoCardPool))]
public sealed class HighAltitudeAssault : CustomCardModel
{
    // 基础配置：3费 | 攻击牌 | 稀有 | 全体敌人目标
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    // 核心变量：21点群体伤害 + 击杀获得3能量（Sunder）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(21m, ValueProp.Move),
        new EnergyVar(3)
    ];

    // 卡牌图片路径（统一格式）
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public HighAltitudeAssault() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出逻辑：群体伤害 + 击杀回费（Sunder判定）
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 群体攻击（无特效，沿用你之前的群攻写法）
        var attackResult = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(base.CombatState!)
            .Execute(choiceContext);

        // 核心：若击杀任意敌人 → 获得3点能量
        if (attackResult.Results.SelectMany(r => r).Any(r => r.WasTargetKilled))
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, base.Owner);
        }
    }

    // 升级：21伤 → 24伤（+3）
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}