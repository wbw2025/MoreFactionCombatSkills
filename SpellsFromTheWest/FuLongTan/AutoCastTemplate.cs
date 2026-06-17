
using GameData.Combat.Math;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan
{
    internal class TuifaX : CombatSkillEffectBase
    {
        public TuifaX()
        {
        }

        public TuifaX(CombatSkillKey skillKey)
            : base(skillKey, 54000)
        {
        }


        private bool _checking;

        private bool _delaying;

        private bool _affecting;

        private int _addDamagePercent;


        public override void OnEnable(DataContext context)
        {
            _affecting = false;
            _checking = false;
            _delaying = false;
            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.RegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            Events.RegisterHandler_CastAttackSkillBegin(OnCastAttackSkillBegin);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);

        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.UnRegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            Events.UnRegisterHandler_CastAttackSkillBegin(OnCastAttackSkillBegin);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);

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
            // TODO: check if Tuifa9 is queued.
            if (Tuifa9.IsQueued(base.CharacterId))
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

                }
            }
        }

        private void OnPrepareSkillBegin(DataContext context, int charId, bool isAlly, short skillId)
        {
            //if (charId == base.CharacterId && skillId == base.SkillTemplateId && _affecting)
            //{
            //    DomainManager.Combat.ChangeSkillPrepareProgress(base.CombatChar, base.CombatChar.SkillPrepareTotalProgress * 50 / 100);
            //}
        }

        private void OnCastAttackSkillBegin(DataContext context, CombatCharacter attacker, CombatCharacter defender, short skillId)
        {
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

    }
}
