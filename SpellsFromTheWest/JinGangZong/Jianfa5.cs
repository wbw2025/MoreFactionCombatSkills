using GameData.Combat.Math;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen;
using GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong
{
    internal class Jianfa5 : CombatSkillEffectBase
    {
        // ShortDesc: ["发挥最少五成威力时，你的【佛王剑】+33%心神"]
        // ShortDesc: ["发挥最少五成威力时，你的下一个【佛王剑】+75%心神。（不可叠加）"]


        int stackcount = 0;
        void breakExec()
        {
            try
            {
                throw new NotImplementedException();

            }
            catch { }

        }
        public Jianfa5()
        {
        }
        public Jianfa5(CombatSkillKey skillKey)
            : base(skillKey, 4113)
        {
        }
        public override void OnEnable(DataContext context)
        {
            stackcount = 0;
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);

            Events.RegisterHandler_AddDirectDamageValue(OnAddDirectDamageValue);
            breakExec();
        }
        public override void OnDisable(DataContext context)
        {
		    Events.UnRegisterHandler_AddDirectDamageValue(OnAddDirectDamageValue);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }
        private void OnAddDirectDamageValue(DataContext context, int attackerId, int defenderId, sbyte bodyPart, bool isInner, int damageValue, short combatSkillId)
        {
            if (attackerId == base.CharacterId && stackcount > 0 && Jianfa9.SkillIsFoWang(combatSkillId))
            {
                int mindDamageValue = (int)(damageValue * (IsDirect ? 0.33 : 0.75) * stackcount);
                CombatContext combatContext = CombatContext.Create(CombatChar, EnemyChar, -1, combatSkillId, -1);
                DomainManager.Combat.AddMindDamage(combatContext, mindDamageValue);
            }
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId)
            {
                return;
            }
            if (skillId == base.SkillTemplateId && power >= 50)
            {
                if (IsDirect)
                {
                    stackcount += 1;

                }
                else
                {
                    stackcount = 1;
                }
                ShowSpecialEffectTips(0);
            }
            if (stackcount > 0 && Jianfa9.SkillIsFoWang(skillId) )
            {
                ShowSpecialEffectTips(0);
                
                if (!IsDirect) 
                { 
                    stackcount = 0;
                }
            }
        }

    }
}
