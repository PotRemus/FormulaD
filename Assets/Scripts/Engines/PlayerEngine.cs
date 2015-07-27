using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FormuleD.Managers;
using System;
using FormuleD.Models;
using UnityEngine.UI;
using FormuleD.Models.Board;
using FormuleD.Models.Contexts;
using FormuleD.Managers.Course.Player;
using FormuleD.Managers.Course.Board;

namespace FormuleD.Engines
{
    public class PlayerEngine : MonoBehaviour
    {
        public CarLayoutManager carLayoutManager;

        public DePanelManager dePanelManager;
        public PlayerPanelManager playerPanelManager;
        public FeaturePanelManager featurePanelManager;

        private PlayerContext _currentPlayer;

        public static PlayerEngine Instance = null;
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple instances of GameEngine!");
            }
            Instance = this;
        }

        public void LoadPlayers(List<CaseManager> starts)
        {
            //this.players = players;

            for (int i = 0; i < ContextEngine.Instance.gameContext.players.Count; i++)
            {
                var player = ContextEngine.Instance.gameContext.players[i];
                if (player.turnHistories == null || !player.turnHistories.Any())
                {
                    var startCase = starts[i];
                    if (player.turnHistories == null)
                    {
                        player.turnHistories = new List<HistoryContext>();
                    }
                    if (!player.turnHistories.Any())
                    {
                        player.turnHistories.Add(new HistoryContext());
                    }
                    var history = player.turnHistories.Last();
                    history.path = new List<IndexDataSource>() { startCase.itemDataSource.index };
                }
            }

            carLayoutManager.BuildCars(ContextEngine.Instance.gameContext.players);
            playerPanelManager.BuildPlayers(ContextEngine.Instance.gameContext.players);

            _currentPlayer = ContextEngine.Instance.gameContext.players.FirstOrDefault(p => p.IsPlayable() && p.state != PlayerStateType.Waiting);
            if (_currentPlayer == null)
            {
                this.NextPlayer();
            }
            else
            {
                this.SelectedPlayerView(_currentPlayer);
            }
        }

        private PlayerContext _previousViewPlayer;
        public void SelectedPlayerView(PlayerContext playerContext)
        {
            RaceEngine.Instance.CleanCurrent();
            if (_previousViewPlayer != null && _previousViewPlayer.turnHistories.Count > 1)
            {
                var previousHistory = _previousViewPlayer.turnHistories.Last();
                if (previousHistory.path.Count > 0)
                {
                    BoardEngine.Instance.CleanRoute(previousHistory.GetFullPath().Select(r => BoardEngine.Instance.GetCase(r)).ToList(), _previousViewPlayer.GetColor());
                }
            }
            _previousViewPlayer = playerContext;
            playerPanelManager.SelectedPlayer(playerContext);
            this.UpdateFeature(playerContext);
            dePanelManager.UpdateDe(playerContext);
            if (playerContext.state == PlayerStateType.RollDice)
            {
                int gear = playerContext.turnHistories.Last().gear;
                if (gear <= 0)
                {
                    gear = 1;
                }
                var de = dePanelManager.buttonDes[gear - 1];
                RaceEngine.Instance.OnViewGear(gear, de.min, de.max);
            }
            else if (playerContext.state == PlayerStateType.ChoseRoute)
            {
                var gear = playerContext.currentTurn.gear;
                RaceEngine.Instance.OnViewRollDice(gear, playerContext.currentTurn.de);
            }
            else if (playerContext.state == PlayerStateType.Aspiration)
            {
                RaceEngine.Instance.OnAspiration(true);
            }

            if (playerContext.turnHistories.Count > 1)
            {
                var previousHistory = playerContext.turnHistories.Last();
                if (previousHistory.path.Count > 0)
                {
                    BoardEngine.Instance.DrawRoute(previousHistory.GetFullPath().Select(r => BoardEngine.Instance.GetCase(r)).ToList(), playerContext.GetColor());
                }
            }
        }

        public void SelectedDe(int gear)
        {
            this.UpdateFeature(_currentPlayer);
            var features = this.ComputeDemotion(gear);
            featurePanelManager.WarningFeature(_currentPlayer.features, features);
            dePanelManager.UpdateSelectedDe(gear);
        }

        public void LoadAspirationView()
        {
            dePanelManager.UpdateDe(_currentPlayer);
        }

        private FeatureContext ComputeDemotion(int targetGear)
        {
            FeatureContext result = _currentPlayer.features.Clone();
            var previousGear = _currentPlayer.turnHistories.Last().gear;
            var gearDif = previousGear - targetGear;
            if (gearDif >= 4)
            {
                result.gearbox = _currentPlayer.features.gearbox - 1;
                result.brake = _currentPlayer.features.brake - 1;
                result.motor = _currentPlayer.features.motor - 1;
            }
            else if (gearDif == 3)
            {
                result.gearbox = _currentPlayer.features.gearbox - 1;
                result.brake = _currentPlayer.features.brake - 1;
            }
            else if (gearDif == 2)
            {
                result.gearbox = _currentPlayer.features.gearbox - 1;
            }
            return result;
        }

        public void UpdateGear(int gear, int deValue, out int minValue, out int maxValue)
        {
            if (_currentPlayer != null && _currentPlayer.state == PlayerStateType.RollDice)
            {
                _currentPlayer.features = this.ComputeDemotion(gear);
                this.UpdateFeature(_currentPlayer);
                _currentPlayer.currentTurn.gear = gear;
                _currentPlayer.currentTurn.de = deValue;
                _currentPlayer.state = PlayerStateType.ChoseRoute;
                dePanelManager.UpdateDe(_currentPlayer);

                this.SelectedPlayerView(_currentPlayer);
            }

            if (_currentPlayer.state == PlayerStateType.ChoseRoute)
            {
                if (_currentPlayer.features.brake >= 3)
                {
                    if (_currentPlayer.features.tire >= 3)
                    {
                        minValue = deValue - 6;
                    }
                    else if (_currentPlayer.features.tire == 2)
                    {
                        minValue = deValue - 5;
                    }
                    else if (_currentPlayer.features.tire == 1)
                    {
                        minValue = deValue - 4;
                    }
                    else
                    {
                        minValue = deValue - 3;
                    }
                }
                else if (_currentPlayer.features.brake == 2)
                {
                    minValue = deValue - 2;
                }
                else if (_currentPlayer.features.brake == 1)
                {
                    minValue = deValue - 1;
                }
                else
                {
                    minValue = deValue;
                }
                if (minValue < 1)
                {
                    minValue = 1;
                }
                maxValue = deValue;
            }
            else
            {
                minValue = deValue;
                maxValue = deValue;
            }
        }

        public FeatureContext ComputeUseBrake(FeatureContext features, PlayerStateType state, int nbCase)
        {
            FeatureContext result = features.Clone();
            int difDe = 0;
            if (state == PlayerStateType.Aspiration)
            {
                difDe = 3 - nbCase;
            }
            else
            {
                difDe = _currentPlayer.currentTurn.de - nbCase;
            }
            if (difDe >= 6)
            {
                result.brake = features.brake - 3;
                result.tire = _currentPlayer.features.tire - 3;
            }
            else if (difDe == 5)
            {
                result.brake = features.brake - 3;
                result.tire = features.tire - 2;
            }
            else if (difDe == 4)
            {
                result.brake = features.brake - 3;
                result.tire = features.tire - 1;
            }
            else if (difDe == 3)
            {
                result.brake = features.brake - 3;
            }
            else if (difDe == 2)
            {
                result.brake = features.brake - 2;
            }
            else if (difDe == 1)
            {
                result.brake = features.brake - 1;
            }
            return result;
        }

        public FeatureContext ComputeOutOfBend(FeatureContext features, int nbOut)
        {
            FeatureContext result = features.Clone();
            result.tire = features.tire - nbOut;
            if (result.tire < 0)
            {
                result.tire = 0;
            }
            return result;
        }

        public void DisplayWarningFeature(FeatureContext features)
        {
            featurePanelManager.WarningFeature(_currentPlayer.features, features);
        }

        public void SelectedRoute(List<CaseManager> route, int nbOutOfBend, bool isBadWay)
        {
            _currentPlayer.features = this.ComputeUseBrake(_currentPlayer.features, _currentPlayer.state, route.Count - 1);
            _currentPlayer.features = this.ComputeOutOfBend(_currentPlayer.features, nbOutOfBend);

            bool hasNewLap = BoardEngine.Instance.ContainsFinishCase(route.Select(r => r.itemDataSource.index));
            if (hasNewLap)
            {
                _currentPlayer.lap = _currentPlayer.lap + 1;
            }

            var endCase = route.Last();
            if (endCase.bendDataSource != null)
            {
                if (nbOutOfBend == 0 && _currentPlayer.state != PlayerStateType.Aspiration)
                {
                    if (_currentPlayer.lastBend == endCase.bendDataSource.name)
                    {
                        _currentPlayer.stopBend = _currentPlayer.stopBend + 1;
                    }
                    else
                    {
                        _currentPlayer.stopBend = 1;
                        _currentPlayer.lastBend = endCase.bendDataSource.name;
                    }
                }
                else if (_currentPlayer.lastBend != endCase.bendDataSource.name)
                {
                    _currentPlayer.stopBend = 0;
                    _currentPlayer.lastBend = endCase.bendDataSource.name;
                }
            }
            else
            {
                _currentPlayer.lastBend = string.Empty;
                _currentPlayer.stopBend = 0;
            }

            foreach (var current in route.Where(c => c.isDangerous))
            {
                var de = RaceEngine.Instance.BlackDice();
                if (de <= 4)
                {
                    _currentPlayer.features.handling = _currentPlayer.features.handling - 1;
                }
            }

            if (isBadWay)
            {
                _currentPlayer.state = PlayerStateType.Dead;
            }
            else if (_currentPlayer.state == PlayerStateType.ChoseRoute)
            {
                if (_currentPlayer.turnHistories.Any())
                {
                    var previousHistory = _currentPlayer.turnHistories.Last();
                    if (previousHistory.path.Count > 0)
                    {
                        BoardEngine.Instance.CleanRoute(previousHistory.GetFullPath().Select(r => BoardEngine.Instance.GetCase(r)).ToList(), _currentPlayer.GetColor());
                    }
                }
                _currentPlayer.currentTurn.path = route.Select(r => r.itemDataSource.index).ToList();
                _currentPlayer.currentTurn.outOfBend = nbOutOfBend;
                _currentPlayer.currentTurn.aspirations = new List<List<IndexDataSource>>();
                if (nbOutOfBend == 0)
                {
                    _currentPlayer.currentTurn.hasAspiration = this.CheckAspiration(route);
                    if (_currentPlayer.currentTurn.hasAspiration)
                    {
                        _currentPlayer.state = PlayerStateType.Aspiration;
                        this.CheckIsDead(_currentPlayer);
                    }
                    else
                    {
                        this.EndTurn();
                    }
                }
                else
                {
                    _currentPlayer.currentTurn.hasAspiration = false;
                    this.EndTurn();
                }
            }
            else if (_currentPlayer.state == PlayerStateType.Aspiration)
            {
                _currentPlayer.currentTurn.aspirations.Add(route.Select(r => r.itemDataSource.index).ToList());
                _currentPlayer.currentTurn.hasAspiration = this.CheckAspiration(route);
                if (!_currentPlayer.currentTurn.hasAspiration)
                {
                    _currentPlayer.state = PlayerStateType.Aspiration;
                    this.CheckIsDead(_currentPlayer);
                }
                else
                {
                    this.EndTurn();
                }
            }
        }

        public void CheckIsDead(PlayerContext player)
        {
            bool isDead = false;
            if (player.features.tire < 0 || player.features.brake < 0 || player.features.body == 0 || player.features.motor == 0 || player.features.handling == 0)
            {
                isDead = true;
            }

            if (isDead)
            {
                player.state = PlayerStateType.Dead;
            }
        }

        public void EndTurn()
        {
            RaceEngine.Instance.OnClash(_currentPlayer);
            RaceEngine.Instance.OnBrokenEngine(_currentPlayer);
            _currentPlayer.state = PlayerStateType.EndTurn;
            this.CheckIsDead(_currentPlayer);
        }

        public void MoveCar(List<CaseManager> route)
        {
            if (route != null && _currentPlayer != null)
            {
                var carManager = carLayoutManager.FindCarManager(_currentPlayer);
                if (carManager != null)
                {
                    var firstCase = route.First();
                    firstCase.hasPlayer = false;
                    var lastCase = route.Last();
                    lastCase.hasPlayer = true;
                    var nextCase = BoardEngine.Instance.GetNextCase(lastCase);

                    var nextPosition = new Vector3(nextCase.transform.position.x, nextCase.transform.position.y, carManager.transform.position.z);
                    var movements = route.Select(d => new Vector3(d.transform.position.x, d.transform.position.y, carManager.transform.position.z));
                    carManager.AddMovements(movements, nextPosition);
                }
            }
        }

        public PlayerContext GetCurrent()
        {
            return _currentPlayer;
        }

        public void ResetPlayerState()
        {
            this.UpdateFeature(_currentPlayer);
        }

        public void NextPlayer()
        {
            PlayerContext nextPlayer = null;
            if (_currentPlayer != null)
            {
                var history = _currentPlayer.currentTurn;
                var fullPath = history.GetFullPath().ToList();
                if (history.outOfBend > 0 && _currentPlayer.features.tire == 0 && fullPath.Count > 1)
                {
                    var carManager = carLayoutManager.FindCarManager(_currentPlayer);
                    var previousCase = BoardEngine.Instance.GetCase(fullPath[fullPath.Count - 2]);
                    carManager.ReturnCar(previousCase.transform.position);
                    history.gear = 0;
                }
                if (_currentPlayer.lap == ContextEngine.Instance.gameContext.totalLap)
                {
                    _currentPlayer.state = PlayerStateType.Finish;
                    _currentPlayer.position = ContextEngine.Instance.gameContext.players.Where(p => p.state == PlayerStateType.Finish).Count();
                }
                else
                {
                    _currentPlayer.state = PlayerStateType.Waiting;
                }
                _currentPlayer.turnHistories.Add(_currentPlayer.currentTurn);
                _currentPlayer.currentTurn = null;
                nextPlayer = this.FindNextPlayer(_currentPlayer);
                ContextEngine.Instance.gameContext.lastTurn = DateTime.Now;
            }

            var playablePlayer = ContextEngine.Instance.gameContext.players.Count(p => p.IsPlayable());
            if (playablePlayer > 1)
            {
                if (nextPlayer == null)
                {
                    ContextEngine.Instance.gameContext.players = this.OrderedPlayerTurn();
                    var test = ContextEngine.Instance.gameContext.players.Count(p => p.IsPlayable());
                    if (ContextEngine.Instance.gameContext.players.Count(p => p.IsPlayable()) > 1)
                    {
                        ContextEngine.Instance.gameContext.turn++;
                        nextPlayer = ContextEngine.Instance.gameContext.players.FirstOrDefault(p => p.IsPlayable());
                    }
                }

                nextPlayer.currentTurn = new HistoryContext();
                nextPlayer.state = PlayerStateType.RollDice;
                _currentPlayer = nextPlayer;
                this.SelectedPlayerView(_currentPlayer);
            }
            else
            {
                RaceEngine.Instance.OnEndGame();
            }
        }

        private List<PlayerContext> OrderedPlayerTurn()
        {
            IEnumerable<PlayerContext> result = ContextEngine.Instance.gameContext.players;
            //TODO gerer la règle des arrets au stande
            result = result
                .OrderBy(p => p.position)
                .ThenByDescending(p => p.lap)
                .ThenByDescending(p => BoardEngine.Instance.GetCase(p.GetLastIndex()).itemDataSource.order)
                .ThenByDescending(p => p.turnHistories.Last().gear)
                .ThenBy(p => BoardEngine.Instance.IsBestColumnTurn(p.GetLastIndex()));
            return result.ToList();
        }

        private PlayerContext FindNextPlayer(PlayerContext player)
        {
            PlayerContext result = null;
            int currentIndex = int.MaxValue;
            for (int i = 0; i < ContextEngine.Instance.gameContext.players.Count; i++)
            {
                var current = ContextEngine.Instance.gameContext.players[i];
                if (current == player)
                {
                    currentIndex = i;
                }
                else if (current.IsPlayable() && currentIndex < i)
                {
                    result = current;
                    break;
                }
            }
            return result;
        }

        private void UpdateFeature(PlayerContext player)
        {
            var currentCase = BoardEngine.Instance.GetCase(player.GetLastIndex());
            var currentStop = 0;
            var maxStop = 0;
            if (currentCase.bendDataSource != null)
            {
                maxStop = currentCase.bendDataSource.stop;
                if (currentCase.bendDataSource.name == player.lastBend)
                {
                    currentStop = player.stopBend;
                }
            }
            featurePanelManager.UpdateFeature(player.features, currentStop, maxStop);
        }

        private bool CheckAspiration(List<CaseManager> route)
        {
            bool result = false;

            var target = route.Last();
            var nbCase = route.Count - 1;
            var currentGear = _currentPlayer.currentTurn.gear;
            if (_currentPlayer.currentTurn.de == nbCase && currentGear >= 4)
            {
                var nextCase = BoardEngine.Instance.GetNextCase(target);
                if (nextCase.hasPlayer)
                {
                    var nextPlayer = ContextEngine.Instance.gameContext.players.FirstOrDefault(p => p.name != _currentPlayer.name && p.GetLastIndex().Equals(nextCase.itemDataSource.index));
                    var nextGear = nextPlayer.turnHistories.Last().gear;
                    if (nextGear >= 4 && currentGear >= nextGear)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public PlayerContext FindPlayer(IndexDataSource index)
        {
            return ContextEngine.Instance.gameContext.players.FirstOrDefault(p => p.GetLastIndex().Equals(index));
        }

        public List<PlayerContext> FindBrokenCandidate()
        {
            List<PlayerContext> result = new List<PlayerContext>();
            if (_currentPlayer != null && _currentPlayer.currentTurn.gear >= 5)
            {
                result.Add(_currentPlayer);
            }
            return ContextEngine.Instance.gameContext.players.Where(p => _currentPlayer.name != p.name && p.turnHistories.Last().gear >= 5).ToList();
        }

        public void PlayerDead(PlayerContext player)
        {
            if (player.state == PlayerStateType.Dead)
            {
                var carManager = carLayoutManager.FindCarManager(player);
                carManager.Dead();
                var target = BoardEngine.Instance.GetCase(player.GetLastIndex());
                RaceEngine.Instance.AddDangerousCase(target);
            }
        }
    }
}