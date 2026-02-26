using System;
using UnityEngine;

namespace KitchenCaravan.Core
{
    public class GameManager : MonoBehaviour
    {
        public event Action<GameState> StateChanged;
        public event Action<GameState, GameState> StateTransitioned;

        [SerializeField] private GameState _initialState = GameState.Boot;

        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            CurrentState = _initialState;
            StateChanged?.Invoke(CurrentState);
        }

        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState)
            {
                return;
            }

            GameState previous = CurrentState;
            CurrentState = newState;
            StateTransitioned?.Invoke(previous, newState);
            StateChanged?.Invoke(CurrentState);
        }
    }
}
