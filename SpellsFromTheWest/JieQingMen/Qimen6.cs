using GameData.Common;
using GameData.Domains.Combat;
using GameData.Domains;
using GameData.Domains.SpecialEffect.CombatSkill;
using System;
using System.Collections.Generic;
using System.Linq;
using GameData.Combat.Math;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using static GameData.DomainEvents.Events;
using GameData.Utilities;
using Config;
using GameData.Domains.SpecialEffect;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen
{
    internal class Qimen6 : CombatSkillEffectBase
    {
        // 正练：当自身获得杀或对手获得无式时增加10%造成伤害，最多10层，直到下次释放。当此效果等于7层时，还会造成1个破绽。
        // 逆练：当对手获得无或者杀式时增加10%造成伤害，最多10层，直到下次释放。当此效果等于7层时，还会造成1个破绽。
        public Qimen6()
        {
        }

        public Qimen6(CombatSkillKey skillKey)
            : base(skillKey, 54103)
        {
        }
        ushort powerKey = 199;
        ushort dmgKey = 69;
        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            AffectDatas = new Dictionary<AffectedDataKey, EDataModifyType>();
            AffectDatas.Add(new AffectedDataKey(base.CharacterId, dmgKey, base.SkillTemplateId), EDataModifyType.Add);
            DomainManager.Combat.AddSkillEffect(context,
                base.CombatChar, new SkillEffectKey(base.SkillTemplateId, base.IsDirect), 0, 
                10, autoRemoveOnNoCount: false);


            Events.RegisterHandler_GetTrick(OnGetTrick);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_CastAttackSkillBegin(OnCastAttackSkillBegin);


        }

        private void OnCastAttackSkillBegin(DataContext context, CombatCharacter attacker, CombatCharacter defender, short skillId)
        {
            if (!SkillKey.IsMatch(attacker.GetId(), skillId))
            {
                return;
            }
            if (EffectCount == 7)
            {
                DomainManager.Combat.AddFlaw(context, defender, 3, SkillKey, base.CombatChar.SkillAttackBodyPart);
            }
            
        }

        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            if (dataKey.CharId != base.CharacterId || dataKey.CombatSkillId != base.SkillTemplateId)
            {
                return 0;
            }
            if (dataKey.FieldId == dmgKey)
            {
                return 7 * base.EffectCount * (base.EffectCount == 7 ? 2 : 1);
            }
            return 0;
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (!(charId != base.CharacterId || skillId != base.SkillTemplateId || interrupted) && base.EffectCount > 0)
            {
                DomainManager.Combat.ChangeSkillEffectToMinCount(context, base.CombatChar, new SkillEffectKey(base.SkillTemplateId, base.IsDirect));
                DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, dmgKey);

            }
        }
        private void accumulate(DataContext context)
        {
            DomainManager.Combat.ChangeSkillEffectCount(context, 
                base.CombatChar, new SkillEffectKey(base.SkillTemplateId, base.IsDirect), 1);
            DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, dmgKey);
        }
        sbyte sha = 19;
        sbyte wu = 20;
        private void OnGetTrick(DataContext context, int charId, bool isAlly, sbyte trickType, bool usable)
        {
            CombatCharacter enemyChar = DomainManager.Combat.GetCombatCharacter(!base.CombatChar.IsAlly);
            CombatCharacter myChar = DomainManager.Combat.GetCombatCharacter(base.CombatChar.IsAlly);
            if (charId == myChar.GetId())
            {
                if (IsDirect && trickType == sha)
                {
                    accumulate(context);
                }
            }
            if (charId == enemyChar.GetId())
            {
                if (trickType == wu || (!IsDirect && trickType == sha))
                {
                    accumulate(context);
                }
            }
        }
    }
}
