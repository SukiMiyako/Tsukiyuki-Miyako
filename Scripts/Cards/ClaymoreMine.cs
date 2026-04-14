using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts;
[Pool(typeof(MiyakoCardPool))]
public sealed class ClaymoreMine : CustomCardModel
{
    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (base.CombatState == null)
            {
                return false;
            }
            return base.CombatState.HittableEnemies.Any((Creature e) => e.Monster?.IntendsToAttack ?? false);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new DamageVar(6m, ValueProp.Move);
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            yield return CardKeyword.Exhaust;
        }
    }
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public ClaymoreMine()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int damage = DynamicVars.Damage.IntValue;
        if (cardPlay.Target.Monster?.IntendsToAttack ?? false)
        {
            damage *= 2;
        }

        // 无任何特效、无音效、无HitFx
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 升级：伤害+2 → 8 / 意图攻击14
        base.DynamicVars.Damage.UpgradeValueBy(2m);
    }
}