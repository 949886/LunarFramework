// Created by LunarEclipse on 2024-7-30 6:43.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Luna.Extensions;
using Luna.Extensions.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luna.UI
{
    [RequireComponent(typeof(ScrollRect), typeof(Mask), typeof(Image))]
    public abstract class ListView<T, U> : Widget where T : ListViewCell<U>
    {
        private List<U> data;
        
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
        
        public int itemCount;


        public int SelectedIndex { get; private set; }
        public int FirstVisibleIndex { get; private set; }
        public int LastVisibleIndex { get; private set; }


        /// The cell prefab to be instantiated for each item in the list view.
        public T cellPrefab;

        public bool snapToCellWhenSelected = true;
        public bool selectFirstCellOnEnable = true;
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

        protected ScrollRect _scrollRect;
        protected Mask _mask;
        protected Image _maskImage;

        private bool _initialized;
        private bool _selected;

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
            _mask.showMaskGraphic = false;

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
        }

        protected virtual async void Start()
        {
            if (Data != null)
            {
                itemCount = Data.Count;
                _itemBuilder ??= i => Data[i.Mod(itemCount)];
            }
            
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

            _initialized = true;
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

            cells.Clear();

            _scrollRect.content.sizeDelta =
                new Vector2(0, MathF.Min(itemCount * cellPrefab.GetComponent<RectTransform>().rect.height, 50000000f)); // Precision issue

            // Create new cells
            CreateCells();

            // Keep selection
            if (_initialized && keepSelectionOnReload && cells.Count > 0)
            {
                if (SelectedIndex < cells.Count)
                    cells[SelectedIndex].Select();
                else cells.Last().Select();

                SelectedIndex = Mathf.Clamp(SelectedIndex, 0, cells.Count - 1);
            }

            isDirty = false;
        }

        public async Task ReloadAsync()
        {
            await UniTask.Yield(PlayerLoopTiming.PreUpdate);
            Reload();
        }

        private void CreateCells()
        {
            var content = _scrollRect.content;

            // Calculate the number of cells to create based on the size of cell prefab and the size of the content
            Debug.Log($"{this.GetComponent<RectTransform>().rect.height} {cellPrefab.GetComponent<RectTransform>().rect.height}");
            var cellCount = Mathf.Max(3, Mathf.CeilToInt(this.GetComponent<RectTransform>().rect.height /
                                            cellPrefab.GetComponent<RectTransform>().rect.height) + 1);
            var cellHeight = cellPrefab.GetComponent<RectTransform>().rect.height;

            for (int i = 0; i < cellCount; i++)
            {
                var newCell = Instantiate(cellPrefab, content, false);
                cells.Add(newCell);
                newCell.gameObject.SetActive(true);
                newCell.Index = i;
                newCell.Data = ItemBuilder(i);
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
            LastVisibleIndex = Mathf.Clamp(Mathf.CeilToInt((scrollY + scrollHeight) / cellHeight), 0, itemCount - 1);

            // Debug.Log($"{scrollY} First: {firstVisibleIndex}, Last: {lastVisibleIndex}");

            for (int i = FirstVisibleIndex; i <= LastVisibleIndex; i++)
            {
                var cell = cells[i.Mod(cells.Count)];
                if (cell.Index != i)
                {
                    cell.Index = i;
                    cell.Data = ItemBuilder(i);
                    cell.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i * cellHeight);
                    onCellLoaded?.Invoke(i, cell);
                }
                OnCellScrolling(i, cell, scrollY + (i - FirstVisibleIndex) * cellHeight);
            }
        }

        public void SnapTo(RectTransform target)
        {
            // var y = -target.offsetMin.y - ((RectTransform)_scrollRect.transform).rect.height;
            // y = Mathf.Clamp(y, 0, _scrollRect.content.rect.height);
            // var pos = new Vector2(_scrollRect.content.anchoredPosition.x, y);
            // DOTween.To(() => _scrollRect.content.anchoredPosition, v => _scrollRect.content.anchoredPosition = v, pos,
            //     0.5f);
        }

        public void FocusOnCell(int index)
        {
            if (cells.Count == 0) return;

            var cell = cells[index.Mod(cells.Count)];
            cell.OnSelect(null);
            EventSystem.current.SetSelectedGameObject(cell.gameObject);
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