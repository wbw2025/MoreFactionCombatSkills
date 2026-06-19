
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

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen
{
    internal class Qimen5 : CombatSkillEffectBase
    {
        public Qimen5()
        {
        }

        public Qimen5(CombatSkillKey skillKey)
            : base(skillKey, 54104)
        {
        }
        // 新实现
        // 正练：当自身获得杀或对手获得无式时增加10%的威力，当此效果大于9层时，如果敌人在攻击范围且可以释放该功法并命中敌人，则自动释放该功法并将层数减少到0层。否则，减少到7层。
        // 逆练：当对手获得无或者杀式时增加10%的威力，当此效果大于9层时，如果敌人在攻击范围且可以释放该功法并命中敌人，则自动释放该功法并将层数减少到0层。否则，减少到7层。

        
        static sbyte wu = 20;
        static sbyte sha = 19;
        static int autoCastThresh = 2;
        private static int CountTricks(CombatCharacter combatChar, sbyte trick)
        {
            var tricks = combatChar.GetTricks();
            return tricks.Tricks.Sum((p) => (p.Value == trick ? 1 : 0));
        }



        private void OnGetShaTrick(DataContext context, int charId, bool isAlly, bool real)
        {
            var enemyChar = DomainManager.Combat.GetCombatCharacter(!base.CombatChar.IsAlly);
            if (charId != base.CombatChar.GetId())
            {
                return;
            }

            var tricks = enemyChar.GetTricks();
            int count;
            if (IsDirect)
            {
                count = tricks.Tricks.Sum((p) => (p.Value == wu || p.Value == sha ? 1 : 0));
            }
            else
            {
                count = tricks.Tricks.Sum((p) => (p.Value == sha ? 1 : 0));
            }
            if (count >= autoCastThresh)
            {
                _delaying = true;

            }
        }

        private void OnGetTrick(DataContext context, int charId, bool isAlly, sbyte trickType, bool usable)
        {
            bool doAdd = false;
            if (IsDirect)
            {
                if ((charId == base.CharacterId && trickType == sha)||(charId == EnemyChar.GetId() && trickType == wu))
                {
                    doAdd = true;
                }
            }
            else
            {
                if (charId == EnemyChar.GetId() && (trickType == sha || trickType == wu))
                {
                    doAdd = true;
                }
            }

            if (doAdd)
            {
                DomainManager.Combat.ChangeSkillEffectCount(context, base.CombatChar, new SkillEffectKey(base.SkillTemplateId, base.IsDirect), 1);
                DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 199);
                if (DomainManager.Combat.GetSkillEffectCount(base.CombatChar, new SkillEffectKey(base.SkillTemplateId, base.IsDirect)) > 9)
                {
                    _delaying = true;
                }
            }
        }



        private bool _checking;

        private bool _delaying;

        private bool _affecting;


        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            CreateAffectedData(199, EDataModifyType.AddPercent, -1);
            _affecting = false;
            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.RegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_GetTrick(OnGetTrick);
            DomainManager.Combat.AddSkillEffect(context,
                base.CombatChar, new SkillEffectKey(base.SkillTemplateId, base.IsDirect), 0,
                10, autoRemoveOnNoCount: false);

        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.UnRegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.UnRegisterHandler_GetTrick(OnGetTrick);
            base.OnDisable(context);

        }


        private void OnCombatStateMachineUpdateEnd(DataContext context, CombatCharacter combatChar)
        {
            if (combatChar.GetId() != base.CharacterId)
            {
                return;
            }
            bool checking = _checking;
            _checking = false;
            if (combatChar.NeedUseSkillFreeId >= 0 || !_delaying || combatChar.StateMachine.GetCurrentStateType() != CombatCharacterStateType.Idle)
            {
                return;
            }
            _checking = true;
            if (checking)
            {
                if (DomainManager.Combat.CanCastSkill(base.CombatChar, base.SkillTemplateId, costFree: true, checkRange: true))
                {
                    _delaying = false;
                    _affecting = true;
                    DomainManager.Combat.CastSkillFree(context, base.CombatChar, base.SkillTemplateId);

                }
                else
                {
                    _delaying = false;
                    short currCount = DomainManager.Combat.GetSkillEffectCount(base.CombatChar, new SkillEffectKey(base.SkillTemplateId, base.IsDirect));
                    DomainManager.Combat.ChangeSkillEffectCount(context, base.CombatChar, new SkillEffectKey(base.SkillTemplateId, base.IsDirect), (short)(7 - currCount));
                    DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 199);

                }
            }
        }

        private void OnPrepareSkillBegin(DataContext context, int charId, bool isAlly, short skillId)
        {
            if (charId == base.CharacterId && skillId == base.SkillTemplateId && _affecting)
            {
                DomainManager.Combat.ChangeSkillPrepareProgress(base.CombatChar, base.CombatChar.SkillPrepareTotalProgress * 50 / 100);
            }
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }
            if (charId == base.CharacterId && skillId == base.SkillTemplateId && _affecting)
            {
                _delaying = false;
                _affecting = false;
                DomainManager.Combat.ChangeSkillEffectToMinCount(context, base.CombatChar, new SkillEffectKey(base.SkillTemplateId, base.IsDirect));
                DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 199);

            }
            /*if (PowerMatchAffectRequire(power))
            {
                //int enemySha = CountTricks(EnemyChar, sha);
                //int enemyWu = CountTricks(EnemyChar, wu);
                //DomainManager.Combat.RemoveTrick(context, EnemyChar, sha, (byte)enemySha);
                //DomainManager.Combat.RemoveTrick(context, EnemyChar, wu, (byte)enemyWu);
                //DomainManager.Combat.AddTrick(context, EnemyChar, sha, (byte)enemyWu);
                //DomainManager.Combat.AddTrick(context, EnemyChar, wu, (byte)enemySha);
            }
            else
            {
            }*/
        }


        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            if (dataKey.CharId == base.CharacterId && dataKey.FieldId == 199 && 
                dataKey.CombatSkillId == base.SkillTemplateId)
            {
                return DomainManager.Combat.GetSkillEffectCount(base.CombatChar, new SkillEffectKey(base.SkillTemplateId, base.IsDirect)) * 10;
            }
            return 0;
        }

    }
}
