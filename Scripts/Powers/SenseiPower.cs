using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System;

namespace TsukiyukiMiyako.Scripts;

/// <summary>
/// 专注护盾 - 二合一核心BUFF
/// 效果：每层 = 常驻格挡加成 + 充能球集中效果 | 可消耗资源
/// 基于 UnmovablePower 原版逻辑修改，无报错、无乱码
/// </summary>
public sealed class SenseiPower : CustomPowerModel
{
    // 基础属性（标准配置）
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";
    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != null)
        {
            if (cardSource.Owner.Creature != base.Owner)
            {
                return 0m;
            }
        }
        else if (base.Owner != target)
        {
            return 0m;
        }
        if (!props.IsPoweredCardOrMonsterMoveBlock())
        {
            return 0m;
        }
        return base.Amount;
    }

    public override decimal ModifyOrbValue(OrbModel orb, decimal value)
    {
        if (base.Owner.Player != orb.Owner)
            return value;

        return Math.Max(value + (decimal)base.Amount, 0m);
    }
}