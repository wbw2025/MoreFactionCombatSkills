using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong;
using GameData.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameData.DomainEvents.Events;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong
{
    internal class Jianfa2 : CombatSkillEffectBase
    {
        // 正： 发挥五成威力时，【佛王之剑】会额外命中一个部位（同一个部位不会被重复命中）。
        // 正： 发挥五成威力时，下一个【佛王之剑】会额外命中两个部位。
        // TODO : test if this amps e.g. poison
        int stackCount = 0;
        public Jianfa2()
        {
        }
        public Jianfa2(CombatSkillKey skillKey)
            : base(skillKey, 54116)
        {
        }

        public override void OnEnable(DataContext context)
        {
            stackCount = 0;
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_AttackSkillAttackEnd(OnAttackSkillAttackEnd);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.UnRegisterHandler_AttackSkillAttackEnd(OnAttackSkillAttackEnd);
        }
        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId)
            {
                return;
            }
            if (skillId == base.SkillTemplateId && power >= 50 && stackCount < 6)
            {
                if (!IsDirect)
                {
                    stackCount = 1;
                }
                else stackCount++;
                ShowSpecialEffectTips(0);
            }
            if (skillId == Jianfa9.Jianfa9TId && !IsDirect)
            {
                stackCount = 0;
            }
        }

        private void OnAttackSkillAttackEnd(CombatContext context, sbyte hitType, bool hit, int index)
        {
            if ((context.SkillKey.SkillTemplateId != Jianfa9.Jianfa9TId && context.SkillKey.SkillTemplateId != Jianfa1.Jianfa1TId) || index != 3)
            {
                return;
            }
            if (stackCount == 0)
            {
                return;
            }
            List<sbyte> bodyPartRandomPool = ObjectPool<List<sbyte>>.Instance.Get();
            bodyPartRandomPool.Clear();
            for (sbyte part = 0; part < 7; part++)
            {
                if (part != base.CombatChar.SkillAttackBodyPart)
                {
                    bodyPartRandomPool.Add(part);
                }
            }
            int partcount = (IsDirect ? stackCount : stackCount * 3);
            for (int i = 0; i < partcount; i++)
            {
                int partIndex = context.Random.Next(0, bodyPartRandomPool.Count);
                DomainManager.Combat.DoSkillHit(context.Attacker, context.Defender, context.SkillKey.SkillTemplateId, bodyPartRandomPool[partIndex], hitType);
                bodyPartRandomPool.RemoveAt(partIndex);
            }
            ObjectPool<List<sbyte>>.Instance.Return(bodyPartRandomPool);
            ShowSpecialEffectTips(0);
            if (!IsDirect)
            {
                stackCount = 0;
            }
        }

    }
}
