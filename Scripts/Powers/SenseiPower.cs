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
        // 只对玩家自身生效
        if (target.IsMonster || target != base.Owner)
        {
            return block;
        }

        // 核心：每层 + 1 格挡（常驻加成，无消耗逻辑）
        return base.Amount;
    }

    public override decimal ModifyOrbValue(Player player, decimal value)
    {
        if (base.Owner.Player != player)
            return value;

        return Math.Max(value + (decimal)base.Amount, 0m);
    }
}