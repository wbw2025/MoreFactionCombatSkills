using System;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Combat.Math;
using GameData.Utilities;
using GameData.Domains.Character;
using GameData.Domains.Item;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen
{
    internal class Qimen4 : CombatSkillEffectBase
    {
        // 正 当击中敌人的部位有破绽时：将这个部位上的所有伤口转为旧伤，每转化一个伤口，施加160*威力赤毒。之后，若发挥了十成威力，使敌人产生一个破绽。
        // 逆 当击中敌人的部位有点穴时：将这个部位上的所有伤口转为旧伤，每转化一个伤口，施加160*威力寒毒。之后，若发挥了十成威力，使敌人产生一个点穴。

        // TODO: 确认毒素 type id：OldRedPoisonType, OldColdPoisonType
        private sbyte OldRedPoisonType = 3;
        private sbyte OldColdPoisonType = 2;

        // TODO: 确认 AddFlaw/AddAcupoint level
        private const sbyte AddFlawLevel = 1;
        private const sbyte AddAcupointLevel = 1;

        public Qimen4()
        {
        }

        public Qimen4(CombatSkillKey skillKey)
            : base(skillKey, 54105)
        {
        }

        public override void OnEnable(DataContext context)
        {
            Events.RegisterHandler_AttackSkillAttackEnd(OnAttackSkillAttackEnd);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_AttackSkillAttackEnd(OnAttackSkillAttackEnd);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        private void OnAttackSkillAttackEnd(CombatContext context, sbyte hitType, bool hit, int index)
        {
            if (context.SkillKey != SkillKey || index != 0) // idx was 3 
            {
                return;
            }
            if (!hit)
            {
                return;
            }

            // Attacker should be our combat char
            if (context.Attacker != base.CombatChar)
            {
                return;
            }

            int part = base.CombatChar.SkillAttackBodyPart;
            if (part < 0)
            {
                return;
            }

            // Direct: require Flaw at part; Reverse: require Acupoint at part
            bool hasTrigger = false;
            if (base.IsDirect)
            {
                FlawOrAcupointCollection flaw = base.CurrEnemyChar.GetFlawCollection();
                if (flaw.BodyPartDict[(sbyte)part].Count > 0)
                {
                    hasTrigger = true;
                }
            }
            else
            {
                FlawOrAcupointCollection acup = base.CurrEnemyChar.GetAcupointCollection();
                if (acup.BodyPartDict[(sbyte)part].Count > 0)
                {
                    hasTrigger = true;
                }
            }

            if (!hasTrigger)
            {
                return;
            }

            // Convert all injuries on this part (outer + inner) to old injuries
            Injuries injuries = base.CurrEnemyChar.GetInjuries();
            Injuries oldInjuries = base.CurrEnemyChar.GetOldInjuries();
            sbyte outer = injuries.Get((sbyte)part, isInnerInjury: false);
            sbyte inner = injuries.Get((sbyte)part, isInnerInjury: true);
            int convertedCount = 0;
            if (outer > 0)
            {
                convertedCount += outer;
                injuries.Change((sbyte)part, isInnerInjury: false, (sbyte)(-outer));
                oldInjuries.Change((sbyte)part, isInnerInjury: false, outer);
            }
            if (inner > 0)
            {
                convertedCount += inner;
                injuries.Change((sbyte)part, isInnerInjury: true, (sbyte)(-inner));
                oldInjuries.Change((sbyte)part, isInnerInjury: true, inner);
            }

            if (convertedCount > 0)
            {
                // commit injuries changes
                base.CurrEnemyChar.SetInjuries(context, injuries);
                base.CurrEnemyChar.SetOldInjuries(oldInjuries, context);

                // Apply poison per converted wound
                int power = context.Attacker.GetAttackSkillPower();
                // TODO: confirm how power scales into poisonValue. Current: poisonValue = 160 * power
                int poisonValuePer = 160 * power;
                sbyte poisonType = base.IsDirect ? OldRedPoisonType : OldColdPoisonType;
                for (int i = 0; i < convertedCount; i++)
                {
                    if (poisonType >= 0)
                    {
                        DomainManager.Combat.AddPoison(context, base.CombatChar, base.CurrEnemyChar, poisonType, 0, poisonValuePer, base.SkillTemplateId, applySpecialEffect: true, canBounce: true, default(ItemKey), isDirectPoison: true);
                    }
                    else
                    {
                        // TODO: poisonType unknown — skip or set TODO behavior
                    }
                }

                ShowSpecialEffectTips(0);
                DomainManager.Combat.AddToCheckFallenSet(base.CurrEnemyChar.GetId());
            }
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }
            // If full power, add Flaw (direct) or Acupoint (reverse) to the attacked part
            if (power >= 100)
            {
                int part = base.CombatChar.SkillAttackBodyPart;
                if (part >= 0)
                {
                    if (base.IsDirect)
                    {
                        DomainManager.Combat.AddFlaw(context, base.CurrEnemyChar, AddFlawLevel, SkillKey, (sbyte)part);
                    }
                    else
                    {
                        DomainManager.Combat.AddAcupoint(context, base.CurrEnemyChar, AddAcupointLevel, SkillKey, (sbyte)part);
                    }
                    ShowSpecialEffectTips(1);
                    DomainManager.Combat.AddToCheckFallenSet(base.CurrEnemyChar.GetId());
                }
            }

        }
    }
}
