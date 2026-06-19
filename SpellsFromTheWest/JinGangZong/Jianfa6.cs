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
namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong
{
    internal class Jianfa6 : CombatSkillEffectBase
    {
        // ShortDesc: ["发挥最少五成威力时，你的【佛王剑】会吸取真气，数量为精纯/2+2。"]
        // ShortDesc: ["发挥最少五成威力时，你的下一个【佛王剑】会吸取真气，数量为精纯+10。（不可叠加）"]


        int stackcount = 0;

        public Jianfa6()
        {
        }
        public Jianfa6(CombatSkillKey skillKey)
            : base(skillKey, 54112)
        {
        }
        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            stackcount = 0;
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);


        }
        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            base.OnDisable(context);
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
                int totalCount = IsDirect ? 
                    stackcount * (2 + base.CombatChar.GetCharacter().GetConsummateLevel() / 2) : 
                    stackcount * (10 + base.CombatChar.GetCharacter().GetConsummateLevel());
                if (totalCount > 0 && power >= 50)
                {
                    CombatCharacter enemyChar = base.CurrEnemyChar;
                    for (byte i = 0; i < 3; i++) 
                    {
                        base.CombatChar.AbsorbNeiliAllocation(context, enemyChar, i, totalCount);
                    }
                }
                if (!IsDirect) 
                { 
                    stackcount = 0;
                }
            }
        }

    }
}
