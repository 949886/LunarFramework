// Created by LunarEclipse on 2024-7-5 18:37.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luna.Extensions;
using Luna.Extensions.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luna.UI
{
    public abstract class FixedListView<T, U> : BaseListView where T : FixedListViewCell<U>
    {
        [SerializeField] 
        protected List<U> data = new();
        
        public List<U> Data
        {
            get => data;
            set
            {
                data = value;

                if (!isDirty)
                {
                    if (_initialized) ReloadAsync();
                    isDirty = true;
                }
                
            }
        }
        
        /// The cell prefab to be instantiated for each item in the list view.
        public T cell;
        

        public bool selectFirstCellOnEnable = false;
        public bool selectFirstCellOnReload = true;
        public bool keepSelectionOnReload = true;
        public bool snapToCellWhenSelected = true;
        
        public delegate void CellCallback(int index, T listViewCell);


        #region Events
        
        public event CellCallback onCellCreated;
        public event CellCallback onCellSelected;
        public event CellCallback onCellDeselected;
        public event CellCallback onCellSubmitted;
        public event CellCallback onCellClicked;
        
        #endregion
        
        
        public readonly List<T> cells = new();
        
        private bool _initialized;
        private bool _selected;
        
        public U SelectedData => Data[SelectedIndex];
        public T SelectedCell => cells[SelectedIndex];
        
        /// Return true any cell is selected.
        public bool Selected => _selected;
        
        public int Count => Data.Count;
        
        public bool Initialized => _initialized;

        public Rect CellSize => ((RectTransform)cell.transform).rect;

        public int VisibleIndex => Mathf.RoundToInt((-((RectTransform)SelectedCell.transform).anchoredPosition.y - ScrollOffset.y) / CellSize.height) - 1;

        
        // You should call base.Awake() in the derived class
        // or override this method to customize the initialization of the list view
        protected virtual void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            onCellCreated += OnCellCreated;
        }

        protected void OnEnable()
        {
            if (selectFirstCellOnEnable && cells.Count > 0)
            {
                // Select first cell
                var firstCell = cells.First().gameObject;
                if (firstCell != null)
                {
                    // await UniTask.NextFrame();
                    EventSystem.current.SetSelectedGameObject(firstCell);
                }
            }
        }

        protected virtual void Start()
        {
            Reload();
            isDirty = false;
            _initialized = true;
        }

        public void Reload()
        {
            _initialized = false;
            
            // Clear existing cells
            foreach (var cell in cells)
            {
                cell.OnCellSelected -= _OnCellSelected;
                cell.OnCellDeselected -= _OnCellDeselected;
                cell.OnCellSubmitted -= _OnCellSubmitted;
                Destroy(cell.gameObject);
            }
            cells.Clear();
            
            // Create new cells
            CreateCells();
            
            isDirty = false;
            _initialized = true;

            KeepSelection();
        }
        
        public async Task ReloadAsync()
        {
            _initialized = false;
            await UniTask.Yield(PlayerLoopTiming.PreUpdate);
            Reload();
        }
        
        private void CreateCells()
        {
            var content = _scrollRect.content;
            for (int i = 0; i < Data.Count; i++)
            {
                var newCell = Instantiate(cell, content);
                cells.Add(newCell);
                newCell.gameObject.SetActive(true);
                newCell.name = $"Cell {i}";
                newCell.Index = i;
                newCell.Data = Data[i];
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
        
        public async void KeepSelection()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            // Keep selection
            if (keepSelectionOnReload && cells.Count > 0)
            {
                if (SelectedIndex < cells.Count)
                    Select(SelectedIndex);
                
                SelectedIndex = Mathf.Clamp(SelectedIndex, 0, cells.Count - 1);
            }
        }

        protected virtual void OnCellCreated(int index, T listViewCell) {}
        protected virtual void OnCellSubmitted(int index, T listViewCell) {}
        protected virtual void OnCellDeselected(int index, T listViewCell) {}
        protected virtual void OnCellSelected(int index, T listViewCell) {}
        protected virtual void OnCellClicked(int index, T listViewCell) {}

        private void _OnCellSubmitted(int index, FixedListViewCell<U> listViewCell)
        {
            OnCellSubmitted(index, listViewCell as T);
            onCellSubmitted?.Invoke(index, listViewCell as T);
        }

        private void _OnCellDeselected(int index, FixedListViewCell<U> listViewCell)
        {
            _selected = false;
            OnCellDeselected(index, listViewCell as T);
            onCellDeselected?.Invoke(index, listViewCell as T);
        }

        private void _OnCellSelected(int index, FixedListViewCell<U> listViewCell)
        {
            _selected = true;
            PreviousIndex = SelectedIndex;
            SelectedIndex = index;
            OnCellSelected(index, listViewCell as T);
            onCellSelected?.Invoke(index, listViewCell as T);
        }
        
        private void _OnCellClicked(int index, FixedListViewCell<U> listViewCell)
        {
            OnCellClicked(index, listViewCell as T);
            onCellClicked?.Invoke(index, listViewCell as T);
        }
        
        private void OnScrollValueChanged(Vector2 normalizedPosition)
        {
            // UpdateVisibleItems();
        }
        
        public void Select(int index)
        {
            if (index >= cells.Count) 
                index = cells.Count - 1;
            
            FocusOnCell(index, 0);
        }
        
        public void Remove(int index, bool autoFocus = true)
        {
            if (index < cells.Count && index >= 0)
            {
                Data.RemoveAt(index);
                Reload();

                if (cells.Count == 0) return;
                var focusIndex = index < cells.Count ? index : cells.Count - 1;
                SelectedIndex = focusIndex;
                
                if (autoFocus)
                    UniTask.NextFrame().ContinueWith(() => {
                        FocusOnCell(focusIndex, false);
                    });
            }
        }
        
        public void RemoveSelected()
        {
            Remove(SelectedIndex);
        }
        
        public void Add(U item, int atIndex)
        {
            Data.Insert(atIndex, item);
            Reload();
        }
        
        public void FocusOnCell(int index, bool autoSnap = false, float duration = 0.5f)
        {
            if (cells.Count == 0) return;
            
            SelectedIndex = index;
            
            var currentSelected = EventSystem.current.currentSelectedGameObject;
            var cell = cells[index.Mod(cells.Count)];
            if (autoSnap) SnapTo(cell.transform as RectTransform, duration);
            if (!isDirty) EventSystem.current.SetSelectedGameObject(cell.gameObject);
        }
        
        public void FocusOnCell(int index, float duration)
        {
            FocusOnCell(index, true, duration);
        }

        public void FocusOnCell(T cell, bool autoSnap = false)
        {
            var index = cells.IndexOf(cell);
            FocusOnCell(index, autoSnap);
        }
    }
    
}



public abstract class FixedListViewCell<T> : Selectable, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    protected T data;
    public abstract T Data { get; set; }
    
    
    public int Index { get; set; }
        
    public event Action<int, FixedListViewCell<T>> OnCellSelected;
    public event Action<int, FixedListViewCell<T>> OnCellDeselected;
    public event Action<int, FixedListViewCell<T>> OnCellSubmitted;
    public event Action<int, FixedListViewCell<T>> OnCellClicked; 

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
    
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        OnCellClicked?.Invoke(Index, this);
    }
    
    public void Focus()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
