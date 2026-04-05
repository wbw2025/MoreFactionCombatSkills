using Config;

using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Combat.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameData.DomainEvents.Events;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen
{
    internal class Qimen9 : CombatSkillEffectBase
    {
        // 开始释放时：获得一个杀式。发挥一成威力时：若我方杀式小于三个，则获得两个杀式。
        // 开始释放时：敌人获得一个杀式。发挥一成威力时：若敌方杀式小于三个，则获得两个杀式。

        // 当前实现（废案）
        // 正 每发挥1成，增加运用者护体类（AffectSkillType）所有技能的威力（field 199）
        // 逆 每发挥1成，极短时间封禁敌人的1个 护体类（AffectSkillType） 功法
        // 正：每1成（10%）增加奇门威力2%
        // 逆：每1成（10%）增加 奇门、剑法、刀法、长兵 威力1%

        // TODO: 填入实际的技能分类 id（短整型 sbyte）：
        // `QiMenSkillType` = 奇门
        // `SwordSkillType` = 剑法
        // `BladeSkillType` = 刀法
        // `LongWeaponSkillType` = 长兵
        // 参见 CombatSkillType
        private sbyte QiMenSkillType = 10;
        private sbyte SwordSkillType = 7;
        private sbyte BladeSkillType = 8;
        private sbyte LongWeaponSkillType = 9;

        private int _SkillPower;

        public Qimen9()
        {
        }

        public Qimen9(CombatSkillKey skillKey)
            : base(skillKey, 4100)
        {
        }
        public override void OnEnable(DataContext context)
        {
            _SkillPower = 0;
            if (base.IsDirect)
            {
                // Affect field 199 (Power)
                AffectDatas = new Dictionary<AffectedDataKey, EDataModifyType>();
                AffectDatas.Add(new AffectedDataKey(base.CharacterId, 199, -1), EDataModifyType.Add);
            }
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);

        }
        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        private void OnPrepareSkillBegin(DataContext context, int charId, bool isAlly, short skillId)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }
            DomainManager.Combat.AddTrick(context, base.IsDirect ? base.CombatChar : DomainManager.Combat.GetCombatCharacter(!isAlly), 19, base.IsDirect);
            ShowSpecialEffectTips(0);

        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }
            int shaCount = base.IsDirect ? MoreFactionCombatSkillsBackend.Helpers.Helpers.CountTricks(base.CombatChar, 19) : MoreFactionCombatSkillsBackend.Helpers.Helpers.CountTricks(base.CurrEnemyChar, 19);

            if (shaCount < 3)
            {
                ShowSpecialEffectTips(0);
                DomainManager.Combat.AddTrick(context, base.IsDirect ? base.CombatChar : DomainManager.Combat.GetCombatCharacter(!isAlly), 19, 2, base.IsDirect);
            }
            // For both 正/逆 we just update the stored value; GetModifyValue will apply bonuses depending on direction.
        }

    }
}
