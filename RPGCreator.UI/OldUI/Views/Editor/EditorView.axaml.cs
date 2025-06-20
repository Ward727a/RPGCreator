#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RPGCreator.MonoGame;
using RPGCreator.UI.OLD.ViewModels._Editor;
using RPGCreator.UI.OLD.Views.Editor.Windows;
using System;
using System.Drawing;
using System.Linq;

namespace RPGCreator.UI.OLD.Views.Editor;

public partial class EditorView : UserControl
{


    private Avalonia.Point _lastPoint;
    private bool _IsDragging = false;

    public EditorGame EditorGame { get; set; }

    public EditorView()
    {
        InitializeComponent();

        if (GameControl.Game is EditorGame)
            EditorGame = (EditorGame)GameControl.Game;
        else
            throw new System.Exception("Game preview is not of 'EditorGame' class.");

        //EditorGame._events.RTPDraw += EditorView_OnDraw;
    }

    private void EditorView_OnDraw(object? sender, System.EventArgs e)
    {
        if (!GameControl.IsLoaded)
            return;

        // This allow to "resize" the "interactable" game window to the "visual" game rectangle.
        // Without this, thing like button aren't alignated to their visual representation. So when you put the mouse on them
        // It's doing nothing, but with your mouse a little on the bottom left of those button it work.
        var window = this.VisualRoot as Window;
        if (window != null)
        {
            var position = (GameControl.TransformToVisual(window)?.Transform(new Avalonia.Point(0, 0))).GetValueOrDefault();
            var windowPositionX = window.Position.X + 8;
            var windowPositionY = window.Position.Y + 30;

            Microsoft.Xna.Framework.Point newEditorPosition = new Microsoft.Xna.Framework.Point(windowPositionX + (int)position.X, windowPositionY + (int)position.Y);

            if (EditorGame.Window.Position != newEditorPosition)
                EditorGame.Window.Position = newEditorPosition;
            
            int newEditorWidth = (int)GameControl.Bounds.Width;
            int newEditorHeight = (int)GameControl.Bounds.Height;

            int currentWidth = EditorGame._graphics.PreferredBackBufferWidth;
            int currentHeight = EditorGame._graphics.PreferredBackBufferHeight;

            if (currentWidth != newEditorWidth || currentHeight != newEditorHeight)
            {
                if (currentWidth != newEditorWidth)
                    EditorGame._graphics.PreferredBackBufferWidth = newEditorWidth;
                if (currentHeight != newEditorHeight)
                    EditorGame._graphics.PreferredBackBufferHeight = newEditorHeight;
                EditorGame._graphics.ApplyChanges();
                //EditorGame.OnNewPreviewSize();
            }
        }

        if(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            if(desktopLifetime.Windows.Where(w => w.IsActive).Any())
            {
                //EditorGame.IsActive = true;
            } else
            {
                //EditorGame.IsActive = false;
            }
        }
    }

    private void Binding(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
    }

    private void Canvas_PointerPressed_1(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if(e.GetCurrentPoint((ScrollViewer)sender).Properties.IsRightButtonPressed)
        {
            _IsDragging = true;
            _lastPoint = e.GetPosition(TilesetImage);
            return;
        }
        if (!e.GetCurrentPoint((ScrollViewer)sender).Properties.IsLeftButtonPressed)
            return;

        float tileSize = ((EditorViewModel?)DataContext)?.SelectionSquareSize ?? 16;
        var point = ((EditorViewModel)DataContext).RenderPosition.Value.Transform(e.GetPosition((Canvas)TilesetCanvas));
        int tileX = (int)(point.X / tileSize);
        int tileY = (int)(point.Y / tileSize);

        // Déplacer le rectangle rouge de sélection
        Canvas.SetLeft(SelectionRectangle, tileX * tileSize);
        Canvas.SetTop(SelectionRectangle, tileY * tileSize);
        SelectionRectangle.IsVisible = true;

        TestText.Text = point.ToString();

        // Stocker la sélection (peut être utilisée pour récupérer la tile choisie)
        //EditorViewModel.EditorGame.SourceRectUV = new Microsoft.Xna.Framework.Rectangle((int)(tileX * tileSize), (int)(tileY * tileSize), (int)tileSize, (int)tileSize);
    }

    private void Canvas_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {

        if(_IsDragging)
        {
            var currentPoint = e.GetPosition((Canvas)sender!);

            var offset = currentPoint - _lastPoint;

            TranslateTransform transform = (TranslateTransform)((EditorViewModel)DataContext).RenderPosition;

            if (offset.X > 2 || offset.X < -2 || offset.Y > 2 || offset.Y < -2)
            {
                var x = double.Clamp(offset.X, -1, 1);
                var y = double.Clamp(offset.Y, -1, 1);
                transform.X += (int)(x*16);
                transform.Y += (int)(y*16);

                ((EditorViewModel)DataContext).RenderPosition = transform;

                _lastPoint = currentPoint;
            }
        }

    }

    private void Canvas_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        if(_IsDragging)
            _IsDragging = false;
    }

    private void Image_PointerPressed_2(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
    }

    private void Rectangle_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        float tileSize = ((EditorViewModel?)DataContext)?.SelectionSquareSize ?? 16;
        Avalonia.Point point = new Avalonia.Point(Canvas.GetLeft(SelectionRectangle) + (tileSize / 1.1)/2, Canvas.GetTop(SelectionRectangle) + (tileSize / 1.1)/2);
        int tileX = (int)(point.X / tileSize);
        int tileY = (int)(point.Y / tileSize);

        // Déplacer le rectangle rouge de sélection
        Canvas.SetLeft(SelectionRectangle, tileX * tileSize);
        Canvas.SetTop(SelectionRectangle, tileY * tileSize);
        SelectionRectangle.IsVisible = true;

        TestText.Text = point.ToString();

        // Stocker la sélection (peut être utilisée pour récupérer la tile choisie)
        //EditorViewModel.EditorGame.SourceRectUV = new Microsoft.Xna.Framework.Rectangle((int)(tileX * tileSize), (int)(tileY * tileSize), (int)tileSize, (int)tileSize);
    }

    private void MonoGameControl_PointerEntered_1(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        //EditorViewModel.EditorGame.InsideEditorBox = true;
    }

    private void MonoGameControl_PointerExited_2(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        //EditorViewModel.EditorGame.InsideEditorBox = false;
    }

    private void OpenSettings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new ProjectSettings().ShowDialog((Window)this.Parent);
    }
}