// Created by LunarEclipse on 2024-7-30 6:43.

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
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Luna.UI
{
    [RequireComponent(typeof(ScrollRect), typeof(Mask), typeof(Image))]
    public abstract class ListView<T, U> : BaseListView where T : ListViewCell<U>
    {
        private List<U> data = new();
        
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
        
        
        private Func<int, U> _itemBuilder;
        public Func<int, U> ItemBuilder
        {
            get => _itemBuilder;
            set
            {
                _itemBuilder = value;

                if (!isDirty)
                {
                    if (_initialized) ReloadAsync();
                    isDirty = true;
                }
            }
        }
        
        [HideInInspector] public int itemCount => Data.Count;


        public int FirstVisibleIndex { get; private set; }
        public int LastVisibleIndex { get; private set; }


        /// The cell prefab to be instantiated for each item in the list view.
        public T cellPrefab;

        public bool snapToCellWhenSelected = true;
        public bool alwaysSelectFirstCell = true;
        public bool selectFirstCellOnReload = true;
        public bool keepSelectionOnReload = true;

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

        protected Mask _mask;
        protected Image _maskImage;

        private bool _initialized;
        private bool _selected;
        
        public U SelectedData => Data[SelectedIndex];
        public T SelectedCell => cells[SelectedIndex];
        
        public bool Initialized => _initialized;
        

        protected virtual void Init()
        {
        }

        protected virtual void Activate()
        {
        }

        protected virtual void Deactivate()
        {
        }

        protected virtual void OnCellCreated(int index, T cell) {}
        protected virtual void OnCellLoaded(int index, T cell) {}
        protected virtual void OnCellSubmitted(int index, T cell) {}
        protected virtual void OnCellDeselected(int index, T cell) {}
        protected virtual void OnCellSelected(int index, T cell) {}
        protected virtual void OnCellClicked(int index, T cell) {}
        protected virtual void OnCellScrolling(int index, T cell, float delta) {}


        // You should call base.Awake() in the derived class
        // or override this method to customize the initialization of the list view
        protected virtual void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            onCellCreated += OnCellCreated;

            _mask = GetComponent<Mask>();
            // _mask.showMaskGraphic = false;
            _maskImage = _mask.GetComponent<Image>();

            cellPrefab.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

            if (_scrollRect.content == null)
            {
                var content = new GameObject("Content");
                var rectTransform = content.AddComponent<RectTransform>();
                rectTransform.SetParent(transform, false);
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0.5f, 1);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
                _scrollRect.content = rectTransform;

                // var layout = content.AddComponent<VerticalLayoutGroup>();
                // layout.childControlHeight = false;
                // layout.childControlWidth = false;
                // layout.childForceExpandWidth = true;

                // var fitter = content.AddComponent<ContentSizeFitter>();
                // fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                var rectTransform = _scrollRect.content.GetComponent<RectTransform>();
                rectTransform.SetParent(transform, false);
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0.5f, 1);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
                
                var contentSizeFitter = _scrollRect.content.GetComponent<ContentSizeFitter>();
                if (contentSizeFitter != null) contentSizeFitter.enabled = false;
            }
        }
        
        // protected void OnEnable()
        // {
        //     if (alwaysSelectFirstCell && cells.Count > 0)
        //     {
        //         // Select first cell
        //         var firstCell = cells.First();
        //         if (firstCell != null)
        //         {
        //             // await UniTask.NextFrame();
        //             firstCell.Select();
        //         }
        //     }
        // }

        protected virtual async void Start()
        {
            if (Data != null && Data.Count > 0)
            {
                // itemCount = Data.Count;
                _itemBuilder ??= i => Data[i.Mod(itemCount)];
            }
            
            Initialize();
            isDirty = false;
            _initialized = true;
        }
        
        public void Initialize()
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

            _scrollRect.content.sizeDelta = new Vector2(0, 50000000f); // Precision issue

            // Create new cells
            CreateCells();

            isDirty = false;
            _initialized = true;

            // Keep selection
            if (keepSelectionOnReload && data.Count > 0)
            {
                if (SelectedIndex < cells.Count)
                    cells[SelectedIndex].Select();
                else cells.Last().Select();

                SelectedIndex = Mathf.Clamp(SelectedIndex, 0, cells.Count - 1);
            }
        }

        public void Reload()
        {
            // itemCount = Data.Count;
            UpdateVisibleItems();
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

            // Calculate the number of cells to create based on the size of cell prefab and the size of the content
            var cellCount = Mathf.Max(3, Mathf.CeilToInt(this.GetComponent<RectTransform>().rect.height /
                                            cellPrefab.GetComponent<RectTransform>().rect.height) + 1);
            var cellHeight = cellPrefab.GetComponent<RectTransform>().rect.height;

            for (int i = 0; i < cellCount; i++)
            {
                var newCell = Instantiate(cellPrefab, content, false);
                cells.Add(newCell);
                newCell.gameObject.SetActive(true);
                newCell.name = "Cell " + i;
                newCell.Index = i;
                newCell.Data = ItemBuilder(i);
                newCell.RectTransform.anchorMin = new Vector2(newCell.RectTransform.anchorMin.x, 1);
                newCell.RectTransform.anchorMax = new Vector2(newCell.RectTransform.anchorMax.x, 1);
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

                // Set cell position
                newCell.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i * cellHeight);

                onCellCreated?.Invoke(i, newCell);

                if (selectFirstCellOnReload && i == 0)
                    EventSystem.current.SetSelectedGameObject(newCell.gameObject);
            }
        }
        
        private T GetReuseableCellAt(int index) 
        {
            if (index < 0 || index >= cells.Count)
                return null;

            var cell = cells[index.Mod(cells.Count)];
            if (cell.Index != index)
            {
                cell.Index = index;
                cell.Data = ItemBuilder(index);
                onCellLoaded?.Invoke(index, cell);
            }

            return cell;
        }
        
        private Vector2 GetScrollPositionAt(int index)
        {
            var cellHeight = cellPrefab.GetComponent<RectTransform>().rect.height;
            var scrollY = index * cellHeight - ((RectTransform)_scrollRect.transform).rect.height;
            if (scrollY < 0) scrollY = 0;
            return new Vector2(0, scrollY);
        }
        
        private void ScrollTo(int index, float animDuration = 0f)
        {
            var targetPosition = GetScrollPositionAt(index);
            if (_scrollRect.content.anchoredPosition == targetPosition)
                return;

            if (animDuration <= 0)
                _scrollRect.content.anchoredPosition = targetPosition;
            else DOTween.To(() => ScrollOffset, v => ScrollOffset = v, targetPosition, animDuration);
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
            FirstVisibleIndex = Mathf.Clamp(Mathf.FloorToInt(scrollY / cellHeight), 0,itemCount - 1);
            LastVisibleIndex = Mathf.Clamp(Mathf.RoundToInt((scrollY + scrollHeight) / cellHeight), 0, itemCount - 1);

            // Debug.Log($"{scrollY} First: {FirstVisibleIndex}, Last: {LastVisibleIndex}");

            for (int i = FirstVisibleIndex; i <= LastVisibleIndex; i++)
            {
                var cell = cells[i.Mod(cells.Count)];
                cell.Index = i;
                cell.Data = ItemBuilder(i);
                cell.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i * cellHeight);
                onCellLoaded?.Invoke(i, cell);
                OnCellScrolling(i, cell, scrollY + (i - FirstVisibleIndex) * cellHeight);
            }
        }
        

        // public override void SnapTo(RectTransform target)
        // {
        //     
        // }
        
        public void Select(int index)
        {
            FocusOnCell(index);
        }

        public async void FocusOnCell(int index)
        {
            if (cells.Count == 0) return;

            ScrollTo(index);
            UpdateVisibleItems();
            // await UniTask.NextFrame();
            var cell = cells[index.Mod(cells.Count)];
            cell.OnSelect(null);
            EventSystem.current.SetSelectedGameObject(cell.gameObject);
        }

        public void Remove(int index)
        {
            if (index < 0 || index >= Data.Count) return;

            Data.RemoveAt(index);
            
            var focusIndex = index < Data.Count ? index : Data.Count - 1;
            FocusOnCell(focusIndex);
        }

        private void _OnCellSubmitted(int index, ListViewCell<U> listViewCell)
        {
            OnCellSubmitted(index, listViewCell as T);
            onCellSubmitted?.Invoke(index, listViewCell as T);
        }

        private void _OnCellDeselected(int index, ListViewCell<U> listViewCell)
        {
            OnCellDeselected(index, listViewCell as T);
            onCellDeselected?.Invoke(index, listViewCell as T);
        }

        private void _OnCellSelected(int index, ListViewCell<U> listViewCell)
        {
            SelectedIndex = index;
            OnCellSelected(index, listViewCell as T);
            onCellSelected?.Invoke(index, listViewCell as T);
        }

        private void _OnCellClicked(int index, ListViewCell<U> listViewCell)
        {
            OnCellClicked(index, listViewCell as T);
            onCellClicked?.Invoke(index, listViewCell as T);
        }
    }
}


[RequireComponent(typeof(RectTransform))]
public abstract class ListViewCell<T> : Selectable, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    public delegate void CellCallback(int index, ListViewCell<T> cell);
    
    
    protected T data;
    public abstract T Data { get; set; }


    public int Index { get; set; }

    public event Action<int, ListViewCell<T>> OnCellSelected;
    public event Action<int, ListViewCell<T>> OnCellDeselected;
    public event Action<int, ListViewCell<T>> OnCellSubmitted;
    public event Action<int, ListViewCell<T>> OnCellClicked;

    public RectTransform RectTransform => GetComponent<RectTransform>();
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