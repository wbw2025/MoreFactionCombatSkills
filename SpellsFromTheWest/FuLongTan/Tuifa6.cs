using System;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan
{
    // 搜神腿
    // 正练 发挥十成威力时，获得(10+精纯)总真气，并打乱自身的真气分布。若运用者醉酒，该数值额外增加50%。
    //       （每一项失去的真气和获得的真气均会触发对应效果）
    // 逆练 发挥十成威力时，敌人失去(10+敌人精纯)总真气，并打乱敌人的真气分布。若运用者醉酒，该数值额外减少50%。
    //       （每一项失去的真气和获得的真气均会触发对应效果）
    internal class Tuifa6 : CombatSkillEffectBase
    {
        private const sbyte FullPower = 100;
        private const int NeiliTypeCount = 4;

        public Tuifa6()
        {
        }

        public Tuifa6(CombatSkillKey skillKey)
            : base(skillKey, 4121)
        {
        }

        public override void OnEnable(DataContext context)
        {
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId || interrupted || power < FullPower)
            {
                return;
            }

            if (base.IsDirect)
            {
                // 正练: self gains (10 + own 精纯) total neili, distributed randomly across 4 types
                int purity = base.CombatChar.GetCharacter().GetConsummateLevel();
                int total = 10 + purity;
                int[] allocation = CreateRandomAllocation(total, NeiliTypeCount, context);
                for (byte type = 0; type < NeiliTypeCount; type++)
                {
                    if (allocation[type] != 0)
                    {
                        base.CombatChar.ChangeNeiliAllocation(context, type, allocation[type]);
                    }
                }
            }
            else
            {
                // 逆练: enemy loses (10 + enemy 精纯) total neili, distributed randomly across 4 types
                CombatCharacter enemy = base.CurrEnemyChar;
                int purity = enemy.GetCharacter().GetConsummateLevel();
                int total = 10 + purity;
                int[] allocation = CreateRandomAllocation(total, NeiliTypeCount, context);
                for (byte type = 0; type < NeiliTypeCount; type++)
                {
                    if (allocation[type] != 0)
                    {
                        enemy.ChangeNeiliAllocation(context, type, -allocation[type]);
                    }
                }
            }

            ShowSpecialEffectTips(0);
        }

        // Randomly splits total into count non-negative parts (stars-and-bars).
        // For example, CreateRandomAllocation(10, 4, ctx) might return [3, 0, 5, 2].
        private static int[] CreateRandomAllocation(int total, int count, DataContext context)
        {
            if (count == 0)
            {
                return new int[0];
            }
            int[] temp = new int[count];
            for (int i = 0; i < count; i++)
            {
                temp[i] = context.Random.Next(0, total);
            }
            Array.Sort(temp);

            int[] result = new int[count];
            result[0] = temp[0];
            for (int i = 1; i < count; i++)
            {
                result[i] = temp[i] - temp[i - 1];
            }
            return result;
        }
    }
}
