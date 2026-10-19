using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「为了基沃托斯的和平」——2 能量 / 能力 / 稀有 / 自身。
///
/// **每 2 层羁绊，获得 1 点力量**（升级后每 1 层 → 1 点），羁绊变动时自动重新结算。
///
/// 定位：羁绊的**第二个兑现口**。之前这张牌只是一句"获得 X 点力量"，
/// 而她的力量来源已经很多（兔子的獠牙 / 我……我没事 / 遗物），纯加力量既重复又没辨识度。
/// 改成"羁绊转力量"之后，它跟「与你一起」（消耗全部羁绊换一发大的）形成反向选择：
///   · 留着羁绊，持续吃力量 → 选这张
///   · 一次性打出去 → 选「与你一起」
///
/// 寂寞时羁绊 ≤ 0 → 力量自动归 0，跟"寂寞里什么都没有"的设定一致。
/// 结算逻辑见 <see cref="PeaceForKivotosPower"/>。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class PeaceForKivotos : CustomCardModel
{
    // 2费 | 能力 | 稀有 | 自身
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>每几层羁绊换 1 点力量（升级后变成 1 层）。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<PeaceForKivotosPower>(2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PeaceForKivotosPower>(),
        HoverTipFactory.FromPower<SenseiPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public PeaceForKivotos() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SfxCmd.Play("res://Audio/PeaceForKivotos.ogg");
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<PeaceForKivotosPower>(
            new BlockingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars[nameof(PeaceForKivotosPower)].BaseValue,
            Owner.Creature,
            this);

        // 打出瞬间按**当前**羁绊立刻结算一次（之后由 power 监听羁绊变化）
        if (power != null)
        {
            await power.SyncStrength(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        // 兑换率 2 层/力量 → 1 层/力量
        DynamicVars[nameof(PeaceForKivotosPower)].UpgradeValueBy(-1m);
    }
}
