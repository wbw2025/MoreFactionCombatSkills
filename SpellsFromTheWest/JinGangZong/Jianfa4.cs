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
    internal class Jianfa4 : CombatSkillEffectBase
    {
        // Desc: ["发挥最少五成威力时，【佛王剑】会减少你的15点入魔值，但是不会低于战斗开始前。"]
        //  Desc: ["发挥最少五成威力时，你的下一个【佛王剑】会减少你的70点入魔值，但是不会低于战斗开始前。"]


        int stackcount = 0;

        public Jianfa4()
        {
        }
        public Jianfa4(CombatSkillKey skillKey)
            : base(skillKey, 54114)
        {
        }
        public override void OnEnable(DataContext context)
        {
            stackcount = 0;
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);


        }
        public override void OnDisable(DataContext context)
        {
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
            if (stackcount > 0 && Jianfa9.SkillIsFoWang(skillId))
            {
                int delta1 = IsDirect ?
                    stackcount * 15 :
                    stackcount * 70;
                int delta2 = CombatChar.GetCharacter().GetXiangshuInfection() - CombatChar.OriginXiangshuInfection;
                int delta = Math.Min(delta1, delta2);

                if (delta > 0)
                {
                    ShowSpecialEffectTips(0);
                    CombatChar.GetCharacter().ChangeXiangshuInfection(context, -delta);
                }
                if (!IsDirect)
                {
                    stackcount = 0;
                }

            }

        }
    }
}
