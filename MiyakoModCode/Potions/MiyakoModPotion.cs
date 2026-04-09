using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace Tsukiyuki_Miyako.MiyakoModCode.Potions;

[Pool(typeof(MiyakoPotionPool))]
public abstract class MiyakoModPotion : CustomPotionModel;