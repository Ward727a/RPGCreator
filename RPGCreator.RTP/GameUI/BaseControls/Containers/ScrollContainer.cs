// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using RPGCreator.RTP.GameUI.BaseControls.Box;
using RPGCreator.RTP.GameUI.Enums;

namespace RPGCreator.RTP.GameUI.BaseControls.Containers;

public class ScrollContainer : SingleChildrenContainerControl
{
    
    protected override string _Name { get; set; } = "ScrollContainer";
    public override bool IgnoreMouseEvents { get; set; } = false;

    public new bool ClipToBounds => true;
    
    public SimpleColorBox ScrollBarVerticalBackground { get; private set; }
    public SimpleColorBox ScrollBarVerticalThumb { get; private set; }
    
    public SimpleColorBox ScrollBarHorizontalBackground { get; private set; }
    public SimpleColorBox ScrollBarHorizontalThumb { get; private set; }
    
    public int ScrollBarMargin { get; set; } = 2;
    public int ScrollBarPadding { get; set; } = 2;
    public int ScrollBarSize { get; set; } = 10;
    public float ScrollSpeed { get; set; } = 0.1f;
    
    private float _scrollVerticalValue = 0f;
    private float _scrollVerticalOffset = 0f;
    private float _scrollHorizontalValue = 0f;
    private float _scrollHorizontalOffset = 0f;
    
    private Size _scrollbarSize => new Size(ScrollBarSize, GlobalsBounds.Height);
    private Size VerticalThumbSize => new Size(ScrollBarSize - (ScrollBarMargin * 2), GlobalsBounds.Height);
    private Size HorizontalThumbSize => new Size(GlobalsBounds.Width, ScrollBarSize - (ScrollBarMargin * 2));
    private Vector2 ThumbPosition { get; set; }
    
    public ScrollBarVisibility ScrollBarVerticalVisibility { get; set; } = ScrollBarVisibility.Auto;

    internal SingleChildrenContainerControl ContentPresenter { get; set; }
    protected BaseControl? Content { get; set; }

    public ScrollContainer(BaseControl? Content = null)
    {
        base.ClipToBounds = false;
        ScrollBarVerticalBackground = new SimpleColorBox(Color.DimGray);
        ScrollBarVerticalBackground.Size = _scrollbarSize;
        ScrollBarVerticalBackground.IgnoreMouseEvents = false;
        ScrollBarVerticalBackground.Anchors = ControlAnchors.AnchorRight | ControlAnchors.AnchorFullVertical;
        ScrollBarVerticalBackground.BackgroundColor = Color.Blue;
        ScrollBarVerticalBackground.Padding = new Thickness(ScrollBarPadding);
        ScrollBarVerticalBackground.ClipToBounds = true;
        AddInternalComponent(ScrollBarVerticalBackground);
        
        ScrollBarVerticalThumb = new SimpleColorBox(Color.Green);
        ScrollBarVerticalThumb.IgnoreMouseEvents = false;
        ScrollBarVerticalThumb.Size = VerticalThumbSize;
        ScrollBarVerticalThumb.SizingMode = ControlSizingMode.Manual; 
        ScrollBarVerticalThumb.Anchors = ControlAnchors.AnchorCenterHorizontal | ControlAnchors.AnchorTop;
        ScrollBarVerticalThumb.BackgroundColor = Color.Green;
        ScrollBarVerticalBackground.AddInternalComponent(ScrollBarVerticalThumb);
        
        ScrollBarHorizontalBackground = new SimpleColorBox(Color.DimGray);
        ScrollBarHorizontalBackground.Size = new Size(GlobalsBounds.Width, ScrollBarSize);
        ScrollBarHorizontalBackground.IgnoreMouseEvents = false;
        ScrollBarHorizontalBackground.Anchors = ControlAnchors.AnchorBottom | ControlAnchors.AnchorFullHorizontal;
        ScrollBarHorizontalBackground.BackgroundColor = Color.Blue;
        ScrollBarHorizontalBackground.Padding = new Thickness(ScrollBarPadding);
        ScrollBarHorizontalBackground.ClipToBounds = true;
        AddInternalComponent(ScrollBarHorizontalBackground);
        
        ScrollBarHorizontalThumb = new SimpleColorBox(Color.Green);
        ScrollBarHorizontalThumb.IgnoreMouseEvents = false;
        ScrollBarHorizontalThumb.Size = HorizontalThumbSize;
        ScrollBarHorizontalThumb.SizingMode = ControlSizingMode.Manual;
        ScrollBarHorizontalThumb.Anchors = ControlAnchors.AnchorCenterVertical | ControlAnchors.AnchorLeft;
        ScrollBarHorizontalThumb.BackgroundColor = Color.Green;
        ScrollBarHorizontalBackground.AddInternalComponent(ScrollBarHorizontalThumb);
        
        
        ContentPresenter = new SingleChildrenContainerControl
        {
            Name = "ContentPresenter",
            Anchors = ControlAnchors.AnchorFull,
            Padding = new Thickness(0, 0, 0, 0),
            ClipToBounds = true
        };
        AddInternalComponent(ContentPresenter);
        
        SetContent(Content);
        
        OnVerticalWheelScrolled += OnVerticalScroll;
        ScrollBarVerticalBackground.OnVerticalWheelScrolled += OnVerticalScroll;
        ScrollBarVerticalThumb.OnVerticalWheelScrolled += OnVerticalScroll;
        
        OnHorizontalWheelScrolled += OnHorizontalScroll;
        ScrollBarHorizontalBackground.OnHorizontalWheelScrolled += OnHorizontalScroll;
        ScrollBarHorizontalThumb.OnVerticalWheelScrolled += OnHorizontalScroll;
    }

    private void OnVerticalScroll(int scrollDelta)
    {
        if (IsMouseOver && scrollDelta != 0)
        {
            float delta = (scrollDelta > 0 ? -1 : 1) * ScrollSpeed;
        
            _scrollVerticalValue = MathHelper.Clamp(_scrollVerticalValue + delta, 0f, 1f);
        }
    }
    
    private void OnHorizontalScroll(int scrollDelta)
    {
        if (IsMouseOver && scrollDelta != 0)
        {
            float delta = (scrollDelta > 0 ? -1 : 1) * ScrollSpeed;
        
            _scrollHorizontalValue = MathHelper.Clamp(_scrollHorizontalValue + delta, 0f, 1f);
        }
    }

    public void SetScrollbarBackground<T>(T newBackground) where T : SimpleColorBox
    {
        if (Equals(newBackground, ScrollBarVerticalBackground))
            return;
        var old = ScrollBarVerticalBackground;
        old.OnVerticalWheelScrolled -= OnVerticalScroll;
        ScrollBarVerticalBackground = newBackground;
        ScrollBarVerticalBackground.OnVerticalWheelScrolled += OnVerticalScroll;
    }
    
    public void SetScrollbarThumb<T>(T newThumb) where T : SimpleColorBox
    {
        if (Equals(newThumb, ScrollBarVerticalThumb))
            return;
        var old = ScrollBarVerticalThumb;
        old.OnVerticalWheelScrolled -= OnVerticalScroll;
        ScrollBarVerticalThumb = newThumb;
        ScrollBarVerticalThumb.OnVerticalWheelScrolled += OnVerticalScroll;
    }
    
    public void SetContent(BaseControl? newContent)
    {
        if (Equals(newContent, Content))
            return;
        var old = Content;
        if (old != null)
        {
            old.SetParent(null);
            ContentPresenter.RemoveChildInternal(old);
        }
        Content = newContent;
        if (Content != null)
        {
            Content.SetParent(ContentPresenter);
            ContentPresenter.AddChildInternal(Content);
        }
    }
    
    public override void Update(TimeSpan deltaTime)
    {
        base.Update(deltaTime);

        if (Content == null) return;

        UpdateVerticalScroll();
        UpdateHorizontalScroll();
        
        Content.Position = new Vector2(-_scrollHorizontalOffset, -_scrollVerticalOffset);
    }

    private void UpdateVerticalScroll()
    {
        
        float viewportHeight = ContentPresenter.GlobalsBounds.Height;
        float totalContentHeight = Content.DesiredSize.Height;

        if (totalContentHeight <= viewportHeight)
        {
            if(ScrollBarVerticalVisibility is ScrollBarVisibility.Auto or ScrollBarVisibility.Hidden)
            {
                ScrollBarVerticalBackground.IsVisible = false;
                ContentPresenter.Padding = new Thickness(0, 0, 0, ContentPresenter.Padding.Bottom);
            }
            _scrollVerticalOffset = 0;
        }
        else
        {
            if(ScrollBarVerticalVisibility is ScrollBarVisibility.Auto or ScrollBarVisibility.Always)
            {
                ScrollBarVerticalBackground.IsVisible = true;
                ContentPresenter.Padding = new Thickness(0, 0, ScrollBarSize, ContentPresenter.Padding.Bottom);
            }
        
            float viewRatio = viewportHeight / totalContentHeight;
            ScrollBarVerticalThumb.Height = (int)(viewportHeight * viewRatio);
        
            ScrollBarVerticalThumb.Anchors = ControlAnchors.AnchorCenterHorizontal | ControlAnchors.AnchorTop;
        
            float maxThumbTravel = viewportHeight - ScrollBarVerticalThumb.Height - (ScrollBarPadding * 2);
            ScrollBarVerticalThumb.Y = (int)(_scrollVerticalValue * maxThumbTravel);
        
            _scrollVerticalOffset = _scrollVerticalValue * (totalContentHeight - viewportHeight);
        }
    }

    private void UpdateHorizontalScroll()
    {
        float viewportWidth = ContentPresenter.GlobalsBounds.Width;
        float totalContentWidth = Content.DesiredSize.Width;

        if (totalContentWidth <= viewportWidth)
        {
            if(ScrollBarVerticalVisibility is ScrollBarVisibility.Auto or ScrollBarVisibility.Hidden)
            {
                ScrollBarHorizontalBackground.IsVisible = false;
                ContentPresenter.Padding = new Thickness(0, 0, ContentPresenter.Padding.Right, 0);
            }
            _scrollHorizontalOffset = 0;
        }
        else
        {
            if(ScrollBarVerticalVisibility is ScrollBarVisibility.Auto or ScrollBarVisibility.Always)
            {
                ScrollBarHorizontalBackground.IsVisible = true;
                ContentPresenter.Padding = new Thickness(0, 0, ContentPresenter.Padding.Right, ScrollBarSize);
            }
            float viewRatio = viewportWidth / totalContentWidth;
            ScrollBarHorizontalThumb.Width = (int)(viewportWidth * viewRatio);
        
            ScrollBarHorizontalThumb.Anchors = ControlAnchors.AnchorCenterVertical | ControlAnchors.AnchorLeft;
        
            float maxThumbTravel = viewportWidth - ScrollBarHorizontalThumb.Width - (ScrollBarPadding * 2);
            ScrollBarHorizontalThumb.X = (int)(_scrollHorizontalValue * maxThumbTravel);
        
            _scrollHorizontalOffset = _scrollHorizontalValue * (totalContentWidth - viewportWidth);
        }
    }
}