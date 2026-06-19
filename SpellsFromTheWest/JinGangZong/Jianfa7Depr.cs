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
using System.Linq;
namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong
{
    internal class Jianfa7Depr : CombatSkillEffectBase
    {
        // 正：Desc: ["发挥最少五成威力时，【佛王剑】有+20%的概率在施展后再次从50%无消耗施展。自动施展的【佛王剑】每释放一次，收到该特效加成效果-0.6倍。"]


        // 逆： Desc: ["发挥最少五成威力时，你的下一个【佛王剑】有75%的概率额外施展一次（不叠加）。"]


        int[] stackcount = new int[3];

        public Jianfa7Depr()
        {
        }
        public Jianfa7Depr(CombatSkillKey skillKey)
            : base(skillKey, 54111)
        {
        }
        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            stackcount = new int[3];
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            CreateAffectedData(32, EDataModifyType.Add, -1);
            CreateAffectedData(33, EDataModifyType.Add, -1);
            CreateAffectedData(34, EDataModifyType.Add, -1);


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
                    stackcount[context.Random.Next(3)] += 1;

                }
                else
                {
                    stackcount = new int[3] { 1, 1, 1 };
                }
                DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 32);
                DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 33);
                DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 34);
                ShowSpecialEffectTips(0);
            }
            if (stackcount.Any(x=>x>0) && Jianfa9.SkillIsFoWang(skillId) )
            {
                ShowSpecialEffectTips(0);
                if (!IsDirect)
                {
                    stackcount = new int[3];

                    DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 32);
                    DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 33);
                    DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 34);
                }
            }
        }
        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {

            if (dataKey.CharId == base.CharacterId && (dataKey.FieldId >= 32 && dataKey.FieldId <= 34) &&
                 Jianfa9.SkillIsFoWang(dataKey.CombatSkillId) )
            {
                int consumate = base.CombatChar.GetCharacter().GetConsummateLevel();
                int amountPerStack = IsDirect ? (400 + consumate * 50) : ( 300 + consumate * 40);
                if (IsDirect)
                {
                    return currModifyValue + 500 + stackcount[dataKey.FieldId - 32] * amountPerStack;
                }
                else
                {
                    return currModifyValue + 400 + stackcount[dataKey.FieldId - 32] * amountPerStack;
                }
            }
            return 0;
        }
    }
}
