// Created by LunarEclipse on 2024-7-30 6:43.

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luna.Core.Pool;
using Luna.Extensions;
using Luna.Extensions.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luna.UI
{
    public abstract class InfiniteListView<T, U> : Widget where T : InfiniteListViewCell<U>
    {
         [SerializeField] 
         private List<U> data;

        public List<U> Data
        {
            get => data;
            set
            {
                data = value;
                isDirty = true;
                foreach (var cell in cells)
                    cell.Data = value[cell.Index.Mod(value.Count)];
            }
        }
        
        public int SelectedIndex { get; private set; }
        public U SelectedData => Data[SelectedIndex];
        

        /// The cell prefab to be instantiated for each item in the list view.
        public T cellPrefab;

        public bool snapToCellWhenSelected = true;
        public bool selectFirstCellOnEnable = true;
        public bool selectFirstCellOnReload = true;

        public delegate void CellCallback(int index, T listViewCell);


        #region Events

        public event CellCallback onCellCreated;
        public event CellCallback onCellLoaded;
        public event CellCallback onCellSelected;
        public event CellCallback onCellDeselected;
        public event CellCallback onCellSubmitted;
        public event CellCallback onCellClicked;

        #endregion


        public readonly List<T> cells = new();

        protected ScrollRect _scrollRect;
        
        protected virtual void Init() {}
        protected virtual void Activate() {}
        protected virtual void Deactivate() {}
        
        protected virtual void OnCellCreated(int index, T listViewCell) {}
        protected virtual void OnCellLoaded(int index, T listViewCell) {}
        protected virtual void OnCellSubmitted(int index, T listViewCell) {}
        protected virtual void OnCellDeselected(int index, T listViewCell) {}
        protected virtual void OnCellSelected(int index, T listViewCell) {}
        protected virtual void OnCellClicked(int index, T listViewCell) {}
        
        
        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            onCellCreated += OnCellCreated;
            CreateCells();
            Init();
        }

        private async void OnEnable()
        {
            Reload();
            isDirty = false;

            if (selectFirstCellOnEnable && cells.Count > 0)
            {
                // Select first cell
                var firstCell = cells.First().gameObject;
                if (firstCell != null)
                {
                    await UniTask.NextFrame();
                    EventSystem.current.SetSelectedGameObject(firstCell);
                }
            }
            
            Activate();
        }
        
        private void OnDisable()
        {
            Deactivate();
        }

        protected void Update()
        {
            if (isDirty)
            {
                // Reload();
                isDirty = false;
            }
        }
        
        
        private void OnScrollValueChanged(Vector2 normalizedPosition)
        {
            UpdateVisibleItems();
        }

        private void UpdateVisibleItems()
        {
            var content = _scrollRect.content;
            var cellHeight = cellPrefab.GetComponent<RectTransform>().rect.height;
            var scrollHeight = _scrollRect.GetComponent<RectTransform>().rect.height;
            var scrollY = content.anchoredPosition.y;
            var firstVisibleIndex = Mathf.FloorToInt(scrollY / cellHeight);
            var lastVisibleIndex = Mathf.CeilToInt((scrollY + scrollHeight) / cellHeight);

            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var cellY = cell.Index * cellHeight;
                var isVisible = cellY >= scrollY && cellY <= scrollY + scrollHeight;
                cell.gameObject.SetActive(isVisible);
                if (isVisible)
                {
                    onCellLoaded?.Invoke(cell.Index, cell);
                }
            }
        }


        public void Reload()
        {
            // Clear existing cells
            foreach (var cell in cells)
            {
                cell.OnCellSelected -= _OnCellSelected;
                cell.OnCellDeselected -= _OnCellDeselected;
                cell.OnCellSubmitted -= _OnCellSubmitted;
                Destroy(cell.gameObject);
            }
        }
        
        public void SnapTo(RectTransform target)
        {
            var y = -target.offsetMin.y - ((RectTransform)_scrollRect.transform).rect.height;
            y = Mathf.Clamp(y, 0, _scrollRect.content.rect.height);
            var pos = new Vector2(_scrollRect.content.anchoredPosition.x, y);
            DOTween.To(() => _scrollRect.content.anchoredPosition, v => _scrollRect.content.anchoredPosition = v, pos,
                0.5f);
        }

        public void FocusOnCell(int index)
        {
            if (cells.Count == 0) return;

            var cell = cells[index.Mod(cells.Count)];
            cell.OnSelect(null);
            EventSystem.current.SetSelectedGameObject(cell.gameObject);
        }

        private void CreateCells()
        {
            var content = _scrollRect.content;
            
            // Calculate the number of cells to create based on the size of cell prefab and the size of the content
            var cellCount = Mathf.CeilToInt(content.rect.height / cellPrefab.GetComponent<RectTransform>().rect.height) + 1;
            
            for (int i = 0; i < cellCount; i++)
            {
                var newCell = ObjectPool.Get(cellPrefab);
                cells.Add(newCell);
                newCell.transform.SetParent(content);
                newCell.gameObject.SetActive(true);
                newCell.Index = i;
                newCell.OnCellSelected += _OnCellSelected;
                newCell.OnCellDeselected += _OnCellDeselected;
                newCell.OnCellSubmitted += _OnCellSubmitted;
                newCell.OnCellClicked += _OnCellClicked;
                if (snapToCellWhenSelected)
                {
                    newCell.OnCellSelected += (index, listViewCell) =>
                    {
                        SnapTo(listViewCell.transform as RectTransform);
                    };
                }

                onCellCreated?.Invoke(i, newCell);

                if (selectFirstCellOnReload && i == 0)
                    EventSystem.current.SetSelectedGameObject(newCell.gameObject);
            }
        }

        private void _OnCellSubmitted(int index, InfiniteListViewCell<U> listViewCell)
        {
            OnCellSubmitted(index, listViewCell as T);
            onCellSubmitted?.Invoke(index, listViewCell as T);
        }

        private void _OnCellDeselected(int index, InfiniteListViewCell<U> listViewCell)
        {
            OnCellDeselected(index, listViewCell as T);
            onCellDeselected?.Invoke(index, listViewCell as T);
        }

        private void _OnCellSelected(int index, InfiniteListViewCell<U> listViewCell)
        {
            SelectedIndex = index;
            OnCellSelected(index, listViewCell as T);
            onCellSelected?.Invoke(index, listViewCell as T);
        }

        private void _OnCellClicked(int index, InfiniteListViewCell<U> listViewCell)
        {
            OnCellClicked(index, listViewCell as T);
            onCellClicked?.Invoke(index, listViewCell as T);
        }
    }
}


public abstract class InfiniteListViewCell<T> : Selectable, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    protected T data;
    public abstract T Data { get; set; }


    public int Index { get; set; }

    public event Action<int, InfiniteListViewCell<T>> OnCellSelected;
    public event Action<int, InfiniteListViewCell<T>> OnCellDeselected;
    public event Action<int, InfiniteListViewCell<T>> OnCellSubmitted;
    public event Action<int, InfiniteListViewCell<T>> OnCellClicked;

    private Image _image;

    protected override void Awake()
    {
        _image = GetComponent<Image>();
        if (_image == null)
        {
            _image = gameObject.AddComponent<Image>();
            this.targetGraphic = _image;
            this.colors = new ColorBlock().ClearColor();
        }
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        OnCellSelected?.Invoke(Index, this);
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        OnCellDeselected?.Invoke(Index, this);
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
        OnCellSubmitted?.Invoke(Index, this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCellClicked?.Invoke(Index, this);
    }

    public void Focus()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
