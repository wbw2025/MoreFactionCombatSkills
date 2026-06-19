using GameData.Combat.Math;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill;
using System;
using System.Linq;
using static GameData.DomainEvents.Events;


namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen
{
    internal class Qimen1 : CombatSkillEffectBase
    {
        // Implementation notes:
        // - This effect mirrors behavior described in other special effects in the codebase.
        // - TODO: verify exact magic numbers / odds if needed.

        // 正（isDirect == true）：以此功法开始攻击敌人时，若我方拥有的杀式大于等于三个，尝试打断敌人正在施展的功法；若我方的杀式为零，则获得一个杀式。
        // 发挥十成威力时：将我方的所有杀式与敌方的所有无式移除并转化为标记：
        //   - 若敌方的战败（Die）标记已达到阈值（近似为 SharedConstValue.DefeatNeedDieMarkCount），则转为必死（Die）标记；
        //   - 否则转为重伤（FatalDamage）标记。

        // 逆（isDirect == false）：行为对称地作用于敌方的杀式/无式（即优先检查并转化敌方的杀式/无式）。

        // Notes: many details are ambiguous in the description; I followed existing helper usages in the project
        // (DomainManager.Combat.AddTrick/RemoveTrick, AppendDieDefeatMark, AppendFatalDamageMark, InterruptSkill).

        // TODO: adjust interrupt odds / thresholds if you want different tuning.

        private const sbyte ShaTrick = 19; // TODO: confirm trick id for "杀式"
        private const sbyte WuTrick = 20; // TODO: confirm trick id for "无式"

        public Qimen1() 
        {
        }

        public Qimen1(CombatSkillKey skillKey)
            : base(skillKey, 54108)
        {
        }

        private int CountTricks(CombatCharacter combatChar, sbyte trick)
        {
            var tricks = combatChar.GetTricks();
            return tricks.Tricks.Sum((p) => (p.Value == trick ? 1 : 0));
        }

        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            Events.RegisterHandler_CastAttackSkillBegin(OnCastAttackSkillBegin);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CastAttackSkillBegin(OnCastAttackSkillBegin);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            base.OnDisable(context);
        }

        private void OnCastAttackSkillBegin(DataContext context, CombatCharacter attacker, CombatCharacter defender, short skillId)
        {
            if (attacker.GetId() != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }

            // Direct: check our own sha count; Reverse: check enemy sha count
            if (base.IsDirect)
            {
                int mySha = CountTricks(base.CombatChar, ShaTrick);
                if (mySha >= 3)
                {
                    // attempt to interrupt enemy's preparing skill
                    DomainManager.Combat.InterruptSkill(context, base.CurrEnemyChar);
                }
                else if (mySha == 0)
                {
                    DomainManager.Combat.AddTrick(context, base.CombatChar, ShaTrick);
                }
            }
            else
            {
                int enemySha = CountTricks(base.CurrEnemyChar, ShaTrick);
                if (enemySha > 3)
                {
                    DomainManager.Combat.InterruptSkill(context, base.CurrEnemyChar);
                }
                else if (enemySha == 0)
                {
                    // when reverse and enemy has zero sha, grant one sha to self (mirrors description ambiguity)
                    DomainManager.Combat.AddTrick(context, base.CombatChar, ShaTrick);
                }
            }
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }

            if (!PowerMatchAffectRequire(power))
            {
                // no further effect if not matching required power
                return;
            }

            // Determine whether to convert to Die (DieMarkList) or FatalDamage (重伤)
            bool convertToDie;
            // Use enemy's current Die mark count as proxy for "高于上限".

            convertToDie = base.CurrEnemyChar.GetDefeatMarkCollection().GetTotalCount() > GlobalConfig.NeedDefeatMarkCount[DomainManager.Combat.GetCombatType()] /2;
            if (!convertToDie) {
            }
            else if (base.IsDirect)
            {
                int mySha = CountTricks(base.CombatChar, ShaTrick);
                if (mySha > 0)
                {
                    DomainManager.Combat.RemoveTrick(context, base.CombatChar, ShaTrick, (byte)mySha);
                    base.CurrEnemyChar.AddDieMark(context, SkillKey, mySha);
                }
            }
            else
            {
                int enemySha = CountTricks(base.CurrEnemyChar, ShaTrick);
                if (enemySha > 0)
                {
                    DomainManager.Combat.RemoveTrick(context, base.CurrEnemyChar, ShaTrick, (byte)enemySha, removedByAlly: false);
                    base.CurrEnemyChar.AddDieMark(context, SkillKey, enemySha);
                    

                }
            }

            ShowSpecialEffectTips(0);
        }
    }
}
