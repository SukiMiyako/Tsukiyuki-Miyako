using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

// 严格沿用你的卡池格式
[Pool(typeof(MiyakoCardPool))]
public sealed class LightLoad : CustomCardModel
{
    // 2星费用
    public override int CanonicalStarCost => 2;

    // 清除乱码，标准格式变量：1点能量
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new EnergyVar(1) };

    // 悬浮提示（照抄原版）
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip> { base.EnergyHoverTip };

    // 图片路径（统一格式）
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    // 0费 技能 稀有 自身目标
    public LightLoad()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // 核心效果（1:1照抄星位序列）
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    // 升级效果：能量+1（1→2）
    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}