using UnityEngine;
using System.Collections;
using FormuleD.Models.Board;
using System;
using System.Linq;
using System.Collections.Generic;
using FormuleD.Models.Contexts;
using FormuleD.Engines;

namespace FormuleD.Managers.Course.Board
{
    public class BoardManager : MonoBehaviour
    {
        public Transform linePrefab;
        public Transform casePrefab;
        public Transform bendPrefab;
        public Transform finishPrefab;

        public BoardDataSource boardDataSource;

        public List<IndexDataSource> firstIndex;
        private Dictionary<IndexDataSource, BoardItem> _boardItems;

        public void InitBoard(BoardDataSource newBoardDataSource)
        {
            this.DeleteBoard();
            boardDataSource = newBoardDataSource;
            this.CreateBoard();
        }

        public CaseManager FindCaseManager(IndexDataSource index)
        {
            CaseManager result = null;
            if (index != null && _boardItems != null && _boardItems.ContainsKey(index))
            {
                result = _boardItems[index].caseManager;
            }
            return result;
        }

        public LineManager FindLineManager(CaseManager from, CaseManager to)
        {
            LineManager result = null;
            if (from != null && to != null)
            {
                if (_boardItems != null && _boardItems.ContainsKey(from.itemDataSource.index))
                {
                    var item = _boardItems[from.itemDataSource.index];
                    result = item.lines.FirstOrDefault(l => l.target.itemDataSource.index.Equals(to.itemDataSource.index));
                }
            }
            return result;
        }

        public List<CaseManager> GetStartCase()
        {
            List<CaseManager> result = new List<CaseManager>();
            if (boardDataSource != null)
            {
                foreach (var startIndex in boardDataSource.starts)
                {
                    var caseManager = this.FindCaseManager(startIndex);
                    if (caseManager != null)
                    {
                        result.Add(caseManager);
                    }
                }
            }
            return result;
        }

        private void CreateBoard()
        {
            if (boardDataSource != null)
            {
                if (_boardItems == null)
                {
                    _boardItems = new Dictionary<IndexDataSource, BoardItem>();
                }
                var offset = this.ComptureOffset();
                var dangerousCases = ContextEngine.Instance.gameContext.dangerousCases;
                foreach (var caseModel in boardDataSource.cases)
                {
                    var caseManager = this.CreateCaseManager(caseModel, offset);
                    if (caseManager != null)
                    {
                        var bendModel = boardDataSource.bends.FirstOrDefault(t => t.Targets.Any(ta => ta.Equals(caseModel.index)));
                        var standModel = boardDataSource.stands.FirstOrDefault(s => s.target.Equals(caseModel.index));
                        caseManager.InitCase(caseModel, bendModel, standModel);
                        if (dangerousCases.Contains(caseModel.index))
                        {
                            caseManager.SetDangerous(true);
                        }
                        _boardItems.Add(caseModel.index, new BoardItem()
                        {
                            caseManager = caseManager
                        });
                    }
                }

                foreach (var item in _boardItems)
                {
                    if (item.Value.caseManager != null)
                    {
                        foreach (var targetModel in item.Value.caseManager.itemDataSource.targets.Where(t => t.enable))
                        {
                            if (_boardItems.ContainsKey(targetModel))
                            {
                                var targetMapItem = _boardItems[targetModel];
                                var lineManager = this.CreateLineManager(item.Value.caseManager, targetMapItem.caseManager);
                                if (lineManager != null)
                                {
                                    lineManager.InitLine(item.Value.caseManager, targetMapItem.caseManager);
                                    item.Value.lines.Add(lineManager);
                                }
                            }
                        }
                    }
                }

                foreach (var turnModel in boardDataSource.bends)
                {
                    var turnManager = this.CreateTurnManager(turnModel, offset);
                    turnManager.InitTurn(turnModel);
                }

                var finishManager = this.CreateFinishManager();
                var firstCases = boardDataSource.cases.Where(c => !boardDataSource.stands.Any(s => s.target.Equals(c.index))).GroupBy(k => k.index.column, v => v).OrderBy(l => l.Key).Select(l => l.OrderBy(c => c.order).Select(c => _boardItems[c.index].caseManager).First()).ToArray();
                firstIndex = firstCases.Select(f => f.itemDataSource.index).ToList();
                finishManager.InitFinish(firstCases);
            }
        }

        private FinishManager CreateFinishManager()
        {
            var finishTransform = Instantiate(finishPrefab);
            finishTransform.SetParent(this.transform);
            return finishTransform.GetComponent<FinishManager>();
        }

        private BendManager CreateTurnManager(BendDataSource turnModel, Vector2 offset)
        {
            var turnTransform = Instantiate(bendPrefab);
            turnTransform.SetParent(this.transform);
            turnTransform.localPosition = new Vector3((turnModel.icon.x - offset.x) / boardDataSource.coef, ((-1 * turnModel.icon.y) + offset.y) / boardDataSource.coef, 0);
            return turnTransform.GetComponent<BendManager>();
        }

        private LineManager CreateLineManager(CaseManager sourceCaseManager, CaseManager targetCaseManager)
        {
            var lineTransform = Instantiate(linePrefab);
            lineTransform.SetParent(this.transform);
            lineTransform.localPosition = new Vector3(sourceCaseManager.transform.position.x, sourceCaseManager.transform.position.y, 0.4f);
            return lineTransform.GetComponent<LineManager>();
        }

        private CaseManager CreateCaseManager(BoardItemDataSource item, Vector2 offset)
        {
            var caseTransform = Instantiate(casePrefab);
            caseTransform.SetParent(this.transform);
            caseTransform.localPosition = new Vector3((item.position.x - offset.x) / boardDataSource.coef, ((-1 * item.position.y) + offset.y) / boardDataSource.coef, 0);
            return caseTransform.GetComponent<CaseManager>();
        }

        private Vector2 ComptureOffset()
        {
            Vector2 result = Vector2.zero;

            var minX = boardDataSource.cases.Min(c => c.position.x);
            var maxX = boardDataSource.cases.Max(c => c.position.x);
            result.x = minX + (maxX - minX) / 2;

            var minY = boardDataSource.cases.Min(c => c.position.y);
            var maxY = boardDataSource.cases.Max(c => c.position.y);
            result.y = minY + (maxY - minY) / 2;

            return result;
        }

        private void DeleteBoard()
        {
            if (_boardItems != null && _boardItems.Any())
            {
                foreach (var item in _boardItems)
                {
                    foreach (var line in item.Value.lines)
                    {
                        Destroy(line);
                    }
                    Destroy(item.Value.caseManager);
                }
                _boardItems = null;
            }
        }

        public BendDataSource GetNextTurn(IndexDataSource indexDataSource)
        {
            BendDataSource result = null;
            var currentCase = this.FindCaseManager(indexDataSource);
            while (result == null)
            {
                if (currentCase.bendDataSource != null)
                {
                    result = currentCase.bendDataSource;
                }
                else
                {
                    var nextIndex = currentCase.itemDataSource.targets.FirstOrDefault(i => i.enable && i.column == indexDataSource.column);
                    if (nextIndex == null)
                    {
                        nextIndex = currentCase.itemDataSource.targets.FirstOrDefault();
                    }
                    currentCase = this.FindCaseManager(nextIndex);
                }
            }
            return result;
        }

        public Bounds GetBounds()
        {
            Bounds result = new Bounds();
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            foreach (var child in this.GetComponentsInChildren<CaseManager>())
            {
                if (child.transform.position.x > max.x)
                {
                    max = new Vector3(child.transform.position.x, max.y, 0);
                }
                if (child.transform.position.x < min.x)
                {
                    min = new Vector3(child.transform.position.x, min.y, 0);
                }
                if (child.transform.position.y > max.y)
                {
                    max = new Vector3(max.x, child.transform.position.y, 0);
                }
                if (child.transform.position.y < min.y)
                {
                    min = new Vector3(min.x, child.transform.position.y, 0);
                }
                
            }
            result.SetMinMax(min, max);
            return result;
        }
    }

    [Serializable]
    public class BoardItem
    {
        public BoardItem()
        {
            lines = new List<LineManager>();
        }

        public CaseManager caseManager;
        public List<LineManager> lines;
    }
}