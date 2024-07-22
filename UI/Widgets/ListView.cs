// Created by LunarEclipse on 2024-7-5 18:37.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luna.Extensions;
using Luna.UI;
using Modules.UI.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modules.UI.Widgets
{
    [RequireComponent(typeof(ScrollRect))]
    public abstract class ListView<T, U> : Widget where T : ListViewCell<U>
    {
        [SerializeField] 
        private List<U> data;
        
        public virtual List<U> Data
        {
            get => data;
            set
            {
                data = value;
                isDirty = true;
            }
        }
        
        public T cell;
        public bool snapToCellWhenSelected = true;
        public bool selectFirstCellOnEnable = true;
        public bool selectFirstCellOnReload = true;
        
        public delegate void CellCallback(int index, T listViewCell);
        
        public event CellCallback onCellCreated;
        public event CellCallback onCellSelected;
        public event CellCallback onCellDeselected;
        public event CellCallback onCellSubmitted;
        public event CellCallback onCellClicked;
        
        public readonly List<T> cells = new();
        
        protected ScrollRect _scrollRect;
        
        public int SelectedIndex { get; private set; }
        public U SelectedData => Data[SelectedIndex];

        // You should call base.Awake() in the derived class
        // or override this method to customize the initialization of the list view
        protected virtual void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            onCellCreated += OnCellCreated;
        }
        
        protected virtual async void OnEnable()
        {
            Debug.Log("ListView enabled.");
            
            Initialize();
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
        }

        protected void Update()
        {
            if (isDirty)
            {
                Initialize();
                isDirty = false;
            }
        }

        public void Initialize()
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
            
            // Create new cells
            CreateCells();
        }
        
        private void CreateCells()
        {
            var content = _scrollRect.content;
            for (int i = 0; i < Data.Count; i++)
            {
                var newCell = Instantiate(cell, content);
                cells.Add(newCell);
                newCell.gameObject.SetActive(true);
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

        protected virtual void OnCellCreated(int index, T listViewCell) {}
        protected virtual void OnCellSubmitted(int index, T listViewCell) {}
        protected virtual void OnCellDeselected(int index, T listViewCell) {}
        protected virtual void OnCellSelected(int index, T listViewCell) {}
        protected virtual void OnCellClicked(int index, T listViewCell) {}

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
        
        void OnScrollValueChanged(Vector2 normalizedPosition)
        {
            // UpdateVisibleItems();
        }
        
        public void SnapTo(RectTransform target)
        {
            var y = -target.offsetMin.y - ((RectTransform)_scrollRect.transform).rect.height;
            y = Mathf.Clamp(y, 0, _scrollRect.content.rect.height);
            var pos = new Vector2(_scrollRect.content.anchoredPosition.x, y);
            DOTween.To(() => _scrollRect.content.anchoredPosition, v => _scrollRect.content.anchoredPosition = v, pos, 0.5f);
        }
        
        public void FocusOnCell(int index)
        {
            if (cells.Count == 0) return;
            
            var cell = cells[index.Mod(cells.Count)];
            cell.OnSelect(null);
            EventSystem.current.SetSelectedGameObject(cell.gameObject);
        }
        
        // public void SnapTo(RectTransform target)
        // {
        //     var newPos =
        //         (Vector2)_scrollRect.transform.InverseTransformPoint(_scrollRect.content.position)
        //         - (Vector2)_scrollRect.transform.InverseTransformPoint(target.position);
        //     var y = Mathf.Clamp(newPos.y - 80f, 0, _scrollRect.content.sizeDelta.y);
        //     var pos = new Vector2(_scrollRect.content.anchoredPosition.x, y);
        //     Debug.Log("SnapTo: " + pos);
        //     DOTween.To(() => _scrollRect.content.anchoredPosition, v => _scrollRect.content.anchoredPosition = v, pos, 0.5f);
        // }

        // private void UpdateVisibleItems()
        // {
        //     float scrollHeight = scrollRect.viewport.rect.height;
        //     float scrollPos = scrollRect.content.anchoredPosition.y;
        //
        //     startIndex = Mathf.FloorToInt(scrollPos / itemHeight);
        //     endIndex = Mathf.CeilToInt((scrollPos + scrollHeight) / itemHeight);
        //
        //     startIndex = Mathf.Clamp(startIndex, 0, itemCount - 1);
        //     endIndex = Mathf.Clamp(endIndex, 0, itemCount - 1);
        //
        //     for (int i = 0; i < content.childCount; i++)
        //     {
        //         content.GetChild(i).gameObject.SetActive(i >= startIndex && i <= endIndex);
        //     }
        // }
        //
        // private void CalculateItemHeight()
        // {
        //     if (content.childCount > 0)
        //     {
        //         RectTransform firstItem = content.GetChild(0) as RectTransform;
        //         RectTransform lastItem = content.GetChild(content.childCount - 1) as RectTransform;
        //
        //         itemHeight = Mathf.Abs(firstItem.anchoredPosition.y - lastItem.anchoredPosition.y) / (content.childCount - 1);
        //     }
        // }
    }
    
}



public abstract class ListViewCell<T> : Selectable, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    virtual public T Data { get; set; }
    
    public int Index { get; set; }
        
    public event Action<int, ListViewCell<T>> OnCellSelected;
    public event Action<int, ListViewCell<T>> OnCellDeselected;
    public event Action<int, ListViewCell<T>> OnCellSubmitted;
    public event Action<int, ListViewCell<T>> OnCellClicked; 

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
        // base.OnSubmit(eventData);
        OnCellSubmitted?.Invoke(Index, this);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // base.OnPointerClick(eventData);
        OnCellClicked?.Invoke(Index, this);
    }
    
    public void Focus()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
