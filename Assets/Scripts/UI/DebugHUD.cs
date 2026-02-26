using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KitchenCaravan.Core;
using KitchenCaravan.Run;

namespace KitchenCaravan.UI
{
    public class DebugHUD : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private ChainController _chainController;
        [SerializeField] private CombatSystem _combatSystem;
        [SerializeField] private TMP_Text _tmpText;
        [SerializeField] private Text _uiText;

        private readonly StringBuilder _builder = new StringBuilder();

        private void OnEnable()
        {
            if (_gameManager != null)
            {
                _gameManager.StateChanged += HandleStateChanged;
            }

            if (_chainController != null)
            {
                _chainController.ChainBuilt += HandleChainBuilt;
            }

            if (_combatSystem != null)
            {
                _combatSystem.CombatStarted += HandleCombatStarted;
                _combatSystem.CombatEnded += HandleCombatEnded;
            }

            RefreshText();
        }

        private void OnDisable()
        {
            if (_gameManager != null)
            {
                _gameManager.StateChanged -= HandleStateChanged;
            }

            if (_chainController != null)
            {
                _chainController.ChainBuilt -= HandleChainBuilt;
            }

            if (_combatSystem != null)
            {
                _combatSystem.CombatStarted -= HandleCombatStarted;
                _combatSystem.CombatEnded -= HandleCombatEnded;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            RefreshText();
        }

        private void HandleChainBuilt(Segment[] segments)
        {
            RefreshText();
        }

        private void HandleCombatStarted()
        {
            RefreshText();
        }

        private void HandleCombatEnded()
        {
            RefreshText();
        }

        private void RefreshText()
        {
            if (_tmpText == null && _uiText == null)
            {
                return;
            }

            _builder.Length = 0;
            _builder.AppendLine("Debug HUD");

            if (_gameManager != null)
            {
                _builder.Append("GameState: ");
                _builder.AppendLine(_gameManager.CurrentState.ToString());
            }

            if (_chainController != null)
            {
                _builder.Append("Chain Count: ");
                _builder.AppendLine(_chainController.Segments.Count.ToString());

                _builder.Append("Progress: ");
                _builder.Append(_chainController.CurrentProgressPercent.ToString("0.0"));
                _builder.AppendLine("%");

                BuildSummary summary = _chainController.LastBuildSummary;
                _builder.Append("Last Rule: ");
                _builder.Append(summary.lastRuleId);
                _builder.Append(" | EveryN: ");
                _builder.Append(summary.lastRuleEveryN);
                _builder.Append(" | Type: ");
                _builder.Append(summary.lastLootType);
                _builder.Append(" | Role: ");
                _builder.Append(summary.lastRole);
                _builder.Append(" | Tier: ");
                _builder.Append(summary.lastTier);
                _builder.Append(" | HP: ");
                _builder.Append(summary.lastHp);
                _builder.Append(" | Default: ");
                _builder.AppendLine(summary.lastRuleIsDefault ? "Yes" : "No");
            }

            if (_combatSystem != null)
            {
                _builder.Append("Combat: ");
                _builder.AppendLine(_combatSystem.IsActive ? "Active" : "Idle");
            }

            SetText(_builder.ToString());
        }

        private void SetText(string value)
        {
            if (_tmpText != null)
            {
                _tmpText.text = value;
                return;
            }

            if (_uiText != null)
            {
                _uiText.text = value;
            }
        }
    }
}
