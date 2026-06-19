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
using GameData.Utilities;
using System;
using static GameData.DomainEvents.Events;
namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong
{
    internal class Jianfa7 : CombatSkillEffectBase
    {
        // 正：Desc: ["发挥最少五成威力时，【佛王剑】有+20%的概率在施展后再次从50%无消耗施展。自动施展的【佛王剑】每释放一次，收到该特效加成效果-0.6倍。"]

        // 逆： Desc: ["发挥最少五成威力时，你的下一个【佛王剑】有75%的概率额外从50%施展一次（不叠加）。"]

        int stackCount = 0;
        bool isAutoCast = false;
        double penaltyFactor = 1;
        public Jianfa7()
        {
        }
        public Jianfa7(CombatSkillKey skillKey)
            : base(skillKey, 54111)
        {
        }
        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            stackCount = 0;
            penaltyFactor = 1;
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);


        }
        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            base.OnDisable(context);
        }

        private void OnPrepareSkillBegin(DataContext context, int charId, bool isAlly, short skillId)
        {
            if (charId == base.CharacterId && Jianfa9.SkillIsFoWang(skillId) && isAutoCast)
            {
                DomainManager.Combat.ChangeSkillPrepareProgress(base.CombatChar, base.CombatChar.SkillPrepareTotalProgress * 50 / 100);
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
                    stackCount++;
                else
                    stackCount = 1;
                ShowSpecialEffectTips(0);
            }
            if (stackCount > 0 && Jianfa9.SkillIsFoWang(skillId))
            {
                int recastChance = (int)(stackCount * (IsDirect ? 20 : 75) * penaltyFactor);
                if (!IsDirect)
                {
                    stackCount = 0;
                }
                if (recastChance > 0 && context.Random.Next(100) < recastChance && DomainManager.Combat.CanCastSkill(base.CombatChar, skillId, costFree: true))
                {
                    ShowSpecialEffectTips(0);
                    penaltyFactor *= 0.6;
                    isAutoCast = true;
                    DomainManager.Combat.CastSkillFree(context, base.CombatChar, skillId);
                }
                else
                {
                    isAutoCast = false;
                    penaltyFactor = 1;
                }


            }
        }
        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            if (dataKey.CharId == base.CharacterId && dataKey.FieldId == 199 &&
                dataKey.CombatSkillId == Jianfa9.Jianfa9TId)
            {
                return stackCount * (IsDirect ? 35 : 75);
            }
            return 0;
        }
    }
}
