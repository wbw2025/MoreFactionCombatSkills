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
    internal class Jianfa8 : CombatSkillEffectBase
    {
        // 正： 发挥五成威力时，增加【佛王之剑】25%的威力。
        // 逆：发挥五成威力时，增加下一个【佛王之剑】60%的威力。

        // we dont need this
        public static readonly int powerAmpDirect = 25;
        public static readonly int powerAmpInDirect = 60;
        
        int stackCount = 0;

        //public static int GetStackCount(int charId)
        //{
        //    Jianfa8 instance = instances[charId];

        //    return instance.stackCount;
        //}
        //public static void OnCast(int charId)
        //{
        //    Jianfa8 instance = instances[charId];

        //    if (!instance.IsDirect)
        //    {
        //        instance.stackCount = 0;
        //    }
        //}
        public Jianfa8()
        {
        }
        public Jianfa8(CombatSkillKey skillKey)
            : base(skillKey, 54110)
        {
        }
        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            stackCount = 0;
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            CreateAffectedData(199, EDataModifyType.Add, -1);


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
                    stackCount++;
                else
                    stackCount = 1;
                DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 199);
                ShowSpecialEffectTips(0);
            }
            if (stackCount > 0 && skillId == Jianfa9.Jianfa9TId)
            {
                ShowSpecialEffectTips(0);
                if (!IsDirect)
                {
                    stackCount = 0;
                    DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 199);

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
