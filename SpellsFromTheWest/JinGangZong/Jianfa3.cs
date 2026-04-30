using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.Item;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong;
using GameData.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static GameData.DomainEvents.Events;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong
{
    internal class Jianfa3 : CombatSkillEffectBase
    {
        // 正： 该功法施加随机类型的600奇毒。发挥五成威力时，【佛王之剑】额外施加400点奇毒。
        // 逆： 该功法施加随即类型的600奇毒。发挥五成威力时，下一个【佛王之剑】额外施加1000点奇毒（不可叠加）。
        int[] stacks = new int[6];
        public Jianfa3()
        {
        }

        public Jianfa3(CombatSkillKey skillKey)
            : base(skillKey, 4115)
        {
        }
        public override void OnEnable(DataContext context)
        {
            stacks = new int[6];
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        public override void OnDisable(DataContext context)
        {
            stacks = new int[6];
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }
        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId)
            {
                return;
            }
            if (skillId == base.SkillTemplateId && power >= 50)
            {
                sbyte idx = (sbyte)(context.Random.Next(0, 6));
                if (IsDirect)
                {
                    stacks[idx]++;
                }
                else
                {
                    stacks = new int[6];
                    stacks[idx] = 1;
                }


                DomainManager.Combat.AddPoison(context, base.CombatChar, base.CurrEnemyChar,
                    idx, 3, 600 *2 , base.SkillTemplateId,
                    applySpecialEffect: true, canBounce: true, default(ItemKey), isDirectPoison: true);
                ShowSpecialEffectTips(0);
            }
            if (Jianfa9.SkillIsFoWang(skillId))
            {
                for (sbyte i = 0; i < stacks.Length; i++)
                {
                    if (stacks[i] > 0)
                    {
                        DomainManager.Combat.AddPoison(context, base.CombatChar, base.CurrEnemyChar, 
                            i, 3, stacks[i] * (IsDirect ? 400 : 1000 * 2), base.SkillTemplateId,
                            applySpecialEffect: true, canBounce: true, default(ItemKey), isDirectPoison: true);
                    }
                }
                ShowSpecialEffectTips(0);
                if (!IsDirect)
                {
                    stacks = new int[6];

                }
            }

        }


    }
}
