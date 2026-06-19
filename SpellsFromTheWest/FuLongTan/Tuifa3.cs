using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan
{
    // 阴阳神龙钻
    // 正练：敌人同一时刻同时获得内伤和外伤时自动释放；自动释放后封禁2秒。若运用者醉酒且该功法为自动释放，该功法从90%施展。
    // 逆练：自身同一时刻同时获得内伤和外伤时自动释放；自动释放后封禁2秒。若运用者醉酒且该功法为自动释放，该功法从90%施展。
    internal class Tuifa3 : CombatSkillEffectBase
    {
        private const int AutoCastSilenceFrames = 120;

        private const int PairWindowFrames = 2;

        private const int NoInjuryFrame = -1000000;

        private bool _checking;

        private bool _delaying;

        private bool _affecting;

        private int _selfFrame;

        private int _innerInjuryFrame;

        private int _outerInjuryFrame;

        public Tuifa3()
        {
        }

        public Tuifa3(CombatSkillKey skillKey)
            : base(skillKey, 54124)
        {
        }

        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            _checking = false;
            _delaying = false;
            _affecting = false;
            _selfFrame = 0;
            _innerInjuryFrame = NoInjuryFrame;
            _outerInjuryFrame = NoInjuryFrame;

            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.RegisterHandler_AddInjury(OnAddInjury);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);

        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.UnRegisterHandler_AddInjury(OnAddInjury);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.UnRegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            base.OnDisable(context);
        }

        private void OnPrepareSkillBegin(DataContext context, int charId, bool isAlly, short skillId)
        {
            if (charId == base.CharacterId && skillId == base.SkillTemplateId && base.CombatChar.GetAutoCastingSkill() && base.CombatChar.GetCharacter().GetEatingItems().ContainsWine())
            {
                DomainManager.Combat.ChangeSkillPrepareProgress(base.CombatChar, base.CombatChar.SkillPrepareTotalProgress * 90 / 100);
            }
        }

        private void OnAddInjury(DataContext context, CombatCharacter character, sbyte bodyPart, bool isInner, int value, bool changeToOld)
        {
            if (_affecting || _delaying)
            {
                return;
            }

            if (value <= 0)
            {
                return;
            }

            int triggerTargetId = base.IsDirect ? base.EnemyChar.GetId() : base.CharacterId;
            if (character.GetId() != triggerTargetId)
            {
                return;
            }

            if (isInner)
            {
                _innerInjuryFrame = _selfFrame;
            }
            else
            {
                _outerInjuryFrame = _selfFrame;
            }

            if (_innerInjuryFrame > NoInjuryFrame
                && _outerInjuryFrame > NoInjuryFrame
                && System.Math.Abs(_innerInjuryFrame - _outerInjuryFrame) <= PairWindowFrames)
            {
                _delaying = true;
                _innerInjuryFrame = NoInjuryFrame;
                _outerInjuryFrame = NoInjuryFrame;
            }
        }

        private void OnCombatStateMachineUpdateEnd(DataContext context, CombatCharacter combatChar)
        {
            if (combatChar.GetId() != base.CharacterId)
            {
                return;
            }

            _selfFrame++;

            bool checking = _checking;
            _checking = false;
            if (combatChar.NeedUseSkillFreeId >= 0 || !_delaying || _affecting || combatChar.StateMachine.GetCurrentStateType() != CombatCharacterStateType.Idle)
            {
                return;
            }

            _checking = true;
            if (!checking)
            {
                return;
            }

            if (DomainManager.Combat.CanCastSkill(base.CombatChar, base.SkillTemplateId, costFree: true, checkRange: true))
            {
                if (Tuifa9.IsQueued(base.CharacterId))
                {
                    return;
                }

                _delaying = false;
                _affecting = true;
                DomainManager.Combat.CastSkillFree(context, base.CombatChar, base.SkillTemplateId);
                ShowSpecialEffectTips(0);
            }
            else
            {
                _delaying = false;
            }
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }

            if (_affecting)
            {
                _affecting = false;
                _delaying = false;
                DomainManager.Combat.SilenceSkill(context, base.CombatChar, base.SkillTemplateId, AutoCastSilenceFrames, 100);
                return;
            }

            _affecting = false;
        }
    }
}
